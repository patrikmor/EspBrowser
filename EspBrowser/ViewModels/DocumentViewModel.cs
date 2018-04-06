using System;
using System.IO;
using System.Windows;
using ICSharpCode.AvalonEdit.Document;
using EspBrowser.Properties;

namespace EspBrowser.ViewModels
{
  public class DocumentViewModel : ContentViewModel
  {
    protected TextDocument document;
    protected string filename;
    protected string path;
    protected bool is_modified;
    protected bool is_readonly;
    protected bool show_line_numbers = true;
    protected bool word_wrap;

    public string FileName
    {
      get { return this.filename; }
      set
      {
        if(Set(ref this.filename, value))
          RefreshTitle();
      }
    }

    public string Path
    {
      get { return this.path; }
      set { Set(ref this.path, value); }
    }

    public TextDocument Document
    {
      get { return this.document; }
      set { Set(ref this.document, value); }
    }

    public bool IsModified
    {
      get { return this.is_modified; }
      set
      {
        if(Set(ref this.is_modified, value))
          RefreshTitle();
      }
    }

    public bool IsReadOnly
    {
      get { return this.is_readonly; }
      set { Set(ref this.is_readonly, value); }
    }

    public bool ShowLineNumbers
    {
      get { return this.show_line_numbers; }
      set { Set(ref this.show_line_numbers, value); }
    }

    public bool WordWrap
    {
      get { return this.word_wrap; }
      set { Set(ref this.word_wrap, value); }
    }

    public DocumentViewModel(MainWindowViewModel parent, string filename, string path, string image_name, string text_content)
      : base(parent, null, filename, (!String.IsNullOrEmpty(path) ? path : filename), image_name)
    {
      this.FileName = filename;
      this.Path     = path;
      this.Document = new TextDocument(text_content);
    }

    public void Close()
    {
      CloseExecute();
    }

    public bool Save(bool save_as)
    {
      if(String.IsNullOrEmpty(this.Path) || save_as)
      {
        var settings = new MvvmDialogs.FrameworkDialogs.SaveFile.SaveFileDialogSettings()
        {
          Filter          = Resources.Document_DefaultFilterLua,
          DefaultExt      = Resources.Document_DefaultExtension,
          OverwritePrompt = true,
          CheckPathExists = true,
          CheckFileExists = false,
          FileName        = this.FileName,
          Title           = "Save file"
        };

        if(this.parent.ShowSaveFileDialog(settings) == true)
          this.Path = settings.FileName;
        else
          return false;
      }

      try
      {
        if(File.Exists(this.Path))
          File.Delete(this.Path);

        using(var writer = File.CreateText(this.Path))
        {
          this.Document.WriteTextTo(writer);
          writer.Flush();
        }
      }
      catch(Exception ex)
      {
        log.Error(ex);
        this.parent.ShowError($"Error saving file {this.Path}", "IO Error");
        return false;
      }

      this.FileName   = System.IO.Path.GetFileName(this.Path);
      this.ToolTip    = this.Path;
      this.IsModified = false;
      return true;
    }

    protected void RefreshTitle()
    {
      string title = this.FileName;
      if(this.IsModified)
        title += "*";

      this.Title = title;
    }

    #region Commands
    protected override void CloseExecute()
    {
      if(this.IsModified)
      {
        var result = MessageBox.Show($"Save changes to file {this.FileName}?", "Close file", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        if(result == MessageBoxResult.Yes)
        {
          if(!Save(false))
            return;
        }
        else if(result == MessageBoxResult.Cancel)
        {
          return;
        }
      }

      this.parent.Documents.Remove(this);

      if(this.parent.ActiveDocument == this)
        this.parent.ActiveDocument = null;
    }
    #endregion
  }
}
