using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Rack.Shared
{
    public static class RxEx
    {
        /// <summary>
        /// Обёртка над <see cref="INotifyDataErrorInfo.ErrorsChanged"/>. 
        /// </summary>
        public static IObservable<EventPattern<DataErrorsChangedEventArgs>> ErrorsChanges<T>(
            this T target)
            where T : INotifyDataErrorInfo =>
            Observable.FromEventPattern<DataErrorsChangedEventArgs>(
                x => target.ErrorsChanged += x,
                x => target.ErrorsChanged -= x);

        /// <summary>
        /// Обёртка над <see cref="INotifyPropertyChanged.PropertyChanged"/>. 
        /// </summary>
        public static IObservable<EventPattern<PropertyChangedEventArgs>> PropertiesChanges<T>(
            this T target)
            where T : INotifyPropertyChanged =>
            Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                x => target.PropertyChanged += x,
                x => target.PropertyChanged -= x);

        /// <summary>
        /// Обёртка над <see cref="INotifyPropertyChanged.PropertyChanged"/>. 
        /// </summary>
        public static IObservable<EventPattern<PropertyChangedEventArgs>> PropertiesChanges<T>(
            this T target,
            params string[] propertiesNames)
            where T : INotifyPropertyChanged =>
            target.PropertiesChanges()
                .Where(x =>
                    propertiesNames.Contains(x.EventArgs.PropertyName));
    }
}