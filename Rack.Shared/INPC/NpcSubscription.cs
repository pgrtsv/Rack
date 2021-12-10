using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Rack.Reflection;

namespace Rack.Shared.INPC
{
    [Obsolete("Маршалинг событий INPC неактуален благодаря Rx.")]
    public class
        NpcSubscription<TViewModel, TObservableProperty, TPrevious> :
            INpcSubscription<TViewModel, TObservableProperty, TPrevious>, INPCElement
        where TViewModel : class, IPropertyChangedEventInvoker, INotifyPropertyChanged
        where TPrevious : class
    {
        private readonly NpcMarshaller<TViewModel> _marshaller;

        public NpcSubscription(
            NpcMarshaller<TViewModel> marshaller,
            TPrevious previous)
        {
            Marshaller = _marshaller = marshaller;
            Back = previous;
            if (previous is INPCElement previousElement) Parent = previousElement;
        }

        public List<string> ViewModelPropertiesNames { get; } = new List<string>();

        public List<INotifyPropertyChanged> ObservablePropertyEntities { get; } = new List<INotifyPropertyChanged>();

        public List<INotifyCollectionChanged> ObservableCollections { get; } = new List<INotifyCollectionChanged>();

        public string ObservablePropertyName { get; set; }

        public bool IsObservablePropertyCollection { get; set; }

        public INPCElement Parent { get; set; }

        public List<INPCElement> Children { get; } = new List<INPCElement>();

        public void SubscribeRecursievely()
        {
            if (Parent == null)
            {
                var newProperty = _marshaller.ViewModel.GetType()
                    .GetProperty(ObservablePropertyName)
                    .GetValue(_marshaller.ViewModel) as INotifyPropertyChanged;
                if (newProperty == null) return;
                newProperty.PropertyChanged += OnPropertyChanged;
                ObservablePropertyEntities.Add(newProperty);
            }
            else
            {
                if (IsObservablePropertyCollection)
                    foreach (var item in Parent.ObservablePropertyEntities)
                    {
                        if (!(item.GetType().GetProperty(ObservablePropertyName).GetValue(item) is
                            INotifyCollectionChanged
                            childItemsCollection)) continue;
                        childItemsCollection.CollectionChanged += OnCollectionChanged;
                        ObservableCollections.Add(childItemsCollection);
                        foreach (INotifyPropertyChanged childItem in (IEnumerable) childItemsCollection)
                        {
                            childItem.PropertyChanged += OnPropertyChanged;
                            ObservablePropertyEntities.Add(childItem);
                        }
                    }
                else
                    foreach (var item in Parent.ObservablePropertyEntities)
                    {
                        if (!(item.GetType().GetProperty(ObservablePropertyName).GetValue(item) is
                            INotifyPropertyChanged
                            childItem)) continue;
                        childItem.PropertyChanged += OnPropertyChanged;
                        ObservablePropertyEntities.Add(childItem);
                    }
            }

            foreach (var child in Children)
                child.SubscribeRecursievely();
        }

        public void UnsubscribeRecursievely()
        {
            foreach (var collection in ObservableCollections)
                collection.CollectionChanged -= OnCollectionChanged;
            ObservableCollections.Clear();
            foreach (var observablePropertyEntity in ObservablePropertyEntities)
                observablePropertyEntity.PropertyChanged -= OnPropertyChanged;
            ObservablePropertyEntities.Clear();
            foreach (var child in Children)
                child.UnsubscribeRecursievely();
        }

        public TPrevious Back { get; }

        public INpcMarshaller<TViewModel> Marshaller { get; }

        public INpcSubscription<TViewModel, TAnotherObservable,
                INpcSubscription<TViewModel, TObservableProperty, TPrevious>>
            AlsoSubscribe<TProperty, TAnotherObservable>(Expression<Func<TViewModel, TProperty>> property,
                Expression<Func<TObservableProperty, TAnotherObservable>> observable)
            where TAnotherObservable : INotifyPropertyChanged
        {
            var viewModelPropertyName = property.GetPropertyName(); //Save
            var observablePropertyName = observable.GetPropertyName(); //ETC
            var ret =
                (NpcSubscription<TViewModel, TAnotherObservable,
                    INpcSubscription<TViewModel, TObservableProperty, TPrevious>>)
                Children.FirstOrDefault(x => x.ObservablePropertyName.Equals(observablePropertyName));
            if (ret == null)
            {
                ret =
                    new NpcSubscription<TViewModel, TAnotherObservable,
                        INpcSubscription<TViewModel, TObservableProperty, TPrevious>>(_marshaller, this)
                    {
                        ObservablePropertyName = observablePropertyName
                    };
                Children.Add(ret);
            }

            if (ret.ViewModelPropertiesNames.Contains(viewModelPropertyName)) return ret;
            ret.ViewModelPropertiesNames.Add(viewModelPropertyName);
            return ret;
        }

        public INpcSubscription<TViewModel, TCollectionElement,
                INpcSubscription<TViewModel, TObservableProperty, TPrevious>>
            AlsoSubscribeCollection<TProperty, TCollection, TCollectionElement>(
                Expression<Func<TViewModel, TProperty>> property,
                Expression<Func<TObservableProperty, TCollection>> observable)
            where TCollection : ICollection<TCollectionElement>, INotifyCollectionChanged
        {
            var viewModelPropertyName = property.GetPropertyName(); //Save
            var observablePropertyName = observable.GetPropertyName(); //ETC
            var ret =
                (NpcSubscription<TViewModel, TCollectionElement,
                    INpcSubscription<TViewModel, TObservableProperty, TPrevious>>)
                Children.FirstOrDefault(x => x.ObservablePropertyName.Equals(observablePropertyName));
            if (ret == null)
            {
                ret =
                    new NpcSubscription<TViewModel, TCollectionElement,
                        INpcSubscription<TViewModel, TObservableProperty, TPrevious>>(_marshaller, this)
                    {
                        ObservablePropertyName = observablePropertyName,
                        IsObservablePropertyCollection = true
                    };
                Children.Add(ret);
            }

            if (ret.ViewModelPropertiesNames.Contains(viewModelPropertyName)) return ret;
            ret.ViewModelPropertiesNames.Add(viewModelPropertyName);
            return ret;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (INotifyPropertyChanged oldItem in e.OldItems)
                    oldItem.PropertyChanged -= OnPropertyChanged;
            if (e.NewItems != null)
                foreach (INotifyPropertyChanged newItem in e.NewItems)
                    newItem.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var relativeChild = Children.FirstOrDefault(x => x.ObservablePropertyName.Equals(e.PropertyName));
            if (relativeChild != null)
            {
                relativeChild.UnsubscribeRecursievely();
                relativeChild.SubscribeRecursievely();
            }

            foreach (var viewModelProprety in ViewModelPropertiesNames)
                _marshaller.ViewModel.OnPropertyChanged(viewModelProprety);
        }
    }

    public interface INPCElement
    {
        string ObservablePropertyName { get; set; }

        bool IsObservablePropertyCollection { get; set; }

        List<INotifyPropertyChanged> ObservablePropertyEntities { get; }

        List<INotifyCollectionChanged> ObservableCollections { get; }

        List<string> ViewModelPropertiesNames { get; }

        INPCElement Parent { get; set; }

        List<INPCElement> Children { get; }

        void SubscribeRecursievely();

        void UnsubscribeRecursievely();
    }
}