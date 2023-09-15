using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace TS3CallsignHelper.API;
public static class ObservableCollectionExtensions {

  /// <summary>
  /// Provides a way to add to an <seealso cref="ObservableCollection{T}"/> in a thread-safe manner by wrapping the call in <c>CurrentDispatcher.Invoke()</c>
  /// </summary>
  /// <typeparam name="T">Type of the items in the <seealso cref="ObservableCollection{T}"/></typeparam>
  /// <param name="collection">Collection to add item to</param>
  /// <param name="item">Item to add</param>
  public static void AddSafe<T>(this ObservableCollection<T> collection, T item) {
    Application.Current.Dispatcher.Invoke(() => collection.Add(item));
  }

  /// <summary>
  /// Provides a way to remove from an <seealso cref="ObservableCollection{T}"/> in a thread-safe manner by wrapping the call in <c>CurrentDispatcher.Invoke()</c>
  /// </summary>
  /// <typeparam name="T">Type of the items in the <seealso cref="ObservableCollection{T}"/></typeparam>
  /// <param name="collection">Collection to remove item from</param>
  /// <param name="item">Item to remove</param>
  public static void RemoveSafe<T>(this ObservableCollection<T> collection, T item) {
    Application.Current.Dispatcher.Invoke(() => collection.Remove(item));
  }

  /// <summary>
  /// Provides a way to clear an <seealso cref="ObservableCollection{T}"/> in a thread-safe manner by wrapping the call in <c>CurrentDispatcher.Invoke()</c>
  /// </summary>
  /// <typeparam name="T">Type of the items in the <seealso cref="ObservableCollection{T}"/></typeparam>
  /// <param name="collection">Collection to clear</param>
  public static void ClearSafe<T>(this ObservableCollection<T> collection) {
    Application.Current.Dispatcher.Invoke(() => collection.Clear());
  }

  /// <summary>
  /// Provides a way to add a range of items to an <seealso cref="ObservableCollection{T}"/> in a thread-safe manner by wrapping the call in <c>CurrentDispatcher.Invoke()</c>
  /// </summary>
  /// <typeparam name="T">Type of the items in the <seealso cref="ObservableCollection{T}"/></typeparam>
  /// <param name="collection">Collection to add items to</param>
  /// <param name="items">Items to add</param>
  public static void AddRangeSafe<T>(this ObservableCollection<T> collection, IEnumerable<T> items) {
    Application.Current.Dispatcher.Invoke(() => {
      foreach (T item in items)
        collection.Add(item);
    });
  }
}
