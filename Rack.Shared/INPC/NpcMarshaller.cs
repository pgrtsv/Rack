using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Rack.Reflection;

namespace Rack.Shared.INPC
{
    [Obsolete("Маршалинг событий INPC неактуален благодаря Rx.")]
    public class NpcMarshaller<TViewModel> : INpcMarshaller<TViewModel>
        where TViewModel : class, IPropertyChangedEventInvoker, INotifyPropertyChanged
    {
        public NpcMarshaller(TViewModel viewModel) => ViewModel = viewModel;

        public List<INPCElement> Children { get; } = new List<INPCElement>();

        public TViewModel ViewModel { get; }

        /// <summary>
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <typeparam name="TObservable"></typeparam>
        /// <param name="property">Свойство <see cref="ViewModel" />, которое необходимо обновлять.</param>
        /// <param name="observable">
        /// Свойство <see cref="ViewModel" />, изменение которого влечёт обновление
        /// <see cref="property" />.
        /// </param>
        /// <returns></returns>
        public INpcSubscription<TViewModel, TObservable, INpcMarshaller<TViewModel>> Subscribe<TProperty, TObservable>(
            Expression<Func<TViewModel, TProperty>> property, Expression<Func<TViewModel, TObservable>> observable)
            where TObservable : INotifyPropertyChanged
        {
            var propertyName = property.GetPropertyName();
            var observableName = observable.GetPropertyName();
            var ret = (NpcSubscription<TViewModel, TObservable, INpcMarshaller<TViewModel>>)
                Children.FirstOrDefault(x => x.ObservablePropertyName.Equals(observableName));
            if (ret == null)
            {
                ret = new NpcSubscription<TViewModel, TObservable, INpcMarshaller<TViewModel>>(this, this)
                {
                    ObservablePropertyName = observableName
                };
                Children.Add(ret);
            }

            if (ret.ViewModelPropertiesNames.Contains(propertyName)) return ret;
            ret.ViewModelPropertiesNames.Add(propertyName);
            return ret;
        }

        public INpcSubscription<TViewModel, TCollectionElement, INpcMarshaller<TViewModel>>
            SubscribeCollection<TProperty, TCollection, TCollectionElement>(
                Expression<Func<TViewModel, TProperty>> property,
                Expression<Func<TViewModel, TCollection>> observable)
            where TCollection : ICollection<TCollectionElement>, INotifyCollectionChanged
        {
            var propertyName = property.GetPropertyName();
            var observableName = observable.GetPropertyName();
            var ret = (NpcSubscription<TViewModel, TCollectionElement, INpcMarshaller<TViewModel>>)
                Children.FirstOrDefault(x => x.ObservablePropertyName.Equals(observableName));
            if (ret == null)
            {
                ret = new NpcSubscription<TViewModel, TCollectionElement, INpcMarshaller<TViewModel>>(this, this)
                {
                    ObservablePropertyName = observableName,
                    IsObservablePropertyCollection = true
                };
                Children.Add(ret);
            }

            if (ret.ViewModelPropertiesNames.Contains(propertyName)) return ret;
            ret.ViewModelPropertiesNames.Add(propertyName);
            return ret;
        }

        public void StartListening()
        {
            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
            foreach (var child in Children)
                child.SubscribeRecursievely();
        }

        public void StopListening()
        {
            ViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
            foreach (var child in Children)
                child.UnsubscribeRecursievely();
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var correspondingChild = Children.FirstOrDefault(x => x.ObservablePropertyName.Equals(e.PropertyName));
            if (correspondingChild == null) return;
            correspondingChild.UnsubscribeRecursievely();
            correspondingChild.SubscribeRecursievely();
            foreach (var viewModelProperty in correspondingChild.ViewModelPropertiesNames)
                ViewModel.OnPropertyChanged(viewModelProperty);
        }
    }
}