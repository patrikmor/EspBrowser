using System;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace EspBrowser.ViewModels
{
  public abstract class ContentViewModel : ViewModelBase
  {
    protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    protected MainWindowViewModel parent;
    protected string content_id;
    protected string title;
    protected string tooltip;
    protected ImageSource icon_source;
    protected bool is_active = false;
    protected bool can_close = true;
    protected bool can_float = true;

    public string ContentId
    {
      get { return this.content_id; }
      set { Set(ref this.content_id, value); }
    }

    public string Title
    {
      get { return this.title; }
      set { Set(ref this.title, value); }
    }

    public string ToolTip
    {
      get { return this.tooltip; }
      set { Set(ref this.tooltip, value); }
    }

    public ImageSource IconSource
    {
      get { return this.icon_source; }
      set { Set(ref this.icon_source, value); }
    }

    public bool IsActive
    {
      get { return this.is_active; }
      set { Set(ref this.is_active, value); }
    }

    public bool CanClose
    {
      get { return this.can_close; }
      set { Set(ref this.can_close, value); }
    }

    public bool CanFloat
    {
      get { return this.can_float; }
      set { Set(ref this.can_float, value); }
    }

    public RelayCommand CloseCommand { get; protected set; }

    public ContentViewModel(MainWindowViewModel parent, string content_id, string title, string tooltip, string image_name)
    {
      this.parent       = parent;
      this.ContentId    = (!String.IsNullOrEmpty(content_id) ? content_id : Guid.NewGuid().ToString());
      this.Title        = title;
      this.ToolTip      = tooltip;
      this.IconSource   = (!String.IsNullOrEmpty(image_name) ? WindowUtils.GetImage(image_name) : null);
      this.CloseCommand = new RelayCommand(CloseExecute, CloseCanExecute);
    }

    protected virtual bool CloseCanExecute()
    {
      return this.CanClose;
    }

    protected abstract void CloseExecute();
  }
}
