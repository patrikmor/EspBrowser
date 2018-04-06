using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using GalaSoft.MvvmLight.Ioc;
using MvvmDialogs;

namespace EspBrowser
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    protected override void OnStartup(StartupEventArgs e)
    {
      // Configure logging
      log4net.Config.XmlConfigurator.Configure();

      // Set current culture of binding system 
      FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

      // Enable tooltips on disabled controls
      ToolTipService.ShowOnDisabledProperty.OverrideMetadata(typeof(Control), new FrameworkPropertyMetadata(true));

      // Register DialogServie
      SimpleIoc.Default.Register<IDialogService>(() => new DialogService());

      // Load Lua highlighting definition
      IHighlightingDefinition luaHighlighting;
      using(Stream s = typeof(App).Assembly.GetManifestResourceStream("EspBrowser.Resources.Lua.xshd"))
      {
        if(s == null)
          throw new InvalidOperationException("Could not find embedded resource");

        using(XmlReader reader = new XmlTextReader(s))
        {
          luaHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }
      }

      // and register it in the HighlightingManager
      HighlightingManager.Instance.RegisterHighlighting("Lua", new string[] { ".lua" }, luaHighlighting);

      log.Info("APP START");
    }

    private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
      log.Error(e.Exception);
      MessageBox.Show(e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      e.Handled = true;
    }

    public BitmapImage GetImage(string image_name)
    {
      if(String.IsNullOrEmpty(image_name))
        return null;

      return this.Resources.MergedDictionaries[0][image_name] as BitmapImage;
    }
  }
}
