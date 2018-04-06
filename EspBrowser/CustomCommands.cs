using System.Windows.Input;

namespace EspBrowser
{
  public static class CustomCommands
  {
    public static readonly RoutedUICommand Exit = new RoutedUICommand(
      "Exit",
      "Exit",
      typeof(CustomCommands),
      new InputGestureCollection() { new KeyGesture(Key.F4, ModifierKeys.Alt) }
    );

    public static readonly RoutedUICommand Terminal = new RoutedUICommand(
      "Terminal",
      "Terminal",
      typeof(CustomCommands),
      new InputGestureCollection() { new KeyGesture(Key.T, ModifierKeys.Control | ModifierKeys.Alt) }
    );

    public static readonly RoutedUICommand Spiffs = new RoutedUICommand(
      "SPIFFS Explorer",
      "Spiffs",
      typeof(CustomCommands),
      new InputGestureCollection() { new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Alt) }
    );
  }
}
