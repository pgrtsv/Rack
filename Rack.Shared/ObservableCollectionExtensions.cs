using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Rack.Shared
{
    public static class ObservableCollectionExtensions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable) 
            => enumerable != null ? new ObservableCollection<T>(enumerable) : null;

        public static void Repopulate<T>(this ObservableCollection<T> observableCollection,
            IEnumerable<T> newItems)
        {
            observableCollection.Clear();
            foreach (var item in newItems)
                observableCollection.Add(item);
        }
    }
}