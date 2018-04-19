using System.Windows.Input;
using EspBrowser.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MvvmDialogs;

namespace EspBrowser.ViewModels
{
  public class DialogSettingsViewModel : ViewModelBase, IModalDialogViewModel
  {
    protected string title;
    protected int timeout;
    protected int send_history_length;
    protected int recent_files_length;
    protected bool save_layout;
    protected bool device_overwrite_prompt;
    protected bool? dialog_result;

    public string Title
    {
      get { return this.title; }
      set { Set(ref this.title, value); }
    }

    public int Timeout
    {
      get { return this.timeout; }
      set { Set(ref this.timeout, value); }
    }

    public int SendHistoryLength
    {
      get { return this.send_history_length; }
      set { Set(ref this.send_history_length, value); }
    }

    public int RecentFilesLength
    {
      get { return this.recent_files_length; }
      set { Set(ref this.recent_files_length, value); }
    }

    public bool SaveLayout
    {
      get { return this.save_layout; }
      set { Set(ref this.save_layout, value); }
    }

    public bool DeviceOverwritePrompt
    {
      get { return this.device_overwrite_prompt; }
      set { Set(ref this.device_overwrite_prompt, value); }
    }

    public bool? DialogResult
    {
      get { return this.dialog_result; }
      set { Set(ref this.dialog_result, value); }
    }

    public ICommand OkCommand { get; protected set; }
    public ICommand CancelCommand { get; protected set; }

    public DialogSettingsViewModel(string title)
    {
      this.Title                 = title;
      this.Timeout               = Settings.Default.Timeout;
      this.SendHistoryLength     = Settings.Default.SendHistoryLength;
      this.RecentFilesLength     = Settings.Default.RecentFilesLength;
      this.SaveLayout            = Settings.Default.SaveLayout;
      this.DeviceOverwritePrompt = Settings.Default.DeviceOverwritePrompt;
      this.OkCommand             = new RelayCommand(OkExecuted);
      this.CancelCommand         = new RelayCommand(CancelExecuted);
    }

    protected void OkExecuted()
    {
      this.DialogResult = true;
    }

    protected void CancelExecuted()
    {
      this.DialogResult = false;
    }
  }
}
