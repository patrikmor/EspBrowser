using System.Linq;
using Xceed.Wpf.AvalonDock.Layout;
using EspBrowser.ViewModels;
using System.Windows;

namespace EspBrowser
{
  /// <summary>
  /// Initialize the AvalonDock Layout. Methods in this class are called before and after the layout is changed.
  /// </summary>
  public class LayoutInitializer : ILayoutUpdateStrategy
  {
    protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private const int SpiffsPreferredWidth = 260;
    private const int TerminalPreferredHeight = 300;

    public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
    {
      if(anchorableToShow.Content is TerminalViewModel)
      {
        var panel_horizontal = layout.Children.OfType<LayoutPanel>().FirstOrDefault();
        var panel_vertical = panel_horizontal.Children.OfType<LayoutPanel>().FirstOrDefault();
        var anchorable_pane_group = panel_vertical.Children.OfType<LayoutAnchorablePaneGroup>().FirstOrDefault();

        if(anchorable_pane_group == null)
        {
          anchorable_pane_group = new LayoutAnchorablePaneGroup();
          panel_vertical.Children.Add(anchorable_pane_group);
        }

        var anchorable_pane = anchorable_pane_group.Children.OfType<LayoutAnchorablePane>().FirstOrDefault();

        if(anchorable_pane == null)
        {
          anchorable_pane = new LayoutAnchorablePane();
          anchorable_pane_group.Children.Add(anchorable_pane);
        }

        anchorable_pane.Children.Add(anchorableToShow);

        return true;
      }
      else if(anchorableToShow.Content is SpiffsViewModel)
      {
        var panel_horizontal = layout.Children.OfType<LayoutPanel>().FirstOrDefault();
        var anchorable_pane_group = panel_horizontal.Children.OfType<LayoutAnchorablePaneGroup>().FirstOrDefault();

        if(anchorable_pane_group == null)
        {
          anchorable_pane_group = new LayoutAnchorablePaneGroup();
          panel_horizontal.Children.Insert(0, anchorable_pane_group);
        }

        var anchorable_pane = anchorable_pane_group.Children.OfType<LayoutAnchorablePane>().FirstOrDefault();

        if(anchorable_pane == null)
        {
          anchorable_pane = new LayoutAnchorablePane();
          anchorable_pane_group.Children.Add(anchorable_pane);
        }

        anchorable_pane.Children.Add(anchorableToShow);

        return true;
      }

      return false;
    }

    public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
    {
      // If this is the first anchorable added to this pane, then use the preferred size.
      if(anchorableShown.Content is TerminalViewModel)
      {
        if(anchorableShown.Parent is LayoutAnchorablePane)
        {
          var anchorable_pane = anchorableShown.Parent as LayoutAnchorablePane;
          if(anchorable_pane.ChildrenCount == 1 && anchorable_pane.Parent is LayoutAnchorablePaneGroup)
          {
            var anchorable_pane_group = anchorable_pane.Parent as LayoutAnchorablePaneGroup;
            if(anchorable_pane_group.ChildrenCount == 1)
              anchorable_pane_group.DockHeight = new GridLength(TerminalPreferredHeight, GridUnitType.Pixel);
          }
        }

        anchorableShown.AutoHideHeight = TerminalPreferredHeight;
      }
      else if(anchorableShown.Content is SpiffsViewModel)
      {
        if(anchorableShown.Parent is LayoutAnchorablePane)
        {
          var anchorable_pane = anchorableShown.Parent as LayoutAnchorablePane;
          if(anchorable_pane.ChildrenCount == 1 && anchorable_pane.Parent is LayoutAnchorablePaneGroup)
          {
            var anchorable_pane_group = anchorable_pane.Parent as LayoutAnchorablePaneGroup;
            if(anchorable_pane_group.ChildrenCount == 1)
              anchorable_pane_group.DockWidth = new GridLength(SpiffsPreferredWidth, GridUnitType.Pixel);
          }
        }

        anchorableShown.AutoHideWidth = SpiffsPreferredWidth;
      }
    }

    public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
    {
      return false;
    }

    public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
    {
    }
  }
}
