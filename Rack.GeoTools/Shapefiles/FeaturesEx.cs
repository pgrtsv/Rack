using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace Rack.GeoTools.Shapefiles
{
    /// <summary>
    /// Методы расширений для работы с <see cref="IFeature"/> и шейп-файлами.
    /// </summary>
    public static class FeaturesEx
    {
        /// <summary>
        /// Записывает пространственные данные в шейп-файл.
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <param name="encoding">Кодировка .dbf-файла</param>
        public static void WriteToShapefile<T>(this IReadOnlyCollection<T> features, string path,
            Encoding encoding = null)
            where T : IFeature
        {
            if (features == null) throw new ArgumentNullException(nameof(features));
            if (features.Count == 0)
                throw new ArgumentException("Collection is empty", nameof(features));
            if (Path.HasExtension(path))
                path = Path.ChangeExtension(path, null);
            encoding ??= Encoding.UTF8;
            new ShapefileDataWriter(path, GeometryFactory.Default, encoding)
            {
                Header = ShapefileDataWriter.GetHeader(features.First(), features.Count, encoding)
            }.Write(features.Cast<IFeature>());
        }
    }
}