using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MvvmDialogs;
using NodeMcuUploader;

namespace EspBrowser.ViewModels
{
  public class DialogNodeInfoViewModel : ViewModelBase, IModalDialogViewModel
  {
    protected string title;
    protected string version;
    protected string chip_id;
    protected string flash_id;
    protected string flash_size;
    protected string flash_mode;
    protected string flash_speed;
    protected bool? dialog_result;

    public string Title
    {
      get { return this.title; }
      set { Set(ref this.title, value); }
    }

    public string Version
    {
      get { return this.version; }
      set { Set(ref this.version, value); }
    }

    public string ChipId
    {
      get { return this.chip_id; }
      set { Set(ref this.chip_id, value); }
    }

    public string FlashId
    {
      get { return this.flash_id; }
      set { Set(ref this.flash_id, value); }
    }

    public string FlashSize
    {
      get { return this.flash_size; }
      set { Set(ref this.flash_size, value); }
    }

    public string FlashMode
    {
      get { return this.flash_mode; }
      set { Set(ref this.flash_mode, value); }
    }

    public string FlashSpeed
    {
      get { return this.flash_speed; }
      set { Set(ref this.flash_speed, value); }
    }

    public bool? DialogResult
    {
      get { return this.dialog_result; }
      set { Set(ref this.dialog_result, value); }
    }

    public ICommand OkCommand { get; protected set; }

    public DialogNodeInfoViewModel(string title, NodeInfo info)
    {
      this.Title      = title;
      this.Version    = info.MajorVer + "." + info.MinorVer + "." + info.DevVer;
      this.ChipId     = info.ChipId;
      this.FlashId    = info.FlashId;
      this.FlashSize  = info.FlashSize;
      this.FlashMode  = info.FlashMode;
      this.FlashSpeed = info.FlashSpeed;
      this.OkCommand  = new RelayCommand(OkExecuted);
    }

    protected void OkExecuted()
    {
      this.DialogResult = true;
    }
  }
}
