using System;
using System.ComponentModel;

namespace Rack.GeoTools
{
    [Serializable]
    public class Style : INotifyPropertyChanged
    {
        /// <summary>
        /// true, чтобы отображать подписи.
        /// </summary>
        public bool IsLabeled { get; set; }

        /// <summary>
        /// Название шрифта для подписей.
        /// </summary>
        public string Font { get; set; } = "Courier New";

        /// <summary>
        /// Размер шрифта для подписей.
        /// </summary>
        public int FontSize { get; set; } = 12;

        /// <summary>
        /// </summary>
        public Color MainFillColor { get; set; } = Color.Random();

        public Color ActiveFillColor { get; set; } = new Color {A = 100, R = 255, G = 255, B = 255};

        public Color StrokeColor { get; set; } = new Color {A = 255, R = 0, G = 0, B = 0};

        public Color FontBackgroundColor { get; set; }

        public double Opacity { get; set; } = 1.0;

        public double StrokeThickness { get; set; } = 3;

        public bool IsVisible { get; set; } = true;

        public int ZIndex { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}