using System.Windows;
using System.Windows.Controls;
using EspBrowser.ViewModels;

namespace EspBrowser
{
  public class PanesStyleSelector : StyleSelector
  {
    public Style DocumentStyle { get; set; }
    public Style TerminalStyle { get; set; }
    public Style SpiffsStyle { get; set; }

    public override Style SelectStyle(object item, DependencyObject container)
    {
      if(item is DocumentViewModel)
        return DocumentStyle;

      if(item is TerminalViewModel)
        return TerminalStyle;

      if(item is SpiffsViewModel)
        return SpiffsStyle;

      return base.SelectStyle(item, container);
    }
  }
}
