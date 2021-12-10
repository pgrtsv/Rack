using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Rack.Shared.INPC
{
    [Obsolete("Маршалинг событий INPC неактуален благодаря Rx.")]
    public interface INpcMarshaller<TViewModel>
        where TViewModel : class, IPropertyChangedEventInvoker, INotifyPropertyChanged
    {
        TViewModel ViewModel { get; }

        /// <summary>
        /// Вызывает OnPropertyChanged(<see cref="property" />) в <see cref="TViewModel" /> всякий раз, когда происходит событие
        /// PropertyChanged в <see cref="observable" />.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <typeparam name="TObservable"></typeparam>
        /// <param name="property"></param>
        /// <param name="observable"></param>
        /// <returns></returns>
        INpcSubscription<TViewModel, TObservable, INpcMarshaller<TViewModel>> Subscribe<TProperty, TObservable>(
            Expression<Func<TViewModel, TProperty>> property,
            Expression<Func<TViewModel, TObservable>> observable)
            where TObservable : INotifyPropertyChanged;

        INpcSubscription<TViewModel, TCollectionElement, INpcMarshaller<TViewModel>>
            SubscribeCollection<TProperty, TCollection, TCollectionElement>(
                Expression<Func<TViewModel, TProperty>> property,
                Expression<Func<TViewModel, TCollection>> observable)
            where TCollection : ICollection<TCollectionElement>, INotifyCollectionChanged;

        void StartListening();
        void StopListening();
    }
}