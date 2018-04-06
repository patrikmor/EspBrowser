using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using EspBrowser.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MvvmDialogs;

namespace EspBrowser.ViewModels
{
  public class MainWindowViewModel : ViewModelBase
  {
    protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    protected const string READY = "Ready";

    protected static int NewDocumentCounter = 1;
    protected DocumentViewModel active_document;
    protected CancellationTokenSource token_source;
    protected int recent_files_length;
    protected bool is_recent_files_visible;
    protected string title;
    protected string status_connection_text;
    protected ImageSource status_connection_image;
    protected string status_working_text;
    protected ImageSource status_working_image;
    protected bool is_progressbar_visible;
    protected bool is_progressbar_value_visible;
    protected bool is_progressbar_indeterminate = true;
    protected int progressbar_value;
    protected bool is_cancel_enabled;

    public IDialogService DialogService { get; protected set; }

    public ObservableCollection<DocumentViewModel> Documents { get; private set; }

    public ObservableCollection<AnchorableViewModel> Anchorables { get; private set; }

    public ObservableCollection<string> RecentFiles { get; private set; }

    public int RecentFilesLength
    {
      get { return this.recent_files_length; }
      set { Set(ref this.recent_files_length, value); }
    }

    public bool IsRecentFilesVisible
    {
      get { return this.is_recent_files_visible; }
      set { Set(ref this.is_recent_files_visible, value); }
    }

    public string Title
    {
      get { return title; }
      set { Set(ref this.title, value); }
    }

    public string StatusConnectionText
    {
      get { return this.status_connection_text; }
      set { Set(ref this.status_connection_text, value); }
    }

    public ImageSource StatusConnectionImage
    {
      get { return this.status_connection_image; }
      set { Set(ref this.status_connection_image, value); }
    }

    public string StatusWorkingText
    {
      get { return this.status_working_text; }
      set { Set(ref this.status_working_text, value); }
    }

    public ImageSource StatusWorkingImage
    {
      get { return this.status_working_image; }
      set { Set(ref this.status_working_image, value); }
    }

    public bool IsProgressBarVisible
    {
      get { return this.is_progressbar_visible; }
      set
      {
        if(Set(ref this.is_progressbar_visible, value))
          this.IsProgressBarValueVisible = (value && !this.IsProgressBarIndeterminate);
      }
    }

    public bool IsProgressBarValueVisible
    {
      get { return this.is_progressbar_value_visible; }
      set { Set(ref this.is_progressbar_value_visible, value); }
    }

    public bool IsProgressBarIndeterminate
    {
      get { return this.is_progressbar_indeterminate; }
      set
      {
        if(Set(ref this.is_progressbar_indeterminate, value))
          this.IsProgressBarValueVisible = (!value && this.IsProgressBarVisible);
      }
    }

    public int ProgressBarValue
    {
      get { return this.progressbar_value; }
      set { Set(ref this.progressbar_value, value); }
    }

    public bool IsCancelEnabled
    {
      get { return this.is_cancel_enabled; }
      set
      {
        if(Set(ref this.is_cancel_enabled, value))
          this.CancelCommand.RaiseCanExecuteChanged();
      }
    }

    public DocumentViewModel ActiveDocument
    {
      get { return this.active_document; }
      set
      {
        if(Set(ref this.active_document, value))
        {
          this.SaveFileCommand.RaiseCanExecuteChanged();
          this.SaveFileAsCommand.RaiseCanExecuteChanged();
          this.SaveAllCommand.RaiseCanExecuteChanged();
          this.UploadFileCommand.RaiseCanExecuteChanged();
          this.CloseFileCommand.RaiseCanExecuteChanged();
          this.LineNumbersCommand.RaiseCanExecuteChanged();
          this.WordWrapCommand.RaiseCanExecuteChanged();
        }
      }
    }

    public TerminalViewModel Terminal { get; private set; }
    public SpiffsViewModel Spiffs { get; private set; }

    public RelayCommand<CancelEventArgs> WindowClosingCommand { get; private set; }
    public RelayCommand NewFileCommand { get; private set; }
    public RelayCommand OpenFileCommand { get; private set; }
    public RelayCommand SaveFileCommand { get; private set; }
    public RelayCommand SaveFileAsCommand { get; private set; }
    public RelayCommand SaveAllCommand { get; private set; }
    public AwaitableRelayCommand UploadFileCommand { get; private set; }
    public RelayCommand CloseFileCommand { get; private set; }
    public RelayCommand ExitCommand { get; private set; }
    public RelayCommand ShowTerminalCommand { get; private set; }
    public RelayCommand ShowSpiffsCommand { get; private set; }
    public RelayCommand<string> RecentFileCommand { get; private set; }
    public RelayCommand LineNumbersCommand { get; private set; }
    public RelayCommand WordWrapCommand { get; private set; }
    public RelayCommand CancelCommand { get; private set; }
    public RelayCommand SettingsCommand { get; private set; }
    public RelayCommand GitHubCommand { get; private set; }
    public RelayCommand AboutCommand { get; private set; }
    public RelayCommand<DocumentViewModel> WindowCommand { get; private set; }

    public MainWindowViewModel(IDialogService dialog_service)
    {
      this.DialogService        = dialog_service;
      this.Documents            = new ObservableCollection<DocumentViewModel>();
      this.Anchorables          = new ObservableCollection<AnchorableViewModel>();
      this.WindowClosingCommand = new RelayCommand<CancelEventArgs>(WindowClosingExecute);
      this.NewFileCommand       = new RelayCommand(NewFileExecute);
      this.OpenFileCommand      = new RelayCommand(OpenFileExecute);
      this.SaveFileCommand      = new RelayCommand(SaveFileExecute, SaveFileCanExecute);
      this.SaveFileAsCommand    = new RelayCommand(SaveFileAsExecute, SaveFileAsCanExecute);
      this.SaveAllCommand       = new RelayCommand(SaveAllExecute, SaveAllCanExecute);
      this.UploadFileCommand    = new AwaitableRelayCommand(UploadFileExecute, UploadFileCanExecute);
      this.CloseFileCommand     = new RelayCommand(CloseFileExecute, CloseFileCanExecute);
      this.ExitCommand          = new RelayCommand(ExitExecute);
      this.ShowTerminalCommand  = new RelayCommand(ShowTerminalExecute);
      this.ShowSpiffsCommand    = new RelayCommand(ShowSpiffsExecute);
      this.RecentFileCommand    = new RelayCommand<string>(RecentFileExecute);
      this.LineNumbersCommand   = new RelayCommand(LineNumbersExecute, LineNumbersCanExecute);
      this.WordWrapCommand      = new RelayCommand(WordWrapExecute, WordWrapCanExecute);
      this.CancelCommand        = new RelayCommand(CancelExecute, CancelCanExecute);
      this.SettingsCommand      = new RelayCommand(SettingsExecute);
      this.GitHubCommand        = new RelayCommand(GitHubExecute);
      this.AboutCommand         = new RelayCommand(AboutExecute);
      this.WindowCommand        = new RelayCommand<DocumentViewModel>(WindowExecute);

      // Window title
      this.Title = "ESP Browser " + GetVersion();

      // Recent files history
      this.RecentFilesLength = Settings.Default.RecentFilesLength;
      this.RecentFiles = new ObservableCollection<string>();
      if(Settings.Default.RecentFiles != null)
      {
        this.RecentFiles.AddRange(Settings.Default.RecentFiles.Cast<string>());
        this.IsRecentFilesVisible = true;
      }

      this.RecentFiles.CollectionChanged += RecentFiles_CollectionChanged;

      // Add terminal window
      this.Terminal = new TerminalViewModel(this, "terminal", "Terminal", "Serial terminal", "Image_SerialPort");
      this.Anchorables.Add(this.Terminal);

      // Register to PropertyChanged event of TerminalViewModel
      this.Terminal.PropertyChanged += Terminal_PropertyChanged;

      // Add SPIFFS window
      this.Spiffs = new SpiffsViewModel(this, "spiffs", "SPIFFS Explorer", "SPIFFS Explorer", "Image_FileGroup");
      this.Anchorables.Add(this.Spiffs);

      // Add new document
      NewFileExecute();

      // Set statusbar text and image
      RefreshStatusConnection();
      this.StatusWorkingText  = READY;
      this.StatusWorkingImage = WindowUtils.GetImage("Image_DotLarge");
    }

    private void RecentFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      this.IsRecentFilesVisible = (this.RecentFiles.Count > 0);
    }

    private void Terminal_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if(e.PropertyName == nameof(TerminalViewModel.IsConnected))
      {
        RefreshStatusConnection();
      }
      else if(e.PropertyName == nameof(TerminalViewModel.IsEnabled))
      {
        this.UploadFileCommand.RaiseCanExecuteChanged();
      }
    }

    public void NewFile(string filename, string text_content)
    {
      DocumentViewModel document = new DocumentViewModel(this, filename, String.Empty, null, text_content);
      this.Documents.Add(document);
      this.ActiveDocument = document;
    }

    public void OpenFile(string path)
    {
      // Check for already opened file with the same path
      var document = this.Documents.FirstOrDefault(c => !String.IsNullOrEmpty(c.Path) && Path.GetFullPath(c.Path).Equals(Path.GetFullPath(path)));
      if(document != null)
      {
        this.ActiveDocument = document;
        return;
      }

      try
      {
        string content = File.ReadAllText(path);
        document = new DocumentViewModel(this, Path.GetFileName(path), path, null, content);
        this.Documents.Add(document);
        this.ActiveDocument = document;

        // Add to recent files history
        if(this.RecentFilesLength > 0)
        {
          if(this.RecentFiles.Contains(path))
            this.RecentFiles.Remove(path);
          else if(this.RecentFiles.Count == this.RecentFilesLength)
            this.RecentFiles.RemoveAt(this.RecentFiles.Count - 1);

          this.RecentFiles.Insert(0, path);
        }
      }
      catch(Exception ex)
      {
        log.Error(ex);
        throw new ApplicationException("Error opening file " + path, ex);
      }
    }

    public void InitializeProgressBar(bool indeterminate)
    {
      this.ProgressBarValue           = 0;
      this.IsProgressBarIndeterminate = indeterminate;
      this.IsProgressBarVisible       = true;
    }

    public void SetProgressBarValue(int value)
    {
      if(value < 0)
        value = 0;
      else if(value > 100)
        value = 100;

      this.ProgressBarValue = value;
    }

    public void RefreshStatusConnection()
    {
      this.StatusConnectionText  = (this.Terminal.IsConnected ? $"Connected to {this.Terminal.PortName} at {this.Terminal.BaudRate}" : "Disconnected");
      this.StatusConnectionImage = WindowUtils.GetImage(this.Terminal.IsConnected ? "Image_Plugged" : "Image_Unplugged");
    }

    public void SetStatusWorking(string text, string image_name)
    {
      this.StatusWorkingText  = text;
      this.StatusWorkingImage = WindowUtils.GetImage(image_name);
    }

    public void RestoreStatusWorking()
    {
      SetStatusWorking(READY, "Image_DotLarge");
      this.IsProgressBarVisible = false;
    }

    public CancellationToken CreateCancellationToken()
    {
      this.token_source = new CancellationTokenSource();
      return this.token_source.Token;
    }

    public string GetVersion()
    {
      return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }

    #region Commands
    protected void WindowClosingExecute(CancelEventArgs e)
    {
      // Save user settings
      try
      {
        Settings.Default.PortName = this.Terminal.PortName;
        Settings.Default.BaudRate = this.Terminal.BaudRate;
        Settings.Default.Timeout  = this.Terminal.Timeout;

        Settings.Default.RecentFilesLength = this.RecentFilesLength;
        Settings.Default.RecentFiles = new System.Collections.Specialized.StringCollection();
        Settings.Default.RecentFiles.AddRange(this.RecentFiles.ToArray());

        Settings.Default.SendHistoryLength = this.Terminal.SendHistoryLength;
        Settings.Default.SendHistory = new System.Collections.Specialized.StringCollection();
        Settings.Default.SendHistory.AddRange(this.Terminal.SendHistory.ToArray());
        Settings.Default.Save();
      }
      catch(Exception ex)
      {
        log.Error(ex);
      }

      // Try to close all documents
      var documents = new List<DocumentViewModel>(this.Documents);
      foreach(var document in documents)
      {
        document.Close();
      }

      // If there are still opened documents then cancel application closing
      if(this.Documents.Count > 0)
        e.Cancel = true;

      // Close serial port
      if(!e.Cancel)
        this.Terminal.CloseSerial();
    }

    protected void NewFileExecute()
    {
      string filename = Resources.Document_DefaultName + NewDocumentCounter + Resources.Document_DefaultExtension;
      NewFile(filename, String.Empty);
      NewDocumentCounter++;
    }

    protected void OpenFileExecute()
    {
      var settings = new MvvmDialogs.FrameworkDialogs.OpenFile.OpenFileDialogSettings()
      {
        Filter          = Resources.Document_DefaultFilterLua,
        DefaultExt      = Resources.Document_DefaultExtension,
        CheckFileExists = true,
        CheckPathExists = true,
        Title           = "Open file"
      };

      if(this.DialogService.ShowOpenFileDialog(this, settings) == true)
      {
        OpenFile(settings.FileName);
      }
    }

    protected bool SaveFileCanExecute()
    {
      return (this.ActiveDocument != null);
    }

    protected void SaveFileExecute()
    {
      this.ActiveDocument.Save(false);
    }

    protected bool SaveFileAsCanExecute()
    {
      return (this.ActiveDocument != null);
    }

    protected void SaveFileAsExecute()
    {
      this.ActiveDocument.Save(true);
    }

    protected bool SaveAllCanExecute()
    {
      return (this.ActiveDocument != null);
    }

    protected void SaveAllExecute()
    {
      foreach(var document in this.Documents)
      {
        document.Save(false);
      }
    }

    protected bool UploadFileCanExecute()
    {
      return (this.ActiveDocument != null && this.Terminal.IsEnabled);
    }

    protected async Task UploadFileExecute()
    {
      // Save modified file
      if(this.ActiveDocument.IsModified)
        this.ActiveDocument.Save(false);

      // Check if file already exists on device
      var filename = this.ActiveDocument.FileName;
      var exists = await this.Terminal.ExistsFile(filename);
      if(exists)
      {
        if(ShowQuestionOkCancel($"File {filename} already exists on device. Overwrite?", "File exists") != System.Windows.MessageBoxResult.OK)
          return;
      }

      // Write file to device
      var token = CreateCancellationToken();
      using(MemoryStream stream = new MemoryStream())
      {
        using(StreamWriter writer = new StreamWriter(stream))
        {
          this.ActiveDocument.Document.WriteTextTo(writer);
          writer.Flush();
          await this.Terminal.WriteFile(filename, stream, token);
        }
      }

      // Refresh files
      await this.Spiffs.Refresh();
    }

    protected bool CloseFileCanExecute()
    {
      return (this.ActiveDocument != null);
    }

    protected void CloseFileExecute()
    {
      this.ActiveDocument.Close();
    }

    protected void ExitExecute()
    {
      App.Current.Shutdown();
    }

    protected void ShowTerminalExecute()
    {
      this.Terminal.Activate();
    }

    protected void ShowSpiffsExecute()
    {
      this.Spiffs.Activate();
    }

    protected void RecentFileExecute(string path)
    {
      if(String.IsNullOrWhiteSpace(path))
        return;

      if(File.Exists(path))
      {
        OpenFile(path);
      }
      else
      {
        ShowWarning($"File {path} does not exist!", "File not found");
        this.RecentFiles.Remove(path);
      }
    }

    protected bool LineNumbersCanExecute()
    {
      return (this.ActiveDocument != null);
    }

    protected void LineNumbersExecute()
    {
      this.ActiveDocument.ShowLineNumbers = !this.ActiveDocument.ShowLineNumbers;
    }

    protected bool WordWrapCanExecute()
    {
      return (this.ActiveDocument != null);
    }

    protected void WordWrapExecute()
    {
      this.ActiveDocument.WordWrap = !this.ActiveDocument.WordWrap;
    }

    protected bool CancelCanExecute()
    {
      return this.IsCancelEnabled;
    }

    protected void CancelExecute()
    {
      this.token_source.Cancel();
    }

    protected void SettingsExecute()
    {
      DialogSettingsViewModel vm = new DialogSettingsViewModel("Settings");
      if(this.DialogService.ShowDialog(this, vm) == true)
      {
        bool save_settings = false;

        // Timeout
        if(vm.Timeout != this.Terminal.Timeout)
        {
          Settings.Default.Timeout = vm.Timeout;
          this.Terminal.Timeout    = vm.Timeout;
          save_settings            = true;
        }

        // SendHistoryLength
        if(vm.SendHistoryLength != this.Terminal.SendHistoryLength)
        {
          Settings.Default.SendHistoryLength = vm.SendHistoryLength;
          this.Terminal.SendHistoryLength = vm.SendHistoryLength;

          // Remove items that exceeded new send history count
          if(this.Terminal.SendHistory.Count > vm.SendHistoryLength)
          {
            for(int i = this.Terminal.SendHistory.Count - 1; i >= vm.SendHistoryLength; i--)
            {
              this.Terminal.SendHistory.RemoveAt(i);
            }
          }

          save_settings = true;
        }

        // RecentFilesLength
        if(vm.RecentFilesLength != this.RecentFilesLength)
        {
          Settings.Default.RecentFilesLength = vm.RecentFilesLength;
          this.RecentFilesLength             = vm.RecentFilesLength;

          // Remove items that exceeded new send history count
          if(this.RecentFiles.Count > vm.RecentFilesLength)
          {
            for(int i = this.RecentFiles.Count - 1; i >= vm.RecentFilesLength; i--)
            {
              this.RecentFiles.RemoveAt(i);
            }
          }

          save_settings = true;
        }

        // SaveLayout
        if(Settings.Default.SaveLayout != vm.SaveLayout)
        {
          Settings.Default.SaveLayout = vm.SaveLayout;
          save_settings              = true;
        }

        // Save user settings
        if(save_settings)
          Settings.Default.Save();
      }
    }

    protected void GitHubExecute()
    {
      string url = Resources.About_GitHubUrl;
      if(!String.IsNullOrWhiteSpace(url))
        System.Diagnostics.Process.Start(url);
    }

    protected void AboutExecute()
    {
      DialogAboutViewModel vm = new DialogAboutViewModel("About ESP Browser", GetVersion());
      this.DialogService.ShowDialog(this, vm);
    }

    protected void WindowExecute(DocumentViewModel document)
    {
      this.ActiveDocument = document;
    }
    #endregion

    #region Dialogs
    public bool? ShowOpenFileDialog(MvvmDialogs.FrameworkDialogs.OpenFile.OpenFileDialogSettings settings)
    {
      return this.DialogService.ShowOpenFileDialog(this, settings);
    }

    public bool? ShowSaveFileDialog(MvvmDialogs.FrameworkDialogs.SaveFile.SaveFileDialogSettings settings)
    {
      return this.DialogService.ShowSaveFileDialog(this, settings);
    }

    public void ShowInfo(string message, string caption)
    {
      this.DialogService.ShowMessageBox(this, message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowWarning(string message, string caption)
    {
      this.DialogService.ShowMessageBox(this, message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public void ShowError(string message, string caption)
    {
      this.DialogService.ShowMessageBox(this, message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public MessageBoxResult ShowQuestionOkCancel(string message, string caption)
    {
      return this.DialogService.ShowMessageBox(this, message, caption, MessageBoxButton.OKCancel, MessageBoxImage.Question);
    }
    #endregion
  }
}
