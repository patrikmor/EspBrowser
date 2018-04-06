using System;
using System.Windows.Input;
using EspBrowser.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MvvmDialogs;

namespace EspBrowser.ViewModels
{
  public class DialogAboutViewModel :  ViewModelBase, IModalDialogViewModel
  {
    protected string title;
    protected string version;
    protected string email_url;
    protected string github_url;
    protected string esplorer_url;
    protected bool? dialog_result;

    public string Title
    {
      get { return this.title; }
      set { Set(ref this.title, value); }
    }

    public string Version
    {
      get { return this.version; }
      set { Set(ref this.version, value); }
    }

    public string EmailUrl
    {
      get { return this.email_url; }
      set { Set(ref this.email_url, value); }
    }

    public string GitHubUrl
    {
      get { return this.github_url; }
      set { Set(ref this.github_url, value); }
    }

    public string ESPlorerUrl
    {
      get { return this.esplorer_url; }
      set { Set(ref this.esplorer_url, value); }
    }

    public bool? DialogResult
    {
      get { return this.dialog_result; }
      set { Set(ref this.dialog_result, value); }
    }

    public ICommand OkCommand { get; protected set; }
    public ICommand NavigateCommand { get; protected set; }
    public ICommand EmailCommand { get; protected set; }

    public DialogAboutViewModel(string title, string version)
    {
      this.Title           = title;
      this.Version         = String.Format(Resources.About_Version, version);
      this.EmailUrl        = String.Format(Resources.About_EmailUrl, version);
      this.GitHubUrl       = Resources.About_GitHubUrl;
      this.ESPlorerUrl     = Resources.About_ESPlorerUrl;
      this.OkCommand       = new RelayCommand(OkExecute);
      this.NavigateCommand = new RelayCommand<string>(NavigateExecute);
      this.EmailCommand    = new RelayCommand(EmailExecute);
    }

    protected void NavigateExecute(string url)
    {
      System.Diagnostics.Process.Start(url);
      this.DialogResult = true;
    }

    protected void EmailExecute()
    {
      System.Diagnostics.Process.Start(this.EmailUrl);
      this.DialogResult = true;
    }

    protected void OkExecute()
    {
      this.DialogResult = true;
    }
  }
}
