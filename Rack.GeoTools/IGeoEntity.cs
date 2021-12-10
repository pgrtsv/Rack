using System;
using System.ComponentModel;

namespace Rack.GeoTools
{
    /// <summary>
    /// Объект, обладающий пространственными данными для отображения на карте.
    /// </summary>
    [Obsolete("Используйте интерфейсы ISpatial или ILabeledSpatial")]
    public interface IGeoEntity: ILabeledSpatial, INotifyPropertyChanged
    {
    }
}