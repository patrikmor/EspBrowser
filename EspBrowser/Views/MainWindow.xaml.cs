using EspBrowser.Properties;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace EspBrowser.Views
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private const string LAYOUT_FILENAME = "layout.xml";
    private GridViewColumnHeader listview_sort_column;
    private SortAdorner listview_sort_adorner;

    public MainWindow()
    {
      InitializeComponent();
    }

    private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
    {
      var column  = sender as GridViewColumnHeader;
      var sort_by = column.Tag.ToString();
      var listview_files = WindowUtils.FindParent<ListView>(column);

      if(this.listview_sort_column != null)
      {
        AdornerLayer.GetAdornerLayer(this.listview_sort_column).Remove(this.listview_sort_adorner);
        listview_files.Items.SortDescriptions.Clear();
      }

      ListSortDirection direction = ListSortDirection.Ascending;
      if(this.listview_sort_column == column && this.listview_sort_adorner.Direction == direction)
        direction = ListSortDirection.Descending;

      this.listview_sort_column = column;
      this.listview_sort_adorner = new SortAdorner(this.listview_sort_column, direction);
      AdornerLayer.GetAdornerLayer(this.listview_sort_column).Add(this.listview_sort_adorner);
      listview_files.Items.SortDescriptions.Add(new SortDescription(sort_by, direction));
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      if(Settings.Default.SaveLayout)
        LoadLayout();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      if(Settings.Default.SaveLayout)
        SaveLayout();
    }

    public void SaveLayout()
    {
      try
      {
        XmlLayoutSerializer serializer = new XmlLayoutSerializer(docking_manager);
        string path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, LAYOUT_FILENAME);
        if(File.Exists(path))
        {
          File.Delete(path);
        }

        using(var file = File.OpenWrite(path))
        {
          serializer.Serialize(file);
        }
      }
      catch(Exception ex)
      {
        log.Error(ex);
      }
    }

    public void LoadLayout()
    {
      try
      {
        XmlLayoutSerializer serializer = new XmlLayoutSerializer(docking_manager);
        string path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, LAYOUT_FILENAME);
        if(File.Exists(path))
        {
          using(var file = File.OpenRead(path))
          {
            serializer.Deserialize(file);
          }
        }
      }
      catch(Exception ex)
      {
        log.Error(ex);
      }
    }
  }
}
