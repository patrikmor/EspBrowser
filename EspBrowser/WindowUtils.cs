using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace EspBrowser
{
  public static class WindowUtils
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="image_name"></param>
    /// <returns></returns>
    public static BitmapImage GetImage(string image_name)
    {
      return ((App)Application.Current).GetImage(image_name);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="child"></param>
    /// <returns></returns>
    public static T FindParent<T>(DependencyObject child) where T : DependencyObject
    {
      // Get parent item
      DependencyObject parentObject = VisualTreeHelper.GetParent(child);

      // We've reached the end of the tree
      if(parentObject == null)
        return null;

      // Check if the parent matches the type we're looking for
      T parent = parentObject as T;
      if(parent != null)
        return parent;
      else
        return FindParent<T>(parentObject);
    }
  }
}
