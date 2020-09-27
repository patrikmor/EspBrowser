using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Search;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Windows.Media;
using System.Windows;
using EspBrowser.Views;

namespace EspBrowser
{
  public class CodeTextEditor : TextEditor
  {
    protected static FontSizeConverter fontSizeConverter = new FontSizeConverter();
    protected LuaCodeCompletion completion;
    protected CompletionWindow completionWindow;
    protected SearchPanel searchPanel;

    /// <summary>
    /// AutoScrollToEnd dependency property
    /// </summary>
    public static readonly DependencyProperty AutoScrollToEndProperty =
      DependencyProperty.Register("AutoScrollToEnd", typeof(bool), typeof(CodeTextEditor), new FrameworkPropertyMetadata(Boxes.False));

    /// <summary>
    /// Specifies whether the text editor will automatically scroll to end of document when the text changed
    /// </summary>
    public bool AutoScrollToEnd
    {
      get { return (bool)GetValue(AutoScrollToEndProperty); }
      set { SetValue(AutoScrollToEndProperty, Boxes.Box(value)); }
    }

    public RoutedCommand CtrlSpaceCommand { get; protected set; }

    public CodeTextEditor()
    {
      // Defaults
      FontSize        = (double)fontSizeConverter.ConvertFromString("10pt");
      FontFamily      = new FontFamily("Consolas");
      BorderBrush     = new SolidColorBrush(Colors.Silver);
      BorderThickness = new Thickness(1);

      // Indentation settings
      Options.IndentationSize = 2;
      Options.ConvertTabsToSpaces = true;
      Options.InheritWordWrapIndentation = false;

      // Create search panel
      this.searchPanel = SearchPanel.Install(TextArea);

      TextArea.TextEntering += OnTextEntering;
      TextArea.TextEntered += OnTextEntered;
      TextArea.MouseRightButtonDown += TextArea_MouseRightButtonDown;

      this.CtrlSpaceCommand = new RoutedCommand();
      this.CtrlSpaceCommand.InputGestures.Add(new KeyGesture(Key.Space, ModifierKeys.Control));
      CommandBinding cb = new CommandBinding(this.CtrlSpaceCommand, OnCtrlSpaceCommand);
      this.CommandBindings.Add(cb);

      cb = new CommandBinding(ApplicationCommands.Replace, OnReplace, OnCanReplace);
      this.CommandBindings.Add(cb);

      this.completion = new LuaCodeCompletion();
    }

    private void TextArea_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      var position = GetPositionFromPoint(e.GetPosition(this));
      if(position.HasValue)
        TextArea.Caret.Position = position.Value;
    }

    protected override void OnTextChanged(EventArgs e)
    {
      base.OnTextChanged(e);
      if(this.AutoScrollToEnd)
        ScrollToEnd();
    }

    protected void OnCanReplace(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = !this.IsReadOnly;
    }

    protected void OnReplace(object sender, ExecutedRoutedEventArgs e)
    {
      if(!this.IsReadOnly)
        DialogFindReplace.ShowForReplace(this);
    }

    #region Code Completion
    private void OnTextEntered(object sender, TextCompositionEventArgs textCompositionEventArgs)
    {
      ShowCompletion(textCompositionEventArgs.Text, false);
    }

    private void OnCtrlSpaceCommand(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
    {
      ShowCompletion(null, true);
    }

    private void ShowCompletion(string enteredText, bool controlSpace)
    {
      if(this.completionWindow == null)
      {
        string trigger_word = String.Empty;
        IEnumerable<ICompletionData> results = null;

        try
        {
          trigger_word = GetTriggerWord();
          if(trigger_word.Length > 0)
            results = this.completion.Items.Where(c => c.Text.StartsWith(trigger_word, StringComparison.OrdinalIgnoreCase));
        }
        catch(Exception exception)
        {
          Debug.WriteLine("Error in getting completion: " + exception);
        }

        if(this.completionWindow == null && results != null && results.Any())
        {
          this.completionWindow = new CompletionWindow(TextArea);
          this.completionWindow.CloseWhenCaretAtBeginning = true;
          this.completionWindow.StartOffset -= trigger_word.Length;

          IList<ICompletionData> data = this.completionWindow.CompletionList.CompletionData;
          foreach(var completion in results.OrderBy(c => c is LuaModuleCompletionItem).ThenBy(item => item.Text))
          {
            data.Add(completion);
          }

          if(trigger_word.Length > 0)
          {
            //completionWindow.CompletionList.IsFiltering = false;
            this.completionWindow.CompletionList.SelectItem(trigger_word);
          }

          this.completionWindow.Show();
          this.completionWindow.Closed += (o, args) => completionWindow = null;
        }
        else
        {
          if(this.completionWindow != null)
            this.completionWindow.Close();
        }
      }
    }

    private void OnTextEntering(object sender, TextCompositionEventArgs textCompositionEventArgs)
    {
      if(textCompositionEventArgs.Text.Length > 0 && this.completionWindow != null)
      {
        if(!Char.IsLetterOrDigit(textCompositionEventArgs.Text[0]))
        {
          // Whenever a non-letter is typed while the completion window is open, insert the currently selected element.
          //completionWindow.CompletionList.RequestInsertion(textCompositionEventArgs);
          this.completionWindow.Close();
        }
        else
        {
          string trigger_word = GetTriggerWord() + textCompositionEventArgs.Text;
          if(trigger_word.Length > 0)
          {
            if(!this.completionWindow.CompletionList.CompletionData.Any(c => c.Text.Equals(trigger_word, StringComparison.OrdinalIgnoreCase)))
              this.completionWindow.Close();
          }
        } 
      }

      // Do not set e.Handled=true. We still want to insert the character that was typed.
    }

    private string GetTriggerWord()
    {
      if(this.Document.TextLength == 0)
        return String.Empty;

      int offset = this.CaretOffset;
      int start_offset = offset - 1;
      for(int i = offset - 1; i >= 0; i--)
      {
        char c = this.Document.GetCharAt(i);
        if(Char.IsLetterOrDigit(c))
          start_offset--;
        else
          break;
      }

      if(start_offset < this.CaretOffset - 1)
        return this.Document.GetText(start_offset + 1, offset - 1 - start_offset);

      return String.Empty;
    }
    #endregion
  }
}
