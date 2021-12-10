using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NetTopologySuite.Geometries;
using ReactiveUI;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using GeometryCollection = System.Windows.Media.GeometryCollection;
using LineSegment = System.Windows.Media.LineSegment;
using NtpGeometry = NetTopologySuite.Geometries.Geometry;
using NtpPoint = NetTopologySuite.Geometries.Point;
using Pen = System.Windows.Media.Pen;
using Polygon = NetTopologySuite.Geometries.Polygon;
using WpfPoint = System.Windows.Point;
using WpfPolygon = System.Windows.Shapes.Polygon;

namespace Rack.GeoTools.Wpf.Controls
{
    public class MapCanvas : Canvas
    {
        /// <summary>
        /// Количество пикселей WPF в одном сантиметре.
        /// </summary>
        private const double PxInCm = 96 / 2.54;

        public static readonly DependencyProperty LayersProperty = DependencyProperty.Register(
            "Layers",
            typeof(IEnumerable),
            typeof(MapCanvas),
            new PropertyMetadata(LayersChangedCallback));

        public static readonly DependencyProperty XProperty = DependencyProperty.Register(
            "X",
            typeof(double),
            typeof(MapCanvas));

        public static readonly DependencyProperty YProperty = DependencyProperty.Register(
            "Y",
            typeof(double),
            typeof(MapCanvas));

        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
            "Scale",
            typeof(double),
            typeof(MapCanvas),
            new FrameworkPropertyMetadata(ScaleChangedCallback) {BindsTwoWayByDefault = true});

        public static readonly DependencyProperty HorizontalScaleProperty =
            DependencyProperty.Register(
                "HorizontalScale",
                typeof(double),
                typeof(MapCanvas),
                new FrameworkPropertyMetadata(HorizontalScaleChangedCallback)
                    {BindsTwoWayByDefault = true});

        public static readonly DependencyProperty VerticalScaleProperty =
            DependencyProperty.Register(
                "VerticalScale",
                typeof(double),
                typeof(MapCanvas),
                new FrameworkPropertyMetadata(VerticalScaleChangedCallback)
                    {BindsTwoWayByDefault = true});

        public static readonly DependencyProperty IsUniformScaleProperty =
            DependencyProperty.Register(
                "IsUniformScale",
                typeof(bool),
                typeof(MapCanvas),
                new PropertyMetadata(true));

        public static readonly DependencyProperty IsAutoScalingProperty =
            DependencyProperty.Register(
                "IsAutoScaling",
                typeof(bool),
                typeof(MapCanvas));

        public static readonly DependencyProperty UniformScalingOffsetProperty =
            DependencyProperty.Register(
                "AutoScalingOffset",
                typeof(double),
                typeof(MapCanvas));

        protected double MaxX;
        protected double MaxY;
        protected double MinX;
        protected double MinY;
        protected double OffsetX;
        protected double OffsetY;

        public MapCanvas()
        {
            IsUniformScale = true;
            AutoScaleCommand = ReactiveCommand.Create(AutoScale);
            ClipToBounds = true;
        }

        /// <summary>
        /// Отступ от краёв карты (в пикселях WPF), который соблюдается при автоматическом масштабировании.
        /// </summary>
        public double AutoScalingOffset
        {
            get => (double) GetValue(UniformScalingOffsetProperty);
            set => SetValue(UniformScalingOffsetProperty, value);
        }

        public double HorizontalScale
        {
            get => (double) GetValue(HorizontalScaleProperty);
            set => SetValue(HorizontalScaleProperty, value);
        }

        public double VerticalScale
        {
            get => (double) GetValue(VerticalScaleProperty);
            set => SetValue(VerticalScaleProperty, value);
        }

        public bool IsUniformScale
        {
            get => (bool) GetValue(IsUniformScaleProperty);
            set => SetValue(IsUniformScaleProperty, value);
        }

        public bool IsAutoScaling
        {
            get => (bool) GetValue(IsAutoScalingProperty);
            set => SetValue(IsAutoScalingProperty, value);
        }

        public ICommand AutoScaleCommand { get; }

        public double X
        {
            get => (double) GetValue(XProperty);
            set => SetValue(XProperty, value);
        }

        public double Y
        {
            get => (double) GetValue(YProperty);
            set => SetValue(YProperty, value);
        }

        public IEnumerable<IDrawableLayer> Layers
        {
            get => (IEnumerable<IDrawableLayer>) GetValue(LayersProperty);
            set => SetValue(LayersProperty, value);
        }

        //TODO: заменить магическое число на нормальное вычисление количества пикселей в одном сантиметре.
        public double Scale
        {
            get => (double) GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        private bool IsEmpty => Layers == null || !Layers.Any();

        private static void HorizontalScaleChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var mapCanvas = (MapCanvas) d;
            if (mapCanvas.IsAutoScaling) return; 
            if (mapCanvas.IsEmpty) return;
            mapCanvas.DrawLayers();
        }

        private static void VerticalScaleChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var mapCanvas = (MapCanvas) d;
            if (mapCanvas.IsEmpty) return;
            if (mapCanvas.IsAutoScaling) return; 
            mapCanvas.DrawLayers();
        }

        public void AutoScale()
        {
            // Ширина и высота объектов на карте (в метрах).
            var featuresWidth = MaxX - MinX;
            var featuresHeight = MaxY - MinY;
            // Ширина и высота карты (в метрах).
            var actualWidth = (ActualWidth - AutoScalingOffset * 2) / PxInCm / 100;
            var actualHeight = (ActualHeight - AutoScalingOffset * 2) / PxInCm / 100;
            if (actualHeight < 0) actualHeight = ActualHeight / PxInCm / 100;
            if (actualWidth < 0) actualWidth = ActualWidth / PxInCm / 100;
            if (IsUniformScale)
            {
                if (featuresWidth > featuresHeight)
                    Scale = actualWidth / featuresWidth;
                else Scale = actualHeight / featuresHeight;
            }
            else
            {
                VerticalScale = actualHeight / featuresHeight;
                HorizontalScale = actualWidth / featuresWidth;
            }
        }

        protected static void LayersChangedCallback(
            DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            var mapCanvas = (MapCanvas) sender;

            if (e.OldValue is IEnumerable<IDrawableLayer> oldList)
            {
                if (oldList is INotifyCollectionChanged oldObservableCollection)
                    CollectionChangedEventManager.RemoveHandler(
                        oldObservableCollection,
                        mapCanvas.OnLayersCollectionChanged);
                foreach (var layer in oldList)
                    mapCanvas.UnsubscribeFromLayersChanges(layer);
            }

            if (!(e.NewValue is IEnumerable<IDrawableLayer> newLayersEnumerable))
            {
                mapCanvas.Children.Clear();
                return;
            }

            if (newLayersEnumerable is INotifyCollectionChanged newObservableCollection)
                CollectionChangedEventManager.AddHandler(newObservableCollection,
                    mapCanvas.OnLayersCollectionChanged);

            foreach (var layer in newLayersEnumerable)
                mapCanvas.SubscribeToLayersChanges(layer);

            mapCanvas.DrawLayers();
        }

        private void SubscribeToLayersChanges(IDrawableLayer layer)
        {
            PropertyChangedEventManager.AddHandler(
                layer,
                OnLayerStyleChanged,
                nameof(IDrawableLayer.Style));
            SubscribeToStyleChanges(layer.Style);
            if (layer.Entities is INotifyCollectionChanged observableCollection)
                CollectionChangedEventManager.AddHandler(
                    observableCollection,
                    OnEntitiesCollectionChanged);
            foreach (var entity in layer.Entities.OfType<INotifyPropertyChanged>())
                SubscribeToSpatialItemChanges(entity);
        }

        private void OnLayerStyleChanged(object sender, PropertyChangedEventArgs e)
        {
            var layer = (IDrawableLayer) sender;

            SubscribeToStyleChanges(layer.Style);

            RedrawLayer(layer);
        }

        private void SubscribeToStyleChanges(Style style)
        {
            PropertyChangedEventManager.AddHandler(
                style,
                OnStylePropertyChanged,
                string.Empty);
        }

        private void OnStylePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var layer = _layerDrawings.Keys.FirstOrDefault(x => x.Style == sender);
            if (layer != default)
                RedrawLayer(layer);
        }

        private void UnsubscribeFromLayersChanges(IDrawableLayer layer)
        {
            PropertyChangedEventManager.RemoveHandler(
                layer,
                OnLayerStyleChanged,
                nameof(IDrawableLayer.Style));
        }

        protected virtual void OnLayersChanged(IList oldList, IList newList)
        {
        }

        protected virtual void OnLayersCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs e)
        {
            foreach (IDrawableLayer oldLayer in e.OldItems)
                UnsubscribeFromLayersChanges(oldLayer);
            foreach (IDrawableLayer newLayer in e.NewItems)
                SubscribeToLayersChanges(newLayer);
            CalculateBoundaries();
            if (IsAutoScaling)
                AutoScale();
        }

        protected List<INotifyPropertyChanged> TrackedSpatialItems =
            new List<INotifyPropertyChanged>();

        protected virtual void OnEntitiesCollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            var layer = Layers.First(x => ReferenceEquals(sender, x.Entities));

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var item in TrackedSpatialItems.ToArray())
                    UnsubscribeFromSpatialItemChanged(item);
                foreach (var item in layer.Entities.OfType<INotifyPropertyChanged>())
                    SubscribeToSpatialItemChanges(item);
            }
            else
            {
                if (e.OldItems != null)
                    foreach (var oldItem in e.OldItems.OfType<INotifyPropertyChanged>())
                        UnsubscribeFromSpatialItemChanged(oldItem);

                if (e.NewItems != null)
                    foreach (var newItem in e.NewItems.OfType<INotifyPropertyChanged>())
                        SubscribeToSpatialItemChanges(newItem);
            }

            CalculateBoundaries();
            if (IsAutoScaling)
                AutoScale();    
            RedrawLayer(layer);
        }

        protected void SubscribeToSpatialItemChanges(INotifyPropertyChanged spatialItem)
        {
            PropertyChangedEventManager.AddHandler(
                spatialItem,
                OnSpatialItemGeometryChanged,
                nameof(ISpatial.Geometry));
            if (spatialItem is ILabeledSpatial)
                PropertyChangedEventManager.AddHandler(
                    spatialItem,
                    OnSpatialItemDescriptionChanged,
                    nameof(ILabeledSpatial.Description));
            TrackedSpatialItems.Add(spatialItem);
        }

        protected void UnsubscribeFromSpatialItemChanged(INotifyPropertyChanged spatialItem)
        {
            PropertyChangedEventManager.RemoveHandler(
                spatialItem,
                OnSpatialItemGeometryChanged,
                nameof(ISpatial.Geometry));
            if (spatialItem is ILabeledSpatial)
                PropertyChangedEventManager.RemoveHandler(
                    spatialItem,
                    OnSpatialItemDescriptionChanged,
                    nameof(ILabeledSpatial.Description));
            TrackedSpatialItems.Remove(spatialItem);
        }

        private void OnSpatialItemGeometryChanged(object sender, PropertyChangedEventArgs e)
        {
            var layer = Layers.First(x => x.Entities.Contains(sender));
            CalculateBoundaries();
            if (IsAutoScaling)
                AutoScale();
            RedrawLayer(layer);
        }

        private void OnSpatialItemDescriptionChanged(object sender, PropertyChangedEventArgs e)
        {
            var layer = Layers.First(x => x.Entities.Contains(sender));
            RedrawLayer(layer);
        }

        protected static void ScaleChangedCallback(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            var canvas = (MapCanvas) sender;
            canvas.OnScaleChanged();
        }

        protected virtual void OnScaleChanged()
        {
            if (IsAutoScaling)
                throw new ArgumentException(
                    "Нельзя вручную задать масштаб при IsAutoScaling == true.");
            if (Layers == null || Children.Count == 0) return;
            if (IsUniformScale)
                DrawLayers();
            else
                VerticalScale = HorizontalScale = Scale;
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            var position = e.GetPosition(this);
            X = TransformXToSource(position.X);
            Y = TransformYToSource(position.Y);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            if (IsAutoScaling && Layers != null)
            {
                AutoScale();
                DrawLayers();
            }
        }

        /// <summary>
        /// Вычисляет максимальные и минимальные значения координат, отображаемых на карте, а также
        /// масштаб.
        /// </summary>
        protected void CalculateBoundaries()
        {
            if (Layers == null) return;

            var collections = Layers;
            var geometries = collections
                .Where(x => x.Entities != null)
                .SelectMany(x => x.Entities)
                .Where(x => x != null)
                .Select(x => x.Geometry);

            using var enumerator = geometries.GetEnumerator();
            if (!enumerator.MoveNext()) return;
            while (enumerator.Current == null && enumerator.MoveNext())
            {
            }

            if (enumerator.Current != null)
            {
                var firstGeometryEnvelope = enumerator.Current.EnvelopeInternal;
                MinX = firstGeometryEnvelope.MinX;
                MinY = firstGeometryEnvelope.MinY;
                MaxX = firstGeometryEnvelope.MaxX;
                MaxY = firstGeometryEnvelope.MaxY;
            }

            while (enumerator.MoveNext())
            {
                if (enumerator.Current == null) continue;
                var geometryEnvelope = enumerator.Current.EnvelopeInternal;
                if (geometryEnvelope.MinX < MinX) MinX = geometryEnvelope.MinX;
                if (geometryEnvelope.MinY < MinY) MinY = geometryEnvelope.MinY;
                if (geometryEnvelope.MaxX > MaxX) MaxX = geometryEnvelope.MaxX;
                if (geometryEnvelope.MaxY > MaxY) MaxY = geometryEnvelope.MaxY;
            }
        }

        /// <summary>
        /// Преобразует координату по оси абсцисс в координату WPF.
        /// </summary>
        /// <param name="sourceX">Координата по оси абсцисс (ед. изм. - метры).</param>
        /// <returns>Координата WPF (ед. изм. - пиксели WPF).</returns>
        protected double TransformXToMap(double sourceX) =>
            IsUniformScale
                ? (sourceX - MinX) * Scale * 100 * PxInCm + OffsetX + AutoScalingOffset
                : (sourceX - MinX) * HorizontalScale * 100 * PxInCm + OffsetX + AutoScalingOffset;

        /// <summary>
        /// Преобразует координату по оси ординат в координату WPF.
        /// </summary>
        /// <param name="sourceY">Координата по оси ординат (ед. изм. - метры).</param>
        /// <returns>Координата WPF (ед. изм. - пиксели WPF).</returns>
        protected double TransformYToMap(double sourceY) =>
            IsUniformScale
                ? (MaxY - sourceY) * Scale * 100 * PxInCm + OffsetY + AutoScalingOffset
                : (MaxY - sourceY) * VerticalScale * 100 * PxInCm + OffsetY + AutoScalingOffset;

        /// <summary>
        /// Преобразует координату WPF в исходную координату.
        /// </summary>
        /// <param name="mapX">Координата WPF (ед. изм. - пиксели WPF).</param>
        /// <returns>Исходная координата (ед. изм. - метры).</returns>
        protected double TransformXToSource(double mapX) =>
            IsUniformScale
                ? (mapX - AutoScalingOffset) / Scale / 100 / PxInCm + MinX
                : (mapX - AutoScalingOffset) / HorizontalScale / 100 / PxInCm + MinX;

        /// <summary>
        /// Преобразует координату WPF в исходную координату.
        /// </summary>
        /// <param name="mapY">Координата WPF (ед. изм. - пиксели WPF).</param>
        /// <returns>Исходная координата (ед. изм. - метры).</returns>
        protected double TransformYToSource(double mapY) =>
            IsUniformScale
                ? MaxY - (mapY - AutoScalingOffset) / Scale / 100 / PxInCm
                : MaxY - (mapY - AutoScalingOffset) / VerticalScale / 100 / PxInCm;

        protected readonly Dictionary<IDrawableLayer, LayerDrawing> _layerDrawings
            = new Dictionary<IDrawableLayer, LayerDrawing>();

        /// <summary>
        /// Пересчитывает границы и отрисовывает слои и подписи на карте.
        /// </summary>
        protected void DrawLayers()
        {
            Children.Clear();
            _layerDrawings.Clear();

            CalculateBoundaries();

            if (IsAutoScaling)
                AutoScale();

            foreach (var layer in Layers)
                DrawLayer(layer);

            OnLayersDrawn();
        }

        protected void DrawLayer(IDrawableLayer layer)
        {
            if (!layer.Style.IsVisible) return;
            var layerDrawing = new LayerDrawing(layer, TransformXToMap, TransformYToMap);
            SetZIndex(layerDrawing, layer.Style.ZIndex);
            _layerDrawings.Add(layer, layerDrawing);
            Children.Add(layerDrawing);
            OnLayerDrawn(layer, layerDrawing);
        }

        protected virtual void OnLayerDrawn(IDrawableLayer layer, LayerDrawing layerDrawing)
        {
        }

        protected void RedrawLayer(IDrawableLayer layer)
        {
            if (!_layerDrawings.ContainsKey(layer)) throw new KeyNotFoundException();
            Children.Remove(_layerDrawings[layer]);
            _layerDrawings.Remove(layer);
            DrawLayer(layer);
        }

        protected sealed class LayerDrawing : FrameworkElement
        {
            private readonly Dictionary<ISpatial, GeometryDrawing> _drawings;
            public IReadOnlyDictionary<ISpatial, GeometryDrawing> Drawings => _drawings;

            public readonly DrawingGroup DrawingGroup;
            private readonly DrawingGroup _labelsDrawingGroup;
            private Func<double, double> _xConverter;
            private Func<double, double> _yConverter;
            public readonly IDrawableLayer Layer;

            /// <inheritdoc />
            public LayerDrawing(
                IDrawableLayer layer,
                Func<double, double> xConverter,
                Func<double, double> yConverter)
            {
                _xConverter = xConverter;
                _yConverter = yConverter;
                Layer = layer;
                DrawingGroup = new DrawingGroup
                {
                    Opacity = layer.Style.Opacity
                };
                _labelsDrawingGroup = new DrawingGroup
                {
                    Opacity = layer.Style.Opacity
                };
                _drawings = new Dictionary<ISpatial, GeometryDrawing>();
                var style = layer.Style;
                var mainBrush = BrushFromColor(style.MainFillColor);
                mainBrush.Freeze();
                var strokeBrush = BrushFromColor(style.StrokeColor);
                strokeBrush.Freeze();
                var pen = new Pen(strokeBrush, style.StrokeThickness);
                pen.Freeze();
                foreach (var entity in layer.Entities.Where(x => x.Geometry != null))
                {
                    var geometryForEntity = entity.Geometry switch
                    {
                        NtpPoint point => CreatePointGeometry(
                            mainBrush,
                            pen,
                            layer.Style.StrokeThickness,
                            point),
                        LineString line => CreatePolylineGeometry(
                            pen,
                            line),
                        Polygon polygon => CreatePolygonGeometry(
                            mainBrush,
                            pen,
                            polygon),
                        MultiPolygon multiPolygon => CreateMultiPolygonGeometry(
                            mainBrush,
                            pen,
                            multiPolygon),
                        _ => throw new NotImplementedException()
                    };
                    _drawings.Add(entity, geometryForEntity);
                    DrawingGroup.Children.Add(geometryForEntity);
                }

                new Typeface(style.Font).TryGetGlyphTypeface(out var glyphTypeFace);
                var advanceWidth = glyphTypeFace.AdvanceWidths[0] * 12.5;

                var fontBackgroundBrush = BrushFromColor(layer.Style.FontBackgroundColor);
                fontBackgroundBrush.Freeze();

                if (!layer.Style.IsLabeled) return;
                foreach (var entity in layer.Entities.OfType<ILabeledSpatial>()
                    .Where(x => x.Geometry != null && !string.IsNullOrEmpty(x.Description)))
                {
                    var glyphIndices = new List<ushort>();
                    var advanceWidths = new List<double>();
                    var glyphOffsets = new List<WpfPoint>();
                    var centroid = entity.Geometry.Centroid;
                    var x = _xConverter.Invoke(centroid.X);
                    var y = _yConverter.Invoke(centroid.Y);

                    for (var i = 0; i < entity.Description.Length; i++)
                    {
                        var glyphIndex =
                            glyphTypeFace.CharacterToGlyphMap[entity.Description[i]];
                        glyphIndices.Add(glyphIndex);
                        advanceWidths.Add(advanceWidth);
                        glyphOffsets.Add(new WpfPoint(0, 0));
                    }

                    _labelsDrawingGroup.Children.Add(new GeometryDrawing(
                        fontBackgroundBrush,
                        null,
                        new RectangleGeometry(new Rect(
                            new WpfPoint(x, y),
                            new WpfPoint(x + advanceWidths.Sum(), y - 12.5)))
                    ));

                    _labelsDrawingGroup.Children.Add(new GlyphRunDrawing(
                        Brushes.Black,
                        new GlyphRun(
                            glyphTypeFace,
                            0,
                            false,
                            style.FontSize,
                            12.5f,
                            glyphIndices,
                            new WpfPoint(x, y),
                            advanceWidths,
                            glyphOffsets,
                            entity.Description.ToList(),
                            null,
                            null,
                            null,
                            null)));
                }
            }

            public static SolidColorBrush BrushFromColor(Color color) =>
                new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                    color.A,
                    color.R,
                    color.G,
                    color.B));

            /// <inheritdoc />
            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);
                drawingContext.DrawDrawing(DrawingGroup);
                drawingContext.DrawDrawing(_labelsDrawingGroup);
            }

            private WpfPoint ConvertCoordinateToWpfPoint(Coordinate coordinate) =>
                new WpfPoint(_xConverter.Invoke(coordinate.X), _yConverter.Invoke(coordinate.Y));

            private GeometryDrawing CreatePointGeometry(
                Brush brush,
                Pen pen,
                double radius,
                NtpPoint point) =>
                new GeometryDrawing(
                    brush,
                    pen,
                    new EllipseGeometry(
                        ConvertCoordinateToWpfPoint(point.Coordinate),
                        radius,
                        radius));

            private GeometryDrawing CreatePolylineGeometry(
                Pen strokePen,
                LineString line) =>
                new GeometryDrawing(
                    null,
                    strokePen,
                    new PathGeometry(
                        new[]
                        {
                            new PathFigure(ConvertCoordinateToWpfPoint(line.Coordinates[0]),
                                line.Coordinates.Select(x =>
                                    new LineSegment(ConvertCoordinateToWpfPoint(x), true)),
                                false) {IsFilled = false},
                        }
                    ));

            private PathGeometry PolygonToPathGeometry(Polygon polygon)
            {
                var shell = polygon.Shell;
                var holes = polygon.Holes;
                return new PathGeometry(Enumerable.Repeat(
                        new PathFigure(
                            ConvertCoordinateToWpfPoint(shell[0]),
                            shell.Coordinates.Skip(1)
                                .Select(x => new LineSegment(
                                    ConvertCoordinateToWpfPoint(x), true)),
                            true),
                        1)
                    .Union(holes.Select(hole => new PathFigure(
                        ConvertCoordinateToWpfPoint(hole[0]),
                        hole.Coordinates.Skip(1)
                            .Select(x => new LineSegment(
                                ConvertCoordinateToWpfPoint(x), true)),
                        true)))
                );
            }

            private GeometryDrawing CreatePolygonGeometry(
                Brush brush,
                Pen pen,
                Polygon polygon) =>
                new GeometryDrawing(brush,
                    pen,
                    PolygonToPathGeometry(polygon));

            private GeometryDrawing CreateMultiPolygonGeometry(
                Brush brush,
                Pen pen,
                MultiPolygon multiPolygon) =>
                new GeometryDrawing(
                    brush,
                    pen,
                    new GeometryGroup
                    {
                        Children = new GeometryCollection(
                            multiPolygon.Geometries.Cast<Polygon>()
                                .Select(PolygonToPathGeometry))
                    });
        }

        protected virtual void OnLayersDrawn()
        {
        }
    }
}