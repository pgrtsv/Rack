using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace Rack.GeoTools.Abstractions.Model
{
    /// <summary>
    /// Поверхность, заданная сеткой (гридом).
    /// </summary>
    public interface ISurface
    {
        /// <summary>
        /// Прямоугольная область, определяющая границы поверхности по горизонтали.
        /// </summary>
        Envelope Envelope { get; }

        /// <summary>
        /// Наибольшее значение поверхности в узлах.
        /// </summary>
        double ZMax { get; }

        /// <summary>
        /// Наименьшее значение поверхности в узлах.
        /// </summary>
        double ZMin { get; }

        /// <summary>
        /// Количество узлов поверхности по оси x.
        /// </summary>
        int XCount { get; }

        /// <summary>
        /// Количество узлов поверхности по оси y.
        /// </summary>
        int YCount { get; }

        /// <summary>
        /// Шаг поверхности по оси x.
        /// </summary>
        double XStep { get; }

        /// <summary>
        /// Шаг поверхности по оси y.
        /// </summary>
        double YStep { get; }

        /// <summary>
        /// Значения поверхности в узлах.
        /// </summary>
        IReadOnlyCollection<double> Values { get; }

        /// <summary>
        /// Коэффициенты сплайнов.
        /// </summary>
        IReadOnlyCollection<double> Coefficients { get; }

        /// <summary>
        /// Возвращает значение z в указанной точке.
        /// </summary>
        /// <exception cref="SurfaceGetZException">
        /// Дополнительные данные:
        /// x, XMax, XMin для <see cref="GetZExceptionKind.XOutOfBounds" />;
        /// y, YMax, YMin для <see cref="GetZExceptionKind.YOutOfBounds" />.
        /// </exception>
        double GetZ(double x, double y);
    }
}