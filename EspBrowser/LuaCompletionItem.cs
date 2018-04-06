using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace EspBrowser
{
  /// <summary>
  /// Implements AvalonEdit ICompletionData interface to provide the entries in the completion drop down.
  /// </summary>
  public class LuaCompletionItem : ICompletionData
  {
    public LuaCompletionItem(string text)
    {
      this.Text = text;
    }

    public ImageSource Image
    {
      get;
      set;
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

