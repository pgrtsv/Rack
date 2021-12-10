using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Rack.Shared.INPC
{
    [Obsolete("Маршалинг событий INPC неактуален благодаря Rx.")]
    public interface INpcSubscription<TViewModel, TObservable, TPrevious>
        where TViewModel : class, IPropertyChangedEventInvoker, INotifyPropertyChanged
    {
        TPrevious Back { get; }

        INpcMarshaller<TViewModel> Marshaller { get; }

        INpcSubscription<TViewModel, TAnotherObservable, INpcSubscription<TViewModel, TObservable, TPrevious>>
            AlsoSubscribe<TProperty, TAnotherObservable>(
                Expression<Func<TViewModel, TProperty>> property,
                Expression<Func<TObservable, TAnotherObservable>> observable)
            where TAnotherObservable : INotifyPropertyChanged;

        /// <summary>
        /// Вызывает OnPropertyChanged(<see cref="property" />) в <see cref="TViewModel" /> каждый раз, когда происходит
        /// изменение свойства во всех элементах <see cref="TCollectionElement" /> коллекции <see cref="observable" />.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <typeparam name="TCollectionElement"></typeparam>
        /// <typeparam name="TCollection"></typeparam>
        /// <param name="property"></param>
        /// <param name="observable"></param>
        /// <returns></returns>
        INpcSubscription<TViewModel, TCollectionElement, INpcSubscription<TViewModel, TObservable, TPrevious>>
            AlsoSubscribeCollection<TProperty, TCollection, TCollectionElement>(
                Expression<Func<TViewModel, TProperty>> property,
                Expression<Func<TObservable, TCollection>> observable)
            where TCollection : ICollection<TCollectionElement>, INotifyCollectionChanged;
    }
}