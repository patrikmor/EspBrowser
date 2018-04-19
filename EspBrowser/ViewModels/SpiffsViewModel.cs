using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.CommandWpf;
using EspBrowser.Properties;

namespace EspBrowser.ViewModels
{
  public class SpiffsViewModel : AnchorableViewModel
  {
    protected const string NA = "n/a";

    protected FileViewModel selected_file;
    protected string total_files_text = NA;
    protected string total_bytes_text = NA;
    protected string used_bytes_text  = NA;
    protected string free_bytes_text  = NA;
    protected string chart_text       = String.Empty;
    protected double chart_value      = 0;

    protected TerminalViewModel Terminal { get; private set; }

    public FileViewModel SelectedFile
    {
      get { return this.selected_file; }
      set
      {
        if(Set(ref this.selected_file, value))
        {
          this.OpenFileCommand.RaiseCanExecuteChanged();
          this.DownloadFileToDiskCommand.RaiseCanExecuteChanged();
          this.PrintFileCommand.RaiseCanExecuteChanged();
          this.ExecuteFileCommand.RaiseCanExecuteChanged();
          this.CompileFileCommand.RaiseCanExecuteChanged();
          this.RenameFileCommand.RaiseCanExecuteChanged();
          this.DeleteFileCommand.RaiseCanExecuteChanged();
        }
      }
    }

    public string TotalFilesText
    {
      get { return this.total_files_text; }
      set { Set(ref this.total_files_text, value); }
    }

    public string TotalBytesText
    {
      get { return this.total_bytes_text; }
      set { Set(ref this.total_bytes_text, value); }
    }

    public string UsedBytesText
    {
      get { return this.used_bytes_text; }
      set { Set(ref this.used_bytes_text, value); }
    }

    public string FreeBytesText
    {
      get { return this.free_bytes_text; }
      set { Set(ref this.free_bytes_text, value); }
    }

    public string ChartText
    {
      get { return this.chart_text; }
      set { Set(ref this.chart_text, value); }
    }

    public double ChartValue
    {
      get { return this.chart_value; }
      set { Set(ref this.chart_value, value); }
    }

    public ObservableCollection<FileViewModel> Files { get; protected set; }

    public AwaitableRelayCommand RefreshCommand { get; protected set; }
    public AwaitableRelayCommand FormatCommand { get; protected set; }
    public AwaitableRelayCommand OpenFileCommand { get; protected set; }
    public AwaitableRelayCommand DownloadFileToDiskCommand { get; protected set; }
    public AwaitableRelayCommand UploadFileFromDiskCommand { get; protected set; }
    public RelayCommand PrintFileCommand { get; protected set; }
    public RelayCommand ExecuteFileCommand { get; protected set; }
    public AwaitableRelayCommand CompileFileCommand { get; protected set; }
    public AwaitableRelayCommand RenameFileCommand { get; protected set; }
    public AwaitableRelayCommand DeleteFileCommand { get; protected set; }

    public SpiffsViewModel(MainWindowViewModel parent, string content_id, string title, string tooltip, string image_name)
      : base(parent, content_id, title, tooltip, image_name)
    {
      // Register to PropertyChanged event of TerminalViewModel
      this.Terminal = parent.Terminal;
      this.Terminal.PropertyChanged += Terminal_PropertyChanged;

      this.Files                     = new ObservableCollection<FileViewModel>();
      this.RefreshCommand            = new AwaitableRelayCommand(RefreshExecute, RefreshCanExecute);
      this.FormatCommand             = new AwaitableRelayCommand(FormatExecute, FormatCanExecute);
      this.OpenFileCommand           = new AwaitableRelayCommand(OpenFileExecute, OpenFileCanExecute);
      this.DownloadFileToDiskCommand = new AwaitableRelayCommand(DownloadFileToDiskExecute, DownloadFileToDiskCanExecute);
      this.UploadFileFromDiskCommand = new AwaitableRelayCommand(UploadFileFromDiskExecute, UploadFileFromDiskCanExecute);
      this.PrintFileCommand          = new RelayCommand(PrintFileExecute, PrintFileCanExecute);
      this.ExecuteFileCommand        = new RelayCommand(ExecuteFileExecute, ExecuteFileCanExecute);
      this.CompileFileCommand        = new AwaitableRelayCommand(CompileFileExecute, CompileFileCanExecute);
      this.RenameFileCommand         = new AwaitableRelayCommand(RenameFileExecute, RenameFileCanExecute);
      this.DeleteFileCommand         = new AwaitableRelayCommand(DeleteFileExecute, DeleteFileCanExecute);
    }

    private void Terminal_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if(e.PropertyName == nameof(TerminalViewModel.IsEnabled))
      {
        this.RefreshCommand.RaiseCanExecuteChanged();
        this.FormatCommand.RaiseCanExecuteChanged();
        this.OpenFileCommand.RaiseCanExecuteChanged();
        this.DownloadFileToDiskCommand.RaiseCanExecuteChanged();
        this.UploadFileFromDiskCommand.RaiseCanExecuteChanged();
        this.PrintFileCommand.RaiseCanExecuteChanged();
        this.ExecuteFileCommand.RaiseCanExecuteChanged();
        this.CompileFileCommand.RaiseCanExecuteChanged();
        this.RenameFileCommand.RaiseCanExecuteChanged();
        this.DeleteFileCommand.RaiseCanExecuteChanged();
      }
    }

    #region Commands
    protected bool RefreshCanExecute()
    {
      return this.Terminal.IsEnabled;
    }

    protected async Task RefreshExecute()
    {
      // Read file list from device
      var fs_info = await this.Terminal.ReadFileList();
      if(fs_info == null)
        return;

      List<FileViewModel> files = new List<FileViewModel>();
      foreach(var file in fs_info.Files)
      {
        files.Add(new FileViewModel() { Name = file.Name, Size = file.Size });
      }

      // Sort files by name
      files.Sort((f1, f2) => { return f1.Name.CompareTo(f2.Name); });

      // Refresh ListView
      this.Files.Clear();
      this.Files.AddRange(files);

      // Refresh StatusBar text
      this.TotalFilesText = fs_info.Files.Count.ToString("n0");
      this.TotalBytesText = fs_info.TotalBytes.ToString("n0");
      this.UsedBytesText  = fs_info.UsedBytes.ToString("n0");
      this.FreeBytesText  = fs_info.FreeBytes.ToString("n0");

      double percentage = 0;
      if(fs_info.TotalBytes > 0)
        percentage = (double)fs_info.UsedBytes / fs_info.TotalBytes;

      this.ChartValue = percentage;
      this.ChartText  = (percentage * 100.0).ToString("n0") + "%";
    }

    protected bool FormatCanExecute()
    {
      return this.Terminal.IsEnabled;
    }

    protected async Task FormatExecute()
    {
      if(this.parent.ShowQuestionOkCancel("Are you sure you want to format SPIFFS?", "SPIFFS") == System.Windows.MessageBoxResult.OK)
      {
        // Format FS
        var result = await this.Terminal.Format();

        // Show result
        if(!String.IsNullOrWhiteSpace(result))
          this.parent.ShowInfo(result.Trim(), "Format");
      }
    }

    protected bool OpenFileCanExecute()
    {
      return (
        this.Terminal.IsEnabled &&
        this.SelectedFile != null &&
        !this.SelectedFile.Name.EndsWith(".lc", StringComparison.OrdinalIgnoreCase));
    }

    protected async Task OpenFileExecute()
    {
      var filename = this.SelectedFile.Name.Trim();
      if(filename.Length == 0)
        return;

      var token = this.parent.CreateCancellationToken();
      using(MemoryStream stream = new MemoryStream())
      {
        // Read file from device to MemoryStream
        await this.Terminal.ReadFile(filename, this.SelectedFile.Size, stream, token);

        stream.Position = 0;
        using(StreamReader reader = new StreamReader(stream))
        {
          // Open content in new file in editor
          var content = reader.ReadToEnd();
          this.parent.NewFile(filename, content);
        }
      }
    }

    protected bool DownloadFileToDiskCanExecute()
    {
      return (this.Terminal.IsEnabled && this.SelectedFile != null);
    }

    protected async Task DownloadFileToDiskExecute()
    {
      string filename = this.SelectedFile.Name.Trim();
      if(filename.Length == 0)
        return;

      var settings = new MvvmDialogs.FrameworkDialogs.SaveFile.SaveFileDialogSettings()
      {
        Filter          = Resources.Document_DefaultFilterLuaLc,
        DefaultExt      = Resources.Document_DefaultExtension,
        OverwritePrompt = true,
        CheckPathExists = true,
        CheckFileExists = false,
        FileName        = filename,
        Title           = "Download file to disk"
      };

      if(this.parent.ShowSaveFileDialog(settings) == true)
      {
        // Delete file if already exists
        var path = settings.FileName;
        if(File.Exists(path))
          File.Delete(path);

        // Write file to FileStream
        var token = this.parent.CreateCancellationToken();
        using(var stream = File.OpenWrite(path))
        {
          await this.Terminal.ReadFile(filename, this.SelectedFile.Size, stream, token);
        }
      }
    }

    protected bool UploadFileFromDiskCanExecute()
    {
      return this.Terminal.IsEnabled;
    }

    protected async Task UploadFileFromDiskExecute()
    {
      var settings = new MvvmDialogs.FrameworkDialogs.OpenFile.OpenFileDialogSettings()
      {
        Filter          = Resources.Document_DefaultFilterLuaLc,
        DefaultExt      = Resources.Document_DefaultExtension,
        CheckFileExists = true,
        CheckPathExists = true,
        Title           = "Upload file from disk"
      };

      if(this.parent.ShowOpenFileDialog(settings) == true)
      {
        var path = settings.FileName;
        var filename = Path.GetFileName(path);

        // Check if file already exists on device
        if(Settings.Default.DeviceOverwritePrompt)
        {
          bool exists = await this.Terminal.ExistsFile(filename);
          if(exists)
          {
            if(this.parent.ShowQuestionOkCancel($"File {filename} already exists on device. Overwrite?", "File exists") != System.Windows.MessageBoxResult.OK)
              return;
          }
        }

        // Write file to device
        var token = this.parent.CreateCancellationToken();
        using(var stream = File.OpenRead(path))
        {
          await this.Terminal.WriteFile(filename, stream, token);
        }

        // Refresh files
        await Refresh();
      }
    }

    protected bool PrintFileCanExecute()
    {
      return (
        this.Terminal.IsEnabled &&
        this.SelectedFile != null &&
        !this.SelectedFile.Name.EndsWith(".lc", StringComparison.OrdinalIgnoreCase));
    }

    protected void PrintFileExecute()
    {
      string filename = this.SelectedFile.Name.Trim();
      if(filename.Length == 0)
        return;

      this.Terminal.PrintFile(filename);
    }

    protected bool ExecuteFileCanExecute()
    {
      return (
        this.Terminal.IsEnabled &&
        this.SelectedFile != null &&
        (this.SelectedFile.Name.EndsWith(".lua", StringComparison.OrdinalIgnoreCase) || this.SelectedFile.Name.EndsWith(".lc", StringComparison.OrdinalIgnoreCase)));
    }

    protected void ExecuteFileExecute()
    {
      string filename = this.SelectedFile.Name.Trim();
      if(filename.Length == 0)
        return;

      this.Terminal.DoFile(filename);
    }

    protected bool CompileFileCanExecute()
    {
      return (
        this.Terminal.IsEnabled &&
        this.SelectedFile != null &&
        this.SelectedFile.Name.EndsWith(".lua", StringComparison.OrdinalIgnoreCase));
    }

    protected async Task CompileFileExecute()
    {
      string filename = this.SelectedFile.Name.Trim();
      if(filename.Length == 0)
        return;

      await this.Terminal.CompileFile(filename);
      await Refresh();
    }

    protected bool RenameFileCanExecute()
    {
      return (this.Terminal.IsEnabled && this.SelectedFile != null);
    }

    protected async Task RenameFileExecute()
    {
      string filename = this.SelectedFile.Name.Trim();
      if(filename.Length == 0)
        return;

      DialogRenameViewModel dialog_vm = new DialogRenameViewModel(this.parent.DialogService, "Enter new file name", "Rename file", this.SelectedFile.Name);
      if(this.parent.DialogService.ShowDialog(this.parent, dialog_vm) == true)
      {
        string new_filename = dialog_vm.Text.Trim();
        if(filename != new_filename)
        {
          await this.Terminal.RenameFile(filename, new_filename);
          await Refresh();
        }
      }
    }

    protected bool DeleteFileCanExecute()
    {
      return (this.Terminal.IsEnabled && this.SelectedFile != null);
    }

    protected async Task DeleteFileExecute()
    {
      string filename = this.SelectedFile.Name.Trim();
      if(filename.Length == 0)
        return;

      if(this.parent.ShowQuestionOkCancel($"Are you sure you want to delete file {filename}?", "Delete file") == System.Windows.MessageBoxResult.OK)
      {
        await this.Terminal.DeleteFile(filename);
        await Refresh();
      }
    }

    public async Task Refresh()
    {
      await RefreshExecute();
    }
    #endregion
  }
}
