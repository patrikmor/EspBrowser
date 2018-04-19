using System.Windows;
using System.Windows.Controls;

namespace EspBrowser.Views
{
  /// <summary>
  /// Interaction logic for DialogSettings.xaml
  /// </summary>
  public partial class DialogSettings : Window
  {
    public DialogSettings()
    {
      InitializeComponent();
    }

    private void Label_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      Label label = sender as Label;
      if(label == null)
        return;

      CheckBox checkbox = label.Target as CheckBox;
      if(checkbox == null)
        return;

      checkbox.IsChecked = !checkbox.IsChecked;
    }
  }
}
