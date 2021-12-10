using System;

namespace Rack.Shared.Attributes
{
    /// <summary>
    /// Свойства, помеченные этим атрибутом, учитываются при автогенерации представлений отчётов и редакторов.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DisplayedAttribute : Attribute
    {
        /// <summary>
        /// Локализованное название свойства.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Строка форматирования.
        /// </summary>
        public string StringFormat { get; set; }

        /// <summary>
        /// Расположение свойства относительно других, учитывается при сортировке.
        /// </summary>
        public int Order { get; set; } = int.MaxValue;

        public double Width { get; set; } = 200;

        /// <summary>
        /// Режим отображения свойства.
        /// </summary>
        public DisplayMode DisplayMode { get; set; } = DisplayMode.Everywhere;

        /// <summary>
        /// Если true, то при генерации текстового поля должна быть предусметрена многострочность.
        /// </summary>
        public bool IsMultiLine { get; set; } = false;

        public string TargetNullValue { get; set; }
    }
}