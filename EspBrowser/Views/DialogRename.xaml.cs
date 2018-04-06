using System;
using System.Windows;
using System.Windows.Input;

namespace EspBrowser.Views
{
  /// <summary>
  /// Interaction logic for DialogRenameView.xaml
  /// </summary>
  public partial class DialogRename : Window
  {
    public DialogRename()
    {
      InitializeComponent();
    }

    private void Window_Activated(object sender, EventArgs e)
    {
      Keyboard.Focus(txt_text);
      int index = txt_text.Text.LastIndexOf('.');
      if(index > -1)
        txt_text.Select(0, index);
      else
        txt_text.SelectAll();
    }
  }
}
