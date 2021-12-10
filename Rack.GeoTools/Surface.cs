using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using Rack.Exceptions;
using Rack.GeoTools.Abstractions.Model;

namespace Rack.GeoTools
{
    /// <inheritdoc cref="ISurface"/>
    [DataContract]
    public class Surface : ISurface, INotifyPropertyChanged
    {
        /// <summary>
        /// Магические переменные, использующиеся при подсчёте коэффициентов поверхности.
        /// </summary>
        private static readonly double[] _qk =
        {
            -0.25, -0.26666666666, -0.26785714285, -0.26794258373, -0.26794871794,
            -0.26794915836, -0.26794918998, -0.26794919225, -0.26794919241, -0.26794919242, -0.26794919243
        };

        private double[] _coefficients;

        private readonly double[] _values;

        protected Surface() {}

        /// <summary>
        /// Создаёт поверхность.
        /// </summary>
        /// <param name="xMin">Наименьшее значение x.</param>
        /// <param name="xMax">Наибольшее значение x.</param>
        /// <param name="yMin">Наименьшее значение y.</param>
        /// <param name="yMax">Наибольшее значение y.</param>
        /// <param name="xCount">Размер поверхности по оси x.</param>
        /// <param name="yCount">Размер поверхности по оси y.</param>
        /// <param name="values">Значения поверхности (Z).</param>
        /// <exception cref="SurfaceCreationException">
        /// Дополнительные данные:
        /// xMin, xMax для <see cref="CreationExceptionKind.XAxisLimitsInverted" />;
        /// yMin, yMax для <see cref="CreationExceptionKind.YAxisLimitsInverted" />;
        /// values.Length для <see cref="CreationExceptionKind.CountUnequality" />.
        /// </exception>
        public Surface(
            double xMin,
            double xMax,
            double yMin,
            double yMax,
            int xCount,
            int yCount,
            IEnumerable<double> values)
        {
            if (xMin > xMax)
                throw new SurfaceCreationException(CreationExceptionKind.XAxisLimitsInverted)
                    .WithAdditionalData(nameof(xMin), xMin)
                    .WithAdditionalData(nameof(xMax), xMax);
            if (yMin > yMax)
                throw new SurfaceCreationException(CreationExceptionKind.YAxisLimitsInverted)
                    .WithAdditionalData(nameof(yMin), yMin)
                    .WithAdditionalData(nameof(yMax), yMax);
            _values = values.ToArray();
            if (xCount * yCount != _values.Length)
                throw new SurfaceCreationException(CreationExceptionKind.CountUnequality)
                    .WithAdditionalData(nameof(xCount), xCount)
                    .WithAdditionalData(nameof(yCount), yCount)
                    .WithAdditionalData("values.Length", _values.Length);
            Envelope = new Envelope(xMin, xMax, yMin, yMax);
            XCount = xCount;
            YCount = yCount;
            ZMin = _values.Min();
            ZMax = _values.Max();
            CalculateSteps();
            CalculateCoefficients();
        }

        /// <inheritdoc />
        public Envelope Envelope { get; }

        /// <summary>
        /// Верхняя граница поверхности по оси x.
        /// </summary>
        [DataMember]
        public double XMax => Envelope.MaxX;

        /// <summary>
        /// Нижняя граница поверхности по оси x.
        /// </summary>
        [DataMember]
        public double XMin => Envelope.MinX;

        /// <summary>
        /// Верхняя граница поверхности по оси y.
        /// </summary>
        [DataMember]
        public double YMax => Envelope.MaxY;

        /// <summary>
        /// Нижняя граница поверхности по оси y.
        /// </summary>
        [DataMember]
        public double YMin => Envelope.MinY;

        /// <inheritdoc />
        public double ZMax { get; private set; }

        /// <inheritdoc />
        public double ZMin { get; private set; }

        /// <inheritdoc />
        [DataMember]
        public int XCount { get; private set; }

        /// <inheritdoc />
        [DataMember]
        public int YCount { get; private set; }

        /// <inheritdoc />
        [DataMember]
        public double XStep { get; private set; }

        /// <inheritdoc />
        [DataMember]
        public double YStep { get; private set; }

        /// <inheritdoc />
        [DataMember]
        public IReadOnlyCollection<double> Values => _values;

        /// <inheritdoc />
        public IReadOnlyCollection<double> Coefficients => _coefficients;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Рассчитывает шаги поверхности по осям x и y.
        /// </summary>
        private void CalculateSteps()
        {
            XStep = (XMax - XMin) / (XCount - 1);
            YStep = (YMax - YMin) / (YCount - 1);
        }

        /// <summary>
        /// Считывает поверхность из .grd файла.
        /// </summary>
        /// <param name="filePath">Путь к .grd файлу.</param>
        /// <exception cref="SurfaceReadFileException">File: string - путь к файлу.</exception>
        public static Surface FromGridFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new SurfaceReadFileException(ReadFileExceptionKind.FileNotExists)
                    .WithAdditionalData("File", filePath);
            try
            {
                using var reader = File.OpenText(filePath);
                reader.ReadLine();
                var counts = reader.ReadLine().Split(' ')
                    .Select(x => int.Parse(x, NumberStyles.Any, NumberFormatInfo.InvariantInfo))
                    .ToArray();
                var xLimits = reader.ReadLine().Split(' ')
                    .Select(x => double.Parse(x, NumberStyles.Any, NumberFormatInfo.InvariantInfo))
                    .ToArray();
                var yLimits = reader.ReadLine().Split(' ')
                    .Select(x => double.Parse(x, NumberStyles.Any, NumberFormatInfo.InvariantInfo))
                    .ToArray();
                var zLimits = reader.ReadLine().Split(' ')
                    .Select(x => double.Parse(x, NumberStyles.Any, NumberFormatInfo.InvariantInfo))
                    .ToArray();

                var values = new double[counts[0] * counts[1]];
                var buffer = new char[65536]; // 128 КБ.
                var number = "";
                var i = 0;
                while (!reader.EndOfStream)
                {
                    for (int j = 0; j < buffer.Length; j++)
                        buffer[j] = '\0';
                    reader.Read(buffer, 0, buffer.Length);
                    for (int j = 0; j < buffer.Length; j++)
                    {
                        var c = buffer[j];
                        if (c == '\0')
                        {
                            if (!string.IsNullOrEmpty(number))
                                values[i] = double.Parse(number, NumberStyles.Any, NumberFormatInfo.InvariantInfo);
                            break;
                        }

                        if (char.IsSeparator(c) || c == '\n' || c == '\r')
                        {
                            if (string.IsNullOrEmpty(number)) continue;
                            values[i] = double.Parse(number, NumberStyles.Any, NumberFormatInfo.InvariantInfo);
                            number = "";
                            i++;
                        }
                        else
                        {
                            number += c;
                        }
                    }
                }
                return new Surface(xLimits[0], xLimits[1], yLimits[0], yLimits[1], counts[0], counts[1], values);
            }
            catch (Exception exc)
            {
                throw new SurfaceReadFileException(ReadFileExceptionKind.UnableToReadFile, exc)
                    .WithAdditionalData("File", filePath);
            }
        }

        /// <summary>
        /// Записывает поверхность в текстовый .grd файл.
        /// </summary>
        /// <param name="filePath">Путь к .grd файлу.</param>
        /// <param name="addEmptyLineToEnd">
        /// <see cref="true" />, если необходимо добавить пустую строку в конец файла
        /// (для GST).
        /// </param>
        /// <returns></returns>
        public void ToGridFile(string filePath, bool addEmptyLineToEnd = false)
        {
            using (var writer = File.CreateText(filePath))
            {
                writer.WriteLine("DSAA");
                writer.WriteLine(
                    $"{XCount.ToString(CultureInfo.InvariantCulture)} {YCount.ToString(CultureInfo.InvariantCulture)}");
                writer.WriteLine(
                    $"{XMin.ToString(CultureInfo.InvariantCulture)} {XMax.ToString(CultureInfo.InvariantCulture)}");
                writer.WriteLine(
                    $"{YMin.ToString(CultureInfo.InvariantCulture)} {YMax.ToString(CultureInfo.InvariantCulture)}");
                writer.WriteLine(
                    $"{ZMin.ToString(CultureInfo.InvariantCulture)} {ZMax.ToString(CultureInfo.InvariantCulture)}");
                for (var i = 0; i < _values.Length - 1; i++)
                {
                    writer.Write(_values[i].ToString(CultureInfo.InvariantCulture));
                    writer.Write(' ');
                }

                writer.Write(_values.Last().ToString(CultureInfo.InvariantCulture));
                if (addEmptyLineToEnd) writer.WriteLine();
            }
        }

        /// <summary>
        /// Записывает поверхность в текстовый .grd файл.
        /// </summary>
        /// <param name="filePath">Путь к .grd файлу.</param>
        /// <param name="addEmptyLineToEnd">
        /// <see cref="true" />, если необходимо добавить пустую строку в конец файла
        /// (для GST).
        /// </param>
        /// <returns></returns>
        public async Task ToGridFileAsync(string filePath, bool addEmptyLineToEnd = false)
        {
            using (var writer = File.CreateText(filePath))
            {
                await writer.WriteLineAsync("DSAA");
                await writer.WriteLineAsync(
                    $"{XCount.ToString(CultureInfo.InvariantCulture)} {YCount.ToString(CultureInfo.InvariantCulture)}");
                await writer.WriteLineAsync(
                    $"{XMin.ToString(CultureInfo.InvariantCulture)} {XMax.ToString(CultureInfo.InvariantCulture)}");
                await writer.WriteLineAsync(
                    $"{YMin.ToString(CultureInfo.InvariantCulture)} {YMax.ToString(CultureInfo.InvariantCulture)}");
                await writer.WriteLineAsync(
                    $"{ZMin.ToString(CultureInfo.InvariantCulture)} {ZMax.ToString(CultureInfo.InvariantCulture)}");
                for (var i = 0; i < _values.Length - 1; i++)
                {
                    await writer.WriteAsync(_values[i].ToString(CultureInfo.InvariantCulture));
                    await writer.WriteAsync(' ');
                }

                await writer.WriteAsync(_values.Last().ToString(CultureInfo.InvariantCulture));
                if (addEmptyLineToEnd) await writer.WriteLineAsync();
            }
        }

        /// <summary>
        /// Производит подсчёт коэффициентов сплайнов поверхности.
        /// </summary>
        private void CalculateCoefficients()
        {
            // Я понятия не имею, что здесь происходит. Этот код взят из C++ программы Андрея Гарьевича.
            var coefficients = new double[(XCount + 2) * (YCount + 2) + 1];
            for (var i = 1; i <= XCount; i++)
            for (var j = 1; j <= YCount; j++)
                coefficients[(i - 1) * YCount + j - 1] = _values[(j - 1) * XCount + i - 1];
            int n1 = XCount + 2,
                m1 = YCount + 2,
                n11 = XCount - 1,
                m11 = YCount - 1,
                i1 = XCount * YCount,
                i2 = (n1 - 1) * m1 - 1;
            for (var i = 1; i <= XCount; i++, i2 -= 2)
            for (var j = 1; j <= YCount; j++, i1--, i2--)
                coefficients[i2 - 1] = coefficients[i1 - 1];
            int k = XCount * m1;
            for (var _in = m1; _in <= k; _in += m1)
            {
                for (var i = 3; i <= YCount; i++)
                {
                    int l = i + _in;
                    coefficients[l - 1] = (coefficients[l - 2] - 6 * coefficients[l - 1]) *
                                          _qk[Math.Min(10, i - 3)];
                }

                int lin = YCount + _in;
                coefficients[lin - 1] += coefficients[lin] * _qk[Math.Min(10, YCount - 3)];
                coefficients[lin + 1] = 2.0 * coefficients[lin] - coefficients[lin - 1];
                for (var j = 3; j <= m11; j++)
                {
                    int i = YCount - j + 2;
                    int l = i + _in;
                    coefficients[l - 1] += coefficients[l] * _qk[Math.Min(10, i - 3)];
                }

                coefficients[_in] = 2.0 * coefficients[_in + 1] - coefficients[_in + 2];
            }

            for (var _in = 1; _in <= m1; _in++)
            {
                for (var i = 3; i <= XCount; i++)
                {
                    var l = (i - 1) * m1 + _in;
                    coefficients[l - 1] = (coefficients[l - m1 - 1] - 6.0 * coefficients[l - 1]) *
                                          _qk[Math.Min(10, i - 3)];
                }

                k = (XCount - 1) * m1 + _in;
                coefficients[k - 1] += coefficients[k + m1 - 1] * _qk[Math.Min(10, XCount - 3)];
                coefficients[k + 2 * m1 - 1] = 2.0 * coefficients[k + m1 - 1] - coefficients[k - 1];
                for (var j = 3; j <= n11; j++)
                {
                    var i = XCount - j + 2;
                    var l = (i - 1) * m1 + _in;
                    coefficients[l - 1] += coefficients[l + m1 - 1] * _qk[Math.Min(10, i - 3)];
                }

                coefficients[_in - 1] = 2.0 * coefficients[_in + m1 - 1] - coefficients[_in + 2 * m1 - 1];
            }

            _coefficients = coefficients;
        }

        public double GetZ(double x, double y)
        {
            if (x > XMax || x < XMin)
                throw new SurfaceGetZException(GetZExceptionKind.XOutOfBounds)
                    .WithAdditionalData(nameof(x), x)
                    .WithAdditionalData(nameof(XMax), XMax)
                    .WithAdditionalData(nameof(XMin), XMin);
            if (y > YMax || y < YMin)
                throw new SurfaceGetZException(GetZExceptionKind.YOutOfBounds)
                    .WithAdditionalData(nameof(y), y)
                    .WithAdditionalData(nameof(YMax), YMax)
                    .WithAdditionalData(nameof(YMin), YMin);

            // Я также не знаю, что происходит здесь.
            const double kb = 1.0 / 6;
            var fi = (int) ((x - XMin) / XStep);
            var fj = (int) ((y - YMin) / YStep);
            if (fi >= XCount - 1) fi = XCount - 2;
            if (fj >= YCount - 1) fj = YCount - 2;
            var tx = (x - XMin) / XStep - fi;
            var ty = (y - YMin) / YStep - fj;
            var bx = new[]
            {
                0,
                kb * Math.Pow(1 - tx, 3),
                kb * (4 - 6 * Math.Pow(tx, 2) + 3 * Math.Pow(tx, 3)),
                kb * (1 + 3 * (tx + Math.Pow(tx, 2) - Math.Pow(tx, 3))),
                kb * Math.Pow(tx, 3)
            };
            var by = new[]
            {
                0,
                kb * Math.Pow(1 - ty, 3),
                kb * (4 - 6 * Math.Pow(ty, 2) + 3 * Math.Pow(ty, 3)),
                kb * (1 + 3 * (ty + Math.Pow(ty, 2) - Math.Pow(ty, 3))),
                kb * Math.Pow(ty, 3)
            };
            var ret = .0;
            for (var i = 1; i <= 4; i++)
            {
                double sko = 0;
                for (var j = 1; j <= 4; j++)
                    sko += Coefficients.ElementAt((fi + i - 1) * (YCount + 2) + j + fj - 1) * by[j];
                ret += sko * bx[i];
            }

            return ret;
        }

        /// <summary>
        /// Возвращает значение производной z по x в указанной точке.
        /// </summary>
        /// <param name="x">Координата x в точке.</param>
        /// <param name="y">Координата y в точке.</param>
        /// <param name="derivative">Производная.</param>
        /// <exception cref="SurfaceGetZException">
        /// Дополнительные данные:
        /// x, XMax, XMin для <see cref="GetZExceptionKind.XOutOfBounds" />;
        /// y, YMax, YMin для <see cref="GetZExceptionKind.YOutOfBounds" />;
        /// derivative для <see cref="GetZExceptionKind.UnknownDerivative" />.
        /// </exception>
        public double GetZ(double x, double y, string derivative)
        {
            if (x > XMax || x < XMin)
                throw new SurfaceGetZException(GetZExceptionKind.XOutOfBounds)
                    .WithAdditionalData(nameof(x), x)
                    .WithAdditionalData(nameof(XMax), XMax)
                    .WithAdditionalData(nameof(XMin), XMin);
            if (y > YMax || y < YMin)
                throw new SurfaceGetZException(GetZExceptionKind.YOutOfBounds)
                    .WithAdditionalData(nameof(y), y)
                    .WithAdditionalData(nameof(YMax), YMax)
                    .WithAdditionalData(nameof(YMin), YMin);

            var dx = 2 + (x - XMin) / XStep;
            var dy = 2 + (y - YMin) / YStep;
            var row = (int) dy;
            var column = (int) dx;
            var rowStart = Math.Max(1, row - 1);
            var rowEnd = Math.Min(row + 2, YCount + 2);
            var columnStart = Math.Max(1, column - 1);
            var columnEnd = Math.Min(column + 2, XCount + 2);
            var ret = .0;

            switch (derivative.ToLowerInvariant())
            {
                case "x":
                    for (int i = rowStart; i <= rowEnd; i++)
                    {
                        var time = .0;
                        for (int j = columnStart; j <= columnEnd; j++)
                            time += _coefficients[(j - 1) * (YCount + 2) + (i - 1)] * GetSplineValueS(j, dx);
                        ret += time * GetSplineValue(i, dy);
                    }

                    return ret / XStep;
                case "y":
                    for (int i = rowStart; i <= rowEnd; i++)
                    {
                        var time = .0;
                        for (int j = columnStart; j <= columnEnd; j++)
                            time += _coefficients[(j - 1) * (YCount + 2) + (i - 1)] * GetSplineValue(j, dx);
                        ret += time * GetSplineValueS(i, dy);
                    }

                    return ret / XStep;
                case "xy":
                    for (int i = rowStart; i <= rowEnd; i++)
                    {
                        var time = .0;
                        for (int j = columnStart; j <= columnEnd; j++)
                            time += _coefficients[(j - 1) * (YCount + 2) + (i - 1)] * GetSplineValueS(j, dx);
                        ret += time * GetSplineValueS(i, dy);
                    }

                    return ret / XStep / YStep;
                case "xx":
                    for (int i = rowStart; i <= rowEnd; i++)
                    {
                        var time = .0;
                        for (int j = columnStart; j <= columnEnd; j++)
                            time += _coefficients[(j - 1) * (YCount + 2) + (i - 1)] * GetSplineValueSS(j, dx);
                        ret += time * GetSplineValue(i, dy);
                    }

                    return ret / Math.Pow(XStep, 2);
                case "yy":
                    for (int i = rowStart; i <= rowEnd; i++)
                    {
                        var time = .0;
                        for (int j = columnStart; j <= columnEnd; j++)
                            time += _coefficients[(j - 1) * (YCount + 2) + (i - 1)] * GetSplineValue(j, dx);
                        ret += time * GetSplineValueSS(i, dy);
                    }

                    return ret / Math.Pow(YStep, 2);
                default:
                    throw new SurfaceGetZException(GetZExceptionKind.UnknownDerivative)
                        .WithAdditionalData(nameof(derivative), derivative);
            }
        }

        /// <summary>
        /// Возвращает значение сплайна.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public double GetSplineValue(int i, double x)
        {
            var del = x - i;
            if (del > -2 && del <= -1)
                return Math.Pow(2 + del, 3) / 6;
            if (del > -1 && del <= 0)
                return 2.0 / 3 - Math.Pow(del, 2) - Math.Pow(del, 3) / 2;
            if (del > 0 && del <= 1)
                return 2.0 / 3 - Math.Pow(del, 2) + Math.Pow(del, 3) / 2;
            if (del > 1 && del < 2)
                return Math.Pow(2 - del, 3) / 6;
            return 0;
        }

        /// <summary>
        /// Возвращает значение сплайна по первой производной.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public double GetSplineValueS(int i, double x)
        {
            var del = x - i;
            if (del > -2 && del <= -1)
                return Math.Pow(2 + del, 2) / 2;
            if (del > -1 && del <= 0)
                return -2 * del - 1.5 * Math.Pow(del, 2);
            if (del > 0 && del <= 1)
                return -2 * del + 1.5 * Math.Pow(del, 2);
            if (del > 1 && del < 2)
                return -Math.Pow(2 - del, 2) / 2;
            return 0;
        }

        /// <summary>
        /// Возвращает значение сплайна по второй производной.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public double GetSplineValueSS(int i, double x)
        {
            var del = x - i;
            if (del > -2 && del <= -1)
                return 2 + del;
            if (del > -1 && del <= 0)
                return -2 - 3 * del;
            if (del > 0 && del <= 1)
                return -2 + 3 * del;
            if (del > 1 && del < 2)
                return 2 - del;
            return 0;
        }

        /// <summary>
        /// Возвращает результат применения пользовательской функции к значениям поверхности.
        /// </summary>
        /// <param name="func">Пользовательская функция, принимающая в качестве аргументов значения x, y и z в узлах поверхности.</param>
        public Surface Apply(Func<double, double, double, double> func)
        {
            var values = new double[_values.Length];
            for (int i = 0; i < YCount; i++)
            for (int j = 0; j < XCount; j++)
                values[i * XCount + j] = func.Invoke(
                    XMin + XStep * j, YMin + YStep * i, _values[i * XCount + j]);
            return new Surface(XMin, XMax, YMin, YMax, XCount, YCount, values);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Surface ({XCount}, {YCount}) [X: ({XMin}, {XMax}), Y: ({YMin}, {YMax}), Z: ({ZMin}, {ZMax})]";
        }
    }
}