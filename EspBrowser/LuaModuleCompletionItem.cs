using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace EspBrowser
{
  /// <summary>
  /// Implements AvalonEdit ICompletionData interface to provide the entries in the completion drop down.
  /// </summary>
  public class LuaModuleCompletionItem : ICompletionData
  {
    private static ImageSource image;

    static LuaModuleCompletionItem()
    {
      Bitmap bmp = new Bitmap(typeof(LuaCompletionItem).Assembly.GetManifestResourceStream("EspBrowser.Resources.Field.png"));
      IntPtr hBitmap = bmp.GetHbitmap();
      image = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
      image.Freeze();
    }

    public LuaModuleCompletionItem(string text)
    {
      this.Text = text;
    }

    public ImageSource Image
    {
      get { return image; }
    }

    public string Text
    {
      get;
      private set;
    }

    // Use this property if you want to show a fancy UIElement in the drop down list.
    public object Content
    {
      get { return this.Text; }
    }

    public object Description
    {
      get { return null; }
    }

    public double Priority
    {
      get { return 0; }
    }

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
      textArea.Document.Replace(completionSegment, this.Text);
    }
  }
}

