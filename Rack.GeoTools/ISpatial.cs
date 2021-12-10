using NetTopologySuite.Geometries;

namespace Rack.GeoTools
{
    public interface ISpatial
    {
        /// <summary>
        /// Координатные данные.
        /// </summary>
        Geometry Geometry { get; }
    }
}