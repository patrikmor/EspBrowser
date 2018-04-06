using System.Linq;
using System.Collections.Generic;

namespace System.Collections.ObjectModel
{
  /// <summary> 
  /// Class that provides extension methods to Collection 
  /// </summary> 
  public static class CollectionExtensions
  {
    /// <summary> 
    /// Add a range of items to a collection. 
    /// </summary> 
    /// <typeparam name="T">Type of objects within the collection.</typeparam> 
    /// <param name="collection">The collection to add items to.</param> 
    /// <param name="items">The items to add to the collection.</param> 
    /// <returns>The collection.</returns> 
    /// <exception cref="ArgumentNullException">An <see cref="System.ArgumentNullException"/> is thrown if <paramref name="collection"/> or <paramref name="items"/> is <see langword="null"/>.</exception> 
    public static Collection<T> AddRange<T>(this Collection<T> collection, IEnumerable<T> items)
    {
      if(collection == null)
        throw new ArgumentNullException(nameof(collection));

      if(items == null)
        throw new ArgumentNullException(nameof(items));

      foreach(var each in items)
      {
        collection.Add(each);
      }

      return collection;
    }

    /// <summary>
    /// Removes all the elements that match the conditions defined by the specified predicate.
    /// </summary>
    /// <typeparam name="T">Type of objects within the collection.</typeparam>
    /// <param name="collection">The collection to remove items from.</param>
    /// <param name="match">The System.Predicate`1 delegate that defines the conditions of the elements to remove.</param>
    /// <returns>The number of elements removed from the Collection.</returns>
    public static int RemoveAll<T>(this Collection<T> collection, Func<T, bool> match)
    {
      if(collection == null)
        throw new ArgumentNullException(nameof(collection));

      if(match == null)
        throw new ArgumentNullException(nameof(match));

      var items = collection.Where(match).ToList();
      foreach(var item in items)
      {
        collection.Remove(item);
      }

      return items.Count;
    }
  }
}
