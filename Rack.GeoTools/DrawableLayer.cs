using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Rack.GeoTools
{
    public sealed class DrawableLayer<T> : IDrawableLayer
        where T : ISpatial
    {
        public DrawableLayer(string name, Style style, IReadOnlyCollection<T> entities)
        {
            Name = name;
            Style = style;
            Entities = entities;
        }

        public IReadOnlyCollection<T> Entities { get; }

        public string Name { get; set; }

        public Style Style { get; set; }

        IEnumerable<ISpatial> IDrawableLayer.Entities => Entities.Cast<ISpatial>();

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString() => Name;
    }
}