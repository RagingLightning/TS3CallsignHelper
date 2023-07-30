using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TS3CallsignHelper.Wpf.Extensions;
public static class ObservableCollectionExtensions {

  public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items) {
    foreach (T item in items)
      collection.Add(item);
  }
}
