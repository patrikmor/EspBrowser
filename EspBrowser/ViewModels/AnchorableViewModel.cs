using GalaSoft.MvvmLight.CommandWpf;

namespace EspBrowser.ViewModels
{
  public class AnchorableViewModel : ContentViewModel
  {
    protected bool can_hide = true;

    public bool CanHide
    {
      get { return this.can_hide; }
      set { Set(ref this.can_hide, value); }
    }

    public RelayCommand HideCommand { get; protected set; }

    public AnchorableViewModel(MainWindowViewModel parent, string content_id, string title, string tooltip, string image_name)
      : base(parent, content_id, title, tooltip, image_name)
    {
      this.HideCommand = new RelayCommand(HideExecute, HideCanExecute);
    }

    public void Activate()
    {
      if(!this.parent.Anchorables.Contains(this))
        this.parent.Anchorables.Add(this);

      this.IsActive = true;
    }

    public void Hide()
    {
      HideExecute();
    }

    #region Commands
    protected override bool CloseCanExecute()
    {
      return HideCanExecute();
    }

    protected override void CloseExecute()
    {
      // Hide anchorable instead of Close
      HideExecute();
    }

    protected bool HideCanExecute()
    {
      return this.can_hide;
    }

    protected void HideExecute()
    {
      this.parent.Anchorables.Remove(this);
    }
    #endregion
  }
}
