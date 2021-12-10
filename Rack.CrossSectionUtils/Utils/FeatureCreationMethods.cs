using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Rack.CrossSectionUtils.Abstractions.Model;
using Rack.CrossSectionUtils.Extensions;
using Rack.CrossSectionUtils.Model;
using Rack.CrossSectionUtils.Model.AttributeTable;
using Rack.CrossSectionUtils.Validation.Messages;
using Rack.CrossSectionUtils.Validation.Validators;
using Rack.GeoTools.Extensions;
using UnitsNet;
using UnitsNet.Units;

namespace Rack.CrossSectionUtils.Utils
{
    /// <summary>
    /// Методы для построения колонок оформления.
    /// </summary>
    public static class FeatureCreationMethods
    {
        /// <summary>
        /// Создаёт колонки оформления.
        /// </summary>
        /// <typeparam name="TRuler">Тип колонки оформления, представляющая линейку.</typeparam>
        /// <typeparam name="TDecorationColumnWithRecords">Тип колонки оформления
        /// с фиксированными границами ячеек колонки.</typeparam>
        /// <typeparam name="TRecord">Тип ячекйи колонки оформления
        /// для <see cref="TDecorationColumnWithRecords"/>.</typeparam>
        /// <param name="crossSectionBuildSettings">Параметры построения разреза.</param>
        /// <param name="decorationColumnsSettings">Параметры построения колонок оформления.</param>
        /// <param name="crossSectionWidth">Ширина разреза в системе координат отрисовки разреза.</param>
        /// <returns>Перечисление <see cref="Feature"/>, которое представляет колонки оформления.</returns>
        /// <exception cref="ValidationException">,
        /// если некорректное состояние
        /// <see cref="IDecorationColumnsSettings{TRuler,TDecorationColumn,TRecord}.Ruler"/>
        /// или <see cref="IDecorationColumnsSettings{TRuler,TDecorationColumn,TRecord}.DecorationColumnsWithRecords"/>.</exception>>
        public static IEnumerable<Feature> CreateDecorationColumns<TRuler, TDecorationColumnWithRecords, TRecord>(
            this ICrossSectionBuildSettings crossSectionBuildSettings,
            IDecorationColumnsSettings<TRuler, TDecorationColumnWithRecords, TRecord> decorationColumnsSettings,
            Length crossSectionWidth)
            where TRuler : IDecorationColumn
            where TDecorationColumnWithRecords : IDecorationColumnWithRecords<TRecord>
            where TRecord : IDecorationColumnRecord
        {
            var ret = Enumerable.Empty<Feature>();

            /* Счётчики количества отрисованных колонок. */
            var leftColumnCount = 0;
            var rightColumnCount = 0;

            /* Формируем колонки слева. */
            foreach (var column in decorationColumnsSettings.DecorationColumnsWithRecords
                .Where(x => x.Mode.HasLeft()))
            {
                var xLeftBorder = new Length(0, LengthUnit.Meter)
                                  + leftColumnCount * crossSectionBuildSettings.DecorationColumnsWidth;
                ret = ret.Union(
                    crossSectionBuildSettings.CreateDecorationColumn<TDecorationColumnWithRecords, TRecord>(
                        column,
                        DecorationColumnMode.Left,
                        xLeftBorder));
                leftColumnCount++;
            }

            if (decorationColumnsSettings.Ruler.Mode.HasLeft())
            {
                var xLeftBorder = new Length(0, LengthUnit.Meter)
                                  + leftColumnCount * crossSectionBuildSettings.DecorationColumnsWidth;
                var scaleRuler = crossSectionBuildSettings
                    .CreateScaleRuler<IDecorationColumn>(
                        decorationColumnsSettings.Ruler, DecorationColumnMode.Left,
                        xLeftBorder);
                ret = ret.Union(scaleRuler);
                leftColumnCount++;
            }

            /* Граница по оси X от которой начинают сторится колонки оформления справа. */
            var rightColumnsStartBorder = new Length(0, LengthUnit.Meter)
                                          + leftColumnCount * crossSectionBuildSettings.DecorationColumnsWidth
                                          + crossSectionWidth;

            /* Формируем колонки справа. */
            if (decorationColumnsSettings.Ruler.Mode.HasRight())
            {
                var scaleRuler = crossSectionBuildSettings
                    .CreateScaleRuler(
                        decorationColumnsSettings.Ruler, DecorationColumnMode.Right,
                        rightColumnsStartBorder);
                ret = ret.Union(scaleRuler);
                rightColumnCount++;
            }

            foreach (var column in decorationColumnsSettings.DecorationColumnsWithRecords
                .Where(x => x.Mode.HasRight())
                .Reverse())
            {
                var xLeftBorder = rightColumnsStartBorder
                                  + rightColumnCount * crossSectionBuildSettings.DecorationColumnsWidth;
                ret = ret.Union(
                    crossSectionBuildSettings.CreateDecorationColumn<TDecorationColumnWithRecords, TRecord>(
                        column,
                        DecorationColumnMode.Right,
                        xLeftBorder));
                rightColumnCount++;
            }

            return ret;
        }

        /// <summary>
        /// Создаёт колонку оформления.
        /// </summary>
        /// <typeparam name="TDecorationColumnWithRecords">Тип колонки оформления
        /// с фиксированными границами ячеек колонки.</typeparam>
        /// <typeparam name="TRecord">Тип ячекйи колонки оформления
        /// для <see cref="TDecorationColumnWithRecords"/>.</typeparam>
        /// <param name="settings">Параметры построения разреза.</param>
        /// <param name="decorationColumn">Колонка оформления.</param>
        /// <param name="columnMode">Режим отрисовки колонки.
        /// Так как метод ориентирован на построение отдельной колонки,
        /// поддерживаются только режимы:
        /// <see cref="DecorationColumnMode.Left"/>,
        /// <see cref="DecorationColumnMode.Right"/>.</param>
        /// <param name="xLeftBorder">Левая граница колонки оформления по оси X.</param>
        /// <returns>Перечисление <see cref="Feature"/>, которое представляет колонку оформления.</returns>
        /// <exception cref="ValidationException">,
        /// если некорректное состояние <see cref="decorationColumn"/>.</exception>
        /// <exception cref="ArgumentException">, если указан недопустимый
        /// режим отрисовки <see cref="columnMode"/>.</exception>
        private static IEnumerable<Feature> CreateDecorationColumn<TDecorationColumnWithRecords, TRecord>(
            this ICrossSectionBuildSettings settings,
            TDecorationColumnWithRecords decorationColumn,
            DecorationColumnMode columnMode,
            Length xLeftBorder)
            where TDecorationColumnWithRecords : IDecorationColumnWithRecords<TRecord>
            where TRecord : IDecorationColumnRecord
        {
            new DecorationColumnWithRecordsValidator<TDecorationColumnWithRecords, TRecord>(
                    new DecorationColumnWithRecordsValidationMessages(),
                    new DecorationColumnValidator<TDecorationColumnWithRecords>(new DecorationColumnValidationMessages()),
                    new DecorationColumnRecordValidator<TRecord>(new DecorationColumnRecordValidationMessages()))
                .ValidateAndThrow(decorationColumn);

            if (columnMode != DecorationColumnMode.Left && columnMode != DecorationColumnMode.Right)
                throw new ArgumentException(
                    $"Режим отрисовки должен быть или {DecorationColumnMode.Left} или {DecorationColumnMode.Right}");

            var crossSectionHeight = settings.GetHeight();
            var positionAttributeValue = columnMode == DecorationColumnMode.Left
                ? "Left"
                : "Right";

            var headerFeature = CreateHeaderFeature(
                new Coordinate(xLeftBorder.Centimeters, crossSectionHeight.Centimeters),
                settings.DecorationColumnsWidth.Centimeters,
                settings.DecorationHeadersHeight.Centimeters,
                decorationColumn.Header);
            headerFeature.Attributes.Add(
                AttributeTableNamesConstants.Position, positionAttributeValue);
            yield return headerFeature;

            var backgroundFeature = CreateBackgroundFeature(
                new Coordinate(xLeftBorder.Centimeters, 0),
                settings.DecorationColumnsWidth.Centimeters,
                crossSectionHeight.Centimeters);
            backgroundFeature.Attributes.Add(
                AttributeTableNamesConstants.Position, positionAttributeValue);
            yield return backgroundFeature;

            var metersAtCentimeter = settings.GetMetersToCentimeterAtVertical();

            foreach (var record in decorationColumn.Records)
            {
                var top = columnMode == DecorationColumnMode.Left
                    ? record.LeftTop
                    : record.RightTop;
                var bottom = columnMode == DecorationColumnMode.Left
                    ? record.LeftBottom
                    : record.RightBottom;
                /* Если запись за границами разреза - её не нужно отрисовывать. */
                if (bottom >= settings.Top || top <= settings.Bottom)
                    continue;

                /* Ограничиваем границы записей колонки границами разреза. */
                top = top <= settings.Top
                    ? top
                    : settings.Top;
                bottom = bottom >= settings.Bottom
                    ? bottom
                    : settings.Bottom;

                var y = Math.Round((bottom - settings.Bottom).Meters / metersAtCentimeter, 2);
                var contentFeature = CreateContentFeature(
                    new Coordinate(xLeftBorder.Centimeters, y),
                    settings.DecorationColumnsWidth.Centimeters,
                    Math.Round((top - bottom).Meters / metersAtCentimeter, 2),
                    record.Text);
                contentFeature.Attributes.Add(
                    AttributeTableNamesConstants.Position, positionAttributeValue);
                yield return contentFeature;
            }
        }

        /// <summary>
        /// Создаёт линейку глубин для разреза.
        /// </summary>
        /// <param name="settings">Параметры построения разреза.</param>
        /// <param name="ruler">Линейка.</param>
        /// <param name="columnMode">Режим отрисовки колонки.
        /// Так как метод ориентирован на построение отдельной колонки,
        /// поддерживаются только режимы:
        /// <see cref="DecorationColumnMode.Left"/>,
        /// <see cref="DecorationColumnMode.Right"/>.</param>
        /// <param name="xLeftBorder">Левая граница линейки по оси X.</param>
        /// <returns>Перечисление <see cref="Feature"/>, которое представляет линейку.</returns>
        /// <exception cref="ValidationException">,
        /// если некорректное состояние <see cref="ruler"/>.</exception>
        /// <exception cref="ArgumentException">, если указан недопустимый
        /// режим отрисовки <see cref="columnMode"/>.</exception>
        private static IEnumerable<Feature> CreateScaleRuler<TRuler>(
            this ICrossSectionBuildSettings settings,
            TRuler ruler,
            DecorationColumnMode columnMode,
            Length xLeftBorder)
            where TRuler : IDecorationColumn
        {
            new DecorationColumnValidator<TRuler>(new DecorationColumnValidationMessages())
                .ValidateAndThrow(ruler);
            if (columnMode != DecorationColumnMode.Left && columnMode != DecorationColumnMode.Right)
                throw new ArgumentException(
                    $"Режим отрисовки должен быть или {DecorationColumnMode.Left} или {DecorationColumnMode.Right}");


            var crossSectionHeight = settings.GetHeight();
            var positionAttributeValue = columnMode == DecorationColumnMode.Left
                ? "Left"
                : "Right";

            var headerFeature = CreateHeaderFeature(
                new Coordinate(xLeftBorder.Centimeters, crossSectionHeight.Centimeters),
                settings.DecorationColumnsWidth.Centimeters,
                settings.DecorationHeadersHeight.Centimeters,
                ruler.Header);
            headerFeature.Attributes.Add(
                AttributeTableNamesConstants.Position, positionAttributeValue);
            yield return headerFeature;

            var backgroundFeature = CreateBackgroundFeature(
                new Coordinate(xLeftBorder.Centimeters, 0),
                settings.DecorationColumnsWidth.Centimeters,
                crossSectionHeight.Centimeters);
            backgroundFeature.Attributes.Add(
                AttributeTableNamesConstants.Position, positionAttributeValue);
            yield return backgroundFeature;

            var stepInMeters = settings.GetMetersToCentimeterAtVertical();
            /* Текущая отметка по шкале в метрах. */
            var currentScaleInMeters = (int) settings.Top.Meters - stepInMeters;
            /* Между самой верхней отметкой линейки и верхней границой разреза
             * должно быть расстояние как минимум  в один шаг
             * (для корректного отображения линейки). */
            currentScaleInMeters = currentScaleInMeters.GetStepConsistency(
                stepInMeters, StepConsistencyMode.LessThanSource);
            while (currentScaleInMeters >= settings.Bottom.Meters)
            {
                var y = Math.Round((currentScaleInMeters - settings.Bottom.Meters) / stepInMeters, 2);
                var scaleFeature = CreateScaleFeature(
                    new Coordinate(xLeftBorder.Centimeters, y),
                    settings.DecorationColumnsWidth.Centimeters,
                    1,
                    currentScaleInMeters);
                currentScaleInMeters -= stepInMeters;
                scaleFeature.Attributes.Add(
                    AttributeTableNamesConstants.Position, positionAttributeValue);
                yield return scaleFeature;
            }
        }


        /// <summary>
        /// Создаёт прямоугольный полигон с атрибутивной информацией,
        /// которая представляет задний фон колонки оформления.
        /// </summary>
        /// <param name="downLeftCorner">Точка нижнего левого угла прямоугольника.</param>
        /// <param name="width">Ширина прямоугольника.</param>
        /// <param name="height">Высота прямоугольника.</param>
        /// <returns></returns>
        private static Feature CreateBackgroundFeature(
            Coordinate downLeftCorner,
            double width,
            double height) =>
            CreateRectangleFeature(downLeftCorner, width, height,
                AttributeTableTypeValueConstants.Background);

        /// <summary>
        /// Создаёт прямоугольный полигон с атрибутивной информацией,
        /// которая представляет ячейку колонки оформления с некоторой надписью.
        /// </summary>
        /// <param name="downLeftCorner">Точка нижнего левого угла прямоугольника.</param>
        /// <param name="width">Ширина прямоугольника.</param>
        /// <param name="height">Высота прямоугольника.</param>
        /// <param name="content"></param>
        private static Feature CreateContentFeature(
            Coordinate downLeftCorner,
            double width,
            double height,
            string content) =>
            CreateRectangleFeature(downLeftCorner, width, height,
                AttributeTableTypeValueConstants.Content, content);

        /// <summary>
        /// Создаёт прямоугольный полигон с атрибутивной информацией,
        /// которая представляет числовую отметку на колонке оформления.
        /// </summary>
        /// <param name="downLeftCorner">Точка нижнего левого угла прямоугольника.</param>
        /// <param name="width">Ширина прямоугольника.</param>
        /// <param name="height">Высота прямоугольника.</param>
        /// <param name="scale"></param>
        private static Feature CreateScaleFeature(
            Coordinate downLeftCorner,
            double width,
            double height,
            double scale) =>
            CreateRectangleFeature(downLeftCorner, width, height,
                AttributeTableTypeValueConstants.Scale, scale);

        /// <summary>
        /// Создаёт прямоугольный полигон с атрибутивной информацией,
        /// которая представляет заголовок колонки оформления.
        /// </summary>
        /// <param name="downLeftCorner">Точка нижнего левого угла прямоугольника.</param>
        /// <param name="width">Ширина прямоугольника.</param>
        /// <param name="height">Высота прямоугольника.</param>
        /// <param name="header">Заголовок.</param>
        private static Feature CreateHeaderFeature(
            Coordinate downLeftCorner,
            double width,
            double height,
            string header) =>
            CreateRectangleFeature(downLeftCorner, width, height,
                AttributeTableTypeValueConstants.Header, header);

        /// <summary>
        /// Создаёт прямоугольный полигон с атрибутивной информацией
        /// о типе полигона и метке.
        /// </summary>
        /// <param name="downLeftCorner">Точка нижнего левого угла прямоугольника.</param>
        /// <param name="width">Ширина прямоугольника.</param>
        /// <param name="height">Высота прямоугольника.</param>
        /// <param name="type">Тип полигона.</param>
        /// <param name="label">Значение метки (по умолчанию пустая строка).</param>
        private static Feature CreateRectangleFeature(
            Coordinate downLeftCorner,
            double width,
            double height,
            string type,
            object label = null)
        {
            if (label is null)
                label = string.Empty;

            var attributes = new[]
            {
                new KeyValuePair<string, object>(AttributeTableNamesConstants.Label, label),
                new KeyValuePair<string, object>(AttributeTableNamesConstants.Type, type),
            };
            return CreateRectangleFeature(downLeftCorner, width, height, attributes);
        }

        /// <summary>
        /// Создаёт прямоугольный полигон с атрибутивной информацией.
        /// </summary>
        /// <param name="downLeftCorner">Точка нижнего левого угла прямоугольника.</param>
        /// <param name="width">Ширина прямоугольника.</param>
        /// <param name="height">Высота прямоугольника.</param>
        /// <param name="attributes">Атрибутивная информация
        /// (ключ - имя атрибута, значение - значение атрибута).</param>
        private static Feature CreateRectangleFeature(
            Coordinate downLeftCorner,
            double width,
            double height,
            IEnumerable<KeyValuePair<string, object>> attributes) =>
            new Feature(downLeftCorner.CreateRectangle(
                    new Coordinate(downLeftCorner.X + width, downLeftCorner.Y + height)),
                new AttributesTable(attributes));

        /// <summary>
        /// Добавляет ко всем полигонам к атрибутивной информации,
        /// что полигон находится слева от разреза.
        /// </summary>
        /// <typeparam name="T">Тип, представляющий полигон с атрибутивной информацией.</typeparam>
        /// <param name="features">Перечисление полигонов.</param>
        private static IEnumerable<T> AddLeftPositionAttribute<T>(
            this IEnumerable<T> features)
            where T : IFeature
        {
            return features.AddAttribute(AttributeTableNamesConstants.Position, "Left");
        }

        /// <summary>
        /// Добавляет ко всем полигонам к атрибутивной информации,
        /// что полигон находится справа от разреза.
        /// </summary>
        /// <typeparam name="T">Тип, представляющий полигон с атрибутивной информацией.</typeparam>
        /// <param name="features">Перечисление полигонов.</param>
        private static IEnumerable<T> AddRightPositionAttribute<T>(
            this IEnumerable<T> features)
            where T : IFeature
        {
            return features.AddAttribute(AttributeTableNamesConstants.Position, "Right");
        }

        /// <summary>
        /// Добавляет ко всем полигонам к атрибутивной информации указанное значение.
        /// </summary>
        /// <typeparam name="T">Тип, представляющий полигон с атрибутивной информацией.</typeparam>
        /// <param name="features">Перечисление полигонов.</param>
        /// <param name="attributeName">Имя атрибута.</param>
        /// <param name="value">Значение атрибута.</param>
        private static IEnumerable<T> AddAttribute<T>(
            this IEnumerable<T> features,
            string attributeName,
            object value)
            where T : IFeature
        {
            foreach (var feature in features)
            {
                feature.Attributes.Add(attributeName, value);
                yield return feature;
            }
        }
    }
}