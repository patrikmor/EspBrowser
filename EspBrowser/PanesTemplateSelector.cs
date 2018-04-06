using System.Windows;
using System.Windows.Controls;
using EspBrowser.ViewModels;
using Xceed.Wpf.AvalonDock.Layout;

namespace EspBrowser
{
  public class PanesTemplateSelector : DataTemplateSelector
  {
    public DataTemplate DocumentTemplate { get; set; }
    public DataTemplate TerminalTemplate { get; set; }
    public DataTemplate SpiffsTemplate { get; set; }

    public PanesTemplateSelector()
    {
    }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      var itemAsLayoutContent = item as LayoutContent;

      if(item is DocumentViewModel)
        return DocumentTemplate;

      if(item is TerminalViewModel)
        return TerminalTemplate;

      if(item is SpiffsViewModel)
        return SpiffsTemplate;

      return base.SelectTemplate(item, container);
    }
  }
}
