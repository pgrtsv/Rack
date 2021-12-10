using System;

namespace Rack.LocalizationTool.Models.LocalizationProblem
{
    /// <summary>
    /// Нелокализованная строка в .xaml-файле.
    /// </summary>
    public sealed class UnlocalizedXamlString : IUnlocalizedString
    {
        public UnlocalizedXamlString(string text, int row, int index)
        {
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentNullException(nameof(text));
            if (row < 0) throw new ArgumentOutOfRangeException(nameof(row));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));

            String = text;
            Row = row;
            Index = index;

            if (text[^1] != '"') throw new ArgumentException();
            if (text[0] != '"') throw new ArgumentException();
            Value = text[1..^1];
        }

        /// <inheritdoc />
        public string String { get; }

        /// <inheritdoc />
        public string Value { get; }

        /// <inheritdoc />
        public int Row { get; }

        /// <inheritdoc />
        public int Index { get; }
    }
}