using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MvvmDialogs;

namespace EspBrowser.ViewModels
{
  public class DialogRenameViewModel : ViewModelBase, IModalDialogViewModel
  {
    protected string message;
    protected string title;
    protected string text;
    protected bool? dialog_result;

    public IDialogService DialogService { get; protected set; }

    public string Message
    {
      get { return this.message; }
      set { Set(ref this.message, value); }
    }

    public string Title
    {
      get { return this.title; }
      set { Set(ref this.title, value); }
    }

    public string Text
    {
      get { return this.text; }
      set { Set(ref this.text, value); }
    }

    public bool? DialogResult
    {
      get { return this.dialog_result; }
      set { Set(ref this.dialog_result, value); }
    }

    public ICommand OkCommand { get; protected set; }
    public ICommand CancelCommand { get; protected set; }

    public DialogRenameViewModel(IDialogService dialog_service, string message, string title, string text)
    {
      this.DialogService = dialog_service;
      this.Message       = message;
      this.Title         = title;
      this.Text          = text;
      this.OkCommand     = new RelayCommand(OkExecuted);
      this.CancelCommand = new RelayCommand(CancelExecuted);
    }

    protected void OkExecuted()
    {
      if(this.Text.Trim().Length == 0)
      {
        this.DialogService.ShowMessageBox(this, "File name must be specified", "Rename file", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }

      this.DialogResult = true;
    }

    protected void CancelExecuted()
    {
      this.DialogResult = false;
    }
  }
}
