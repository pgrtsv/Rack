using System;
using System.Collections.Generic;
using System.Linq;

namespace Rack.LocalizationTool.Models.LocalizationData
{
    /// <summary>
    /// Тип форматирования строки.
    /// </summary>
    public enum StringFormatStyle
    {
        /// <summary>
        /// Без форматирования.
        /// </summary>
        WithoutFormat,
        /// <summary>
        /// Стандартное форматирование string.Format().
        /// </summary>
        ClassicFormat,
        /// <summary>
        /// Альтернатива стандартного форматирования, которая не выбрасывает исключения,
        /// если форматируемая строка - пустая.
        /// </summary>
        DefaultFormat
    }

    /// <summary>
    /// Участок кода в файле, где используется локализация (.cs, .xaml файлы).
    /// </summary>
    public sealed class LocalizedPlace: IEquatable<LocalizedPlace>
    {
        public LocalizedPlace(
            string fullString, 
            string key, 
            int row,
            int index):this(fullString, key, row, index, 
            StringFormatStyle.WithoutFormat, new string[0])
        {
        }

        public LocalizedPlace(
            string fullString,
            string key,
            int row,
            int index,
            StringFormatStyle formatStyle,
            IEnumerable<string> formatArguments)
        {
            FullString = fullString;
            LocalizationKey = key;
            Row = row;
            Index = index;
            FormatStyle = formatStyle;
            IsFormatted = formatStyle != StringFormatStyle.WithoutFormat;
            FormatArguments = formatArguments?.ToArray();
        }


        /// <summary>
        /// Строка, в которой обнаружена локализация.
        /// </summary>
        public string FullString { get; }

        /// <summary>
        /// Ключ локализации.
        /// </summary>
        public string LocalizationKey { get; }

        /// <summary>
        /// Индекс, начиная с которого идёт ключ локализации.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Номер строки в файле.
        /// </summary>
        public int Row { get; }

        /// <summary>
        /// Стиль форматирования значения фразы локализации.
        /// </summary>
        public StringFormatStyle FormatStyle { get; }

        /// <summary>
        /// <see langword="true"/>, если строка форматирована.
        /// </summary>
        public bool IsFormatted { get; }

        /// <summary>
        /// Коллекция аргументов заменяющие плейсхолдеры в форматировании.
        /// </summary>
        public IReadOnlyCollection<string> FormatArguments { get; }

        public bool Equals(LocalizedPlace other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return FullString == other.FullString 
                   && LocalizationKey == other.LocalizationKey 
                   && Index == other.Index 
                   && Row == other.Row;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is LocalizedPlace other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (FullString != null ? FullString.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LocalizationKey != null ? LocalizationKey.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Index;
                hashCode = (hashCode * 397) ^ Row;
                return hashCode;
            }
        }
    }
}