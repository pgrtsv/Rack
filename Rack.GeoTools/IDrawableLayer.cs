using System.Collections.Generic;
using System.ComponentModel;

namespace Rack.GeoTools
{
    /// <summary>
    /// Слой, который можно отрисовать на карте.
    /// </summary>
    public interface IDrawableLayer: INotifyPropertyChanged
    {
        /// <summary>
        /// Название слоя.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Стиль, применяемый для отображения элементов слоя.
        /// </summary>
        Style Style { get; }

        /// <summary>
        /// Перечисление сущностей для отрисовки.
        /// </summary>
        IEnumerable<ISpatial> Entities { get; }
    }
}