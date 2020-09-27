using System;
using System.Linq;
using System.IO;
using System.IO.Ports;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using GalaSoft.MvvmLight.CommandWpf;
using EspBrowser.Properties;
using NodeMcuUploader;

namespace EspBrowser.ViewModels
{
  public class TerminalViewModel : AnchorableViewModel
  {
    protected const string CRLF       = "\r\n";
    protected const string CONNECT    = "Connect";
    protected const string DISCONNECT = "Disconnect";

    protected Uploader uploader;
    protected string port_name;
    protected int baud_rate;
    protected int timeout;
    protected bool echo_enabled = true;
    protected bool cts_holding;
    protected bool dtr_enabled;
    protected bool rts_enabled;
    protected int send_history_length;
    protected bool is_connected;
    protected bool is_enabled;
    protected bool autoscroll_to_end = true;
    protected ImageSource connect_image_source;
    protected string connect_tooltip = CONNECT;
    protected TextDocument document;
    protected string send_text = String.Empty;

    public string PortName
    {
      get { return this.port_name; }
      set { Set(ref this.port_name, value); }
    }

    public int BaudRate
    {
      get { return this.baud_rate; }
      set { Set(ref this.baud_rate, value); }
    }

    public int Timeout
    {
      get { return this.timeout; }
      set
      {
        if(Set(ref this.timeout, value))
        {
          if(this.uploader != null)
            this.uploader.SetTimeout(value);
        }
      }
    }

    public bool EchoEnabled
    {
      get { return this.echo_enabled; }
      set { Set(ref this.echo_enabled, value); }
    }

    public bool CtsHolding
    {
      get { return this.cts_holding; }
      set { Set(ref this.cts_holding, value); }
    }

    public bool DtrEnabled
    {
      get { return this.dtr_enabled; }
      set { Set(ref this.dtr_enabled, value); }
    }

    public bool RtsEnabled
    {
      get { return this.rts_enabled; }
      set { Set(ref this.rts_enabled, value); }
    }

    public int SendHistoryLength
    {
      get { return this.send_history_length; }
      set { Set(ref this.send_history_length, value); }
    }

    public bool IsConnected
    {
      get { return this.is_connected; }
      set
      {
        if(Set(ref this.is_connected, value))
        {
          this.IsDisconnected     = !value;
          this.IsEnabled          = value;
          this.ConnectImageSource = WindowUtils.GetImage(value ? "Image_Disconnect" : "Image_Connect");
          this.ConnectToolTip     = (value ? DISCONNECT : CONNECT);
          this.CtsHolding         = (this.uploader?.CtsHolding ?? false);
          RaisePropertyChanged(() => IsDisconnected);
          RaisePropertyChanged(() => ConnectImageSource);
        }
      }
    }

    public bool IsDisconnected { get; protected set; }

    public bool IsEnabled
    {
      get { return this.is_enabled; }
      set
      {
        if(Set(ref this.is_enabled, value))
        {
          this.ConnectCommand.RaiseCanExecuteChanged();
          this.SendCommand.RaiseCanExecuteChanged();
          this.RefreshPortNamesCommand.RaiseCanExecuteChanged();
          this.SoftResetCommand.RaiseCanExecuteChanged();
          this.HardResetCommand.RaiseCanExecuteChanged();
          this.ChipIdCommand.RaiseCanExecuteChanged();
          this.HeapCommand.RaiseCanExecuteChanged();
          this.InfoCommand.RaiseCanExecuteChanged();
        }
      }
    }

    public bool AutoScrollToEnd
    {
      get { return this.autoscroll_to_end; }
      set { Set(ref this.autoscroll_to_end, value); }
    }

    public ImageSource ConnectImageSource
    {
      get { return this.connect_image_source; }
      set { Set(ref this.connect_image_source, value); }
    }

    public string ConnectToolTip
    {
      get { return this.connect_tooltip; }
      set { Set(ref this.connect_tooltip, value); }
    }

    public TextDocument Document
    {
      get { return this.document; }
      set { Set(ref this.document, value); }
    }

    public string SendText
    {
      get { return this.send_text; }
      set { Set(ref this.send_text, value); }
    }

    public ObservableCollection<string> SendHistory { get; protected set; }

    public ObservableCollection<string> PortNames { get; protected set; }

    public int[] BaudRates { get; protected set; }

    public RelayCommand RefreshPortNamesCommand { get; protected set; }
    public RelayCommand ConnectCommand { get; protected set; }
    public RelayCommand SendCommand { get; protected set; }
    public RelayCommand ClearAllCommand { get; protected set; }
    public RelayCommand SoftResetCommand { get; protected set; }
    public RelayCommand HardResetCommand { get; protected set; }
    public RelayCommand ChipIdCommand { get; protected set; }
    public RelayCommand HeapCommand { get; protected set; }
    public AwaitableRelayCommand InfoCommand { get; protected set; }

    public TerminalViewModel(MainWindowViewModel parent, string content_id, string title, string tooltip, string image_name)
      : base(parent, content_id, title, tooltip, image_name)
    {
      this.Document                = new TextDocument();
      this.PortNames               = new ObservableCollection<string>();
      this.RefreshPortNamesCommand = new RelayCommand(RefreshPortNamesExecute, RefreshPortNamesCanExecute);
      this.ConnectCommand          = new RelayCommand(ConnectExecute, ConnectCanExecute);
      this.SendCommand             = new RelayCommand(SendExecute, SendCanExecute);
      this.ClearAllCommand         = new RelayCommand(ClearAllExecute);
      this.SoftResetCommand        = new RelayCommand(SoftResetExecute, SoftResetCanExecute);
      this.HardResetCommand        = new RelayCommand(HardResetExecute, HardResetCanExecute);
      this.ChipIdCommand           = new RelayCommand(ChipIdExecute, ChipIdCanExecute);
      this.HeapCommand             = new RelayCommand(HeapExecute, HeapCanExecute);
      this.InfoCommand             = new AwaitableRelayCommand(InfoExecute, InfoCanExecute);

      // Refresh port names
      RefreshPortNamesExecute();
      if(this.PortNames.Count > 0)
      {
        if(this.PortNames.Contains(Settings.Default.PortName))
          this.PortName = Settings.Default.PortName;
        else
          this.PortName = this.PortNames[0];
      }

      // Refresh baud rates
      this.BaudRates = new int[] { 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200, 230400 };
      this.BaudRate  = Settings.Default.BaudRate;

      // Timeout
      this.Timeout = Settings.Default.Timeout;

      // Send command history
      this.SendHistoryLength = Settings.Default.SendHistoryLength;
      this.SendHistory = new ObservableCollection<string>();
      if(Settings.Default.SendHistory != null)
        this.SendHistory.AddRange(Settings.Default.SendHistory.Cast<string>());

      // Image of connect button
      this.ConnectImageSource = WindowUtils.GetImage("Image_Connect");

      this.IsDisconnected = true;
    }

    protected void SendSimple(string command, bool add_to_history)
    {
      if(String.IsNullOrWhiteSpace(command) || !CheckConnection())
        return;

      this.IsEnabled = false;

      try
      {
        this.uploader.WriteLine(command);
      }
      catch(Exception ex)
      {
        log.Error(ex);
        this.parent.ShowWarning(ex.Message, "Communication Error");
      }
      finally
      {
        this.IsEnabled = true;
      }

      if(add_to_history)
      {
        // Clear command combobox
        this.SendText = String.Empty;

        // Add command to history
        if(this.SendHistoryLength > 0)
        {
          if(this.SendHistory.Count == this.SendHistoryLength)
            this.SendHistory.RemoveAt(this.SendHistoryLength - 1);

          this.SendHistory.Remove(command);
          this.SendHistory.Insert(0, command);
        }
      }
    }

    public async Task<string> Format()
    {
      if(!CheckConnection())
        return null;

      string result;
      this.IsEnabled = false;
      this.parent.SetStatusWorking($"Formatting SPIFFS, wait please ...", "Image_Event");
      this.parent.InitializeProgressBar(true);
      DettachSerialEvents();

      try
      {
        result = await Task.Run(() => { return this.uploader.Format(); });
      }
      finally
      {
        this.IsEnabled = true;
        this.parent.RestoreStatusWorking();
        AttachSerialEvents();
      }

      return result;
    }

    public async Task<FsInfo> ReadFileList()
    {
      if(!CheckConnection())
        return null;

      this.IsEnabled = false;
      this.parent.SetStatusWorking($"Reading file list ...", "Image_Refresh");
      this.parent.InitializeProgressBar(true);
      DettachSerialEvents();
      FsInfo result = new FsInfo();

      try
      {
        result = await Task.Run(() => { return this.uploader.ReadFileList(); });
      }
      finally
      {
        this.IsEnabled = true;
        this.parent.RestoreStatusWorking();
        AttachSerialEvents();
      }

      return result;
    }

    public void PrintFile(string filename)
    {
      if(!CheckConnection())
        return;

      SendSimple(String.Format(LuaCode.PRINT_FILE, filename), false);
    }

    public void DoFile(string filename)
    {
      if(!CheckConnection())
        return;

      SendSimple(String.Format(LuaCode.DO_FILE, filename), false);
    }

    public async Task CompileFile(string filename)
    {
      if(!CheckConnection())
        return;

      this.IsEnabled = false;
      this.parent.SetStatusWorking($"Compiling {filename} ...", "Image_BuildSolution");
      this.parent.InitializeProgressBar(true);
      DettachSerialEvents();

      try
      {
        await Task.Run(() => { this.uploader.Compile(filename); });
      }
      finally
      {
        this.IsEnabled = true;
        this.parent.RestoreStatusWorking();
        AttachSerialEvents();
      }
    }

    public async Task RenameFile(string filename, string new_filename)
    {
      if(!CheckConnection())
        return;

      this.IsEnabled = false;
      DettachSerialEvents();

      try
      {
        await Task.Run(() => { this.uploader.Rename(filename, new_filename); });
      }
      finally
      {
        this.IsEnabled = true;
        AttachSerialEvents();
      }
    }

    public async Task DeleteFile(string filename)
    {
      if(!CheckConnection())
        return;

      this.IsEnabled = false;
      DettachSerialEvents();

      try
      {
        await Task.Run(() => { this.uploader.Remove(filename); });
      }
      finally
      {
        this.IsEnabled = true;
        AttachSerialEvents();
      }
    }

    public async Task<bool> ReadFile(string filename, int size, Stream stream, CancellationToken token)
    {
      if(!CheckConnection())
        return false;

      this.IsEnabled = false;
      this.parent.IsCancelEnabled = true;
      this.parent.SetStatusWorking($"Downloading {filename} ...", "Image_DownloadFile");
      this.parent.InitializeProgressBar(false);
      DettachSerialEvents();

      var report_progress = new Progress<int>((value) => { this.parent.SetProgressBarValue(value); });

      try
      {
        await Task.Run(() => { this.uploader.ReadFile(filename, size, stream, report_progress, token); }, token);
        return !token.IsCancellationRequested;
      }
      finally
      {
        this.IsEnabled = true;
        this.parent.IsCancelEnabled = false;
        this.parent.RestoreStatusWorking(); 
        AttachSerialEvents();
      }
    }

    public async Task<bool> ExistsFile(string filename)
    {
      if(!CheckConnection())
        return false;

      this.IsEnabled = false;
      DettachSerialEvents();

      try
      {
        bool result = await Task.Run(() => { return this.uploader.ExistsFile(filename); });
        return result;
      }
      finally
      {
        this.IsEnabled = true;
        AttachSerialEvents();
      }
    }

    public async Task<bool> WriteFile(string filename, Stream stream, CancellationToken token)
    {
      if(!CheckConnection())
        return false;

      this.IsEnabled = false;
      this.parent.IsCancelEnabled = true;
      this.parent.SetStatusWorking($"Uploading {filename} ...", "Image_UploadFile");
      this.parent.InitializeProgressBar(false);
      DettachSerialEvents();

      var report_progress = new Progress<int>((value) => { this.parent.SetProgressBarValue(value); });

      try
      {
        await Task.Run(() => { this.uploader.WriteFile(stream, filename, report_progress, token); }, token);
        return !token.IsCancellationRequested;
      }
      finally
      {
        this.IsEnabled = true;
        this.parent.IsCancelEnabled = false;
        this.parent.RestoreStatusWorking();
        AttachSerialEvents();
      }
    }

    private bool CheckConnection()
    {
      if(this.uploader == null)
      {
        this.parent.ShowWarning("Serial port is closed", "Connection error");
        return false;
      }

      return true;
    }

    #region Commands
    protected bool RefreshPortNamesCanExecute()
    {
      return this.IsDisconnected;
    }

    protected void RefreshPortNamesExecute()
    {
      string prev_port_name = this.PortName;

      try
      {
        this.PortNames.Clear();
        this.PortNames.AddRange(SerialPort.GetPortNames());
      }
      catch(Exception ex)
      {
        log.Error(ex);
      }

      if(this.PortNames.Count > 0)
      {
        if(this.PortNames.Contains(Settings.Default.PortName))
          this.PortName = Settings.Default.PortName;
        else if(this.PortNames.Contains(prev_port_name))
          this.PortName = prev_port_name;
        else
          this.PortName = this.PortNames[0];
      }
      else
      {
        this.PortName = null;
      }
    }

    protected bool ConnectCanExecute()
    {
      if(this.IsConnected && !this.IsEnabled)
        return false;

      return true;
    }

    protected void ConnectExecute()
    {
      if(this.IsConnected)
      {
        CloseSerial();
        this.IsConnected = false;
      }
      else
      {
        if(String.IsNullOrWhiteSpace(this.PortName))
        {
          this.parent.ShowWarning("Port name is not specified", "Connection error");
          return;
        }

        try
        {
          this.uploader = new Uploader(this.PortName, this.BaudRate, this.Timeout, this.EchoEnabled);
          this.IsConnected = true;
        }
        catch(Exception ex)
        {
          log.Error(ex);
          throw new ApplicationException("Error opening port " + this.PortName, ex);
        }
        finally
        {
          AttachSerialEvents();
        }
      }
    }

    protected bool SendCanExecute()
    {
      return this.IsEnabled;
    }

    protected void SendExecute()
    {
      SendSimple(this.SendText, true);
    }

    protected void ClearAllExecute()
    {
      this.Document.Text = String.Empty;
    }

    protected bool SoftResetCanExecute()
    {
      return this.IsEnabled;
    }

    protected void SoftResetExecute()
    {
      this.uploader.SoftReset();
    }

    protected bool HardResetCanExecute()
    {
      return this.IsEnabled;
    }

    protected void HardResetExecute()
    {
      this.uploader.HardReset();
    }

    protected bool ChipIdCanExecute()
    {
      return this.IsEnabled;
    }

    protected void ChipIdExecute()
    {
      SendSimple(LuaCode.PRINT_CHIPID, false);
    }

    protected bool HeapCanExecute()
    {
      return this.IsEnabled;
    }

    protected void HeapExecute()
    {
      SendSimple(LuaCode.PRINT_HEAP, false);
    }

    protected bool InfoCanExecute()
    {
      return this.IsEnabled;
    }

    protected async Task InfoExecute()
    {
      if(!CheckConnection())
        return;

      this.IsEnabled = false;
      this.parent.SetStatusWorking($"Reading node info ...", "Image_Refresh");
      this.parent.InitializeProgressBar(true);
      DettachSerialEvents();
      NodeInfo info;

      try
      {
        info = await Task.Run(() => { return this.uploader.ReadInfo(); });
      }
      finally
      {
        this.IsEnabled = true;
        this.parent.RestoreStatusWorking();
        AttachSerialEvents();
      }

      if(info != null)
      {
        DialogNodeInfoViewModel vm = new DialogNodeInfoViewModel("NodeMCU Info", info);
        this.parent.DialogService.ShowDialog(this.parent, vm);
      }
    }
    #endregion

    protected void AttachSerialEvents()
    {
      this.uploader.AttachEvents(Serial_DataReceived, Serial_ErrorReceived);
    }

    protected void DettachSerialEvents()
    {
      this.uploader.DettachEvents(Serial_DataReceived, Serial_ErrorReceived);
    }

    protected void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
      SerialPort sp = (SerialPort)sender;
      string indata = sp.ReadExisting();
      App.Current.Dispatcher.Invoke(() => { TerminalAppendText(indata); });
    }

    protected void Serial_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
    {
      SerialPort sp = (SerialPort)sender;
      string indata = sp.ReadExisting();
      App.Current.Dispatcher.Invoke(() => { TerminalAppendText(indata); });
    }

    protected void TerminalAppendText(string text)
    {
      this.Document.Text += text;
    }

    public void CloseSerial()
    {
      try
      {
        if(this.uploader != null)
        {
          DettachSerialEvents();
          this.uploader.Dispose();
          this.uploader = null;
        }
      }
      catch(Exception ex)
      {
        log.Error(ex);
      }
    }
  }
}
