using System;

namespace Rack.Shared.INPC
{
    [Obsolete("Маршалинг событий INPC неактуален благодаря Rx.")]
    public interface IPropertyChangedEventInvoker
    {
        void OnPropertyChanged(string propertyName);
    }
}