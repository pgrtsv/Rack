using System;
using System.Diagnostics.CodeAnalysis;

namespace Rack.GeoTools.Extensions
{
    /// <summary>
    /// Способ расчёта ближайшего значения, кратному шагу.
    /// </summary>
    public enum StepConsistencyMode
    {
        /// <summary>
        /// Приведение к ближайшему кратному, которое больше исходного значения.
        /// </summary>
        MoreThanSource,
        /// <summary>
        /// Приведение к ближайшему кратному, которое меньше исходного значения.
        /// </summary>
        LessThanSource
    }

    /// <summary>
    /// Методы расширения для работы с осью координат.
    /// </summary>
    public static class AxisEx
    {
        /// <summary>
        /// Возвращает ближайшее к <see cref="value"/> значение, кратное <see cref="step"/>.
        /// </summary>
        /// <param name="value">Значение, приводимое к кратному.</param>
        /// <param name="step">Шаг.</param>
        /// <param name="mode">Способ расчёта ближайшего значения.</param>
        /// <param name="precision">Точность сравнения.</param>
        /// <returns>Значение кратное шагу,
        /// округленное до заданной точности <see cref="precision"/>
        /// (округляется по правилу <see cref="MidpointRounding.AwayFromZero"/>).</returns>
        /// <exception cref="ArgumentException">, если:
        /// шаг меньше или равен нулю,
        /// или точность меньше нуля.</exception>
        /// <exception cref="NotImplementedException">, если не реализованный
        /// способ приведения к шагу.</exception>
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public static double GetStepConsistency(
            this double value, double step,
            StepConsistencyMode mode, int precision)
        {
            if(step <= 0)
                throw new ArgumentException("Шаг должен быть больше нуля.");
            if(precision < 0)
                throw new ArgumentException("Точность должна быть больше или равна нулю.");

            /* Используем Math.IEEERemainder, а не оператор "%", чтобы получить такое число,
             * которое при вычитании из value даст ближайшее кратное шагу.
             * Тогда как оператор "%" даст такое число, которое при вычитании из value
             * даст НАИМЕНЬШЕЕ ближайшее кратное.
             * Это актуально, поскольку из-за потери точности, при кратности value шагу,
             * в результате вычитание результата оператора "%" из value,
             * уже кратное шагу значение value может уменьшиться на шаг. */
            var consistence = value - Math.IEEERemainder(value, step);
            consistence = Math.Round(consistence, precision, MidpointRounding.AwayFromZero);
            value = Math.Round(value, precision, MidpointRounding.AwayFromZero);
            if (consistence == value)
                return value;

            value = mode switch
            {
                StepConsistencyMode.MoreThanSource => consistence > value
                    ? consistence
                    : consistence + step,
                StepConsistencyMode.LessThanSource => consistence < value
                    ? consistence
                    : consistence - step,
                _ => throw new NotImplementedException(
                    $"Данный cпособ привидения значения к ближайшему кратному шагу (\"{mode}\") не поддерживается.")
            };
            return Math.Round(value, precision, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Возвращает ближайшее к <see cref="value"/> значение, кратное <see cref="step"/>.
        /// </summary>
        /// <param name="value">Значение, приводимое к кратному.</param>
        /// <param name="step">Шаг.</param>
        /// <param name="mode">Способ расчёта ближайшего значения.</param>
        /// <exception cref="ArgumentException">, если шаг меньше или равен нулю.</exception>
        /// <exception cref="NotImplementedException">, если не реализованный
        /// способ приведения к шагу.</exception>
        public static int GetStepConsistency(
            this int value, int step,
            StepConsistencyMode mode)
        {
            if (step <= 0)
                throw new ArgumentException("Шаг должен быть больше нуля.");

            if (step > Math.Abs(value))
                return mode == StepConsistencyMode.MoreThanSource
                    ? value > 0
                        ? 0 + step
                        : 0
                    : value > 0
                        ? 0
                        : 0 - step;

            var consistence = value - value % step;

            if (consistence == value)
                return value;

            return mode switch
            {
                StepConsistencyMode.MoreThanSource => consistence > value
                    ? consistence
                    : consistence + step,
                StepConsistencyMode.LessThanSource => consistence < value
                    ? consistence
                    : consistence - step,
                _ => throw new NotImplementedException(
                    $"Данный cпособ привидения значения к ближайшему кратному шагу (\"{mode}\") не поддерживается.")
            };
        }
    }
}