using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rack.LocalizationTool.Models.LocalizationProblem
{
    /// <summary>
    /// Нелокализованная строка в .cs-файле.
    /// </summary>
    public sealed class UnlocalizedCsharpString : IUnlocalizedString
    {
        public UnlocalizedCsharpString(string text, int row, int index)
        {
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentNullException(nameof(text));
            if (row < 0) throw new ArgumentOutOfRangeException(nameof(row));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));

            String = text;
            Row = row;
            Index = index;

            if (text[^1] != '"') throw new ArgumentException();
            if (text[0] == '"')
            {
                Value = text[1..^1];
            }
            else if (text[0] == '@')
            {
                Value = text[2..^1];
                IsScreened = true;
            }
            else if (text[0] == '$')
            {
                if (text[1] == '"')
                {
                    Value = text[2..^1];
                    IsInterpolated = true;
                }
                else if (text[1] == '@')
                {
                    if (text[2] != '"') throw new ArgumentException();
                    Value = text[3..^1];
                    IsScreened = true;
                    IsInterpolated = true;
                }
            }
            else throw new ArgumentException();

            if (IsInterpolated)
                InterpolationExpressions = GetInterpolationExpressions().ToArray();
        }

        /// <inheritdoc />
        public string String { get; }

        /// <inheritdoc />
        public string Value { get; }

        /// <inheritdoc />
        public int Row { get; }

        /// <inheritdoc />
        public int Index { get; }

        /// <summary>
        /// true, если <see cref="String"/> интерполирована.
        /// </summary>
        public bool IsInterpolated { get; }

        /// <summary>
        /// true, если <see cref="String"/> экранирована.
        /// </summary>
        public bool IsScreened { get; }

        /// <summary>
        /// Коллекция интерполяционных выражений, находящихся в строке.
        /// <see langword="null"/>, если строка неинтерполирована.
        /// </summary>
        public IReadOnlyCollection<string> InterpolationExpressions { get; }

        /// <summary>
        /// Возвращает нелокализированное значение <see cref="Value"/>
        /// в формате фразы из файла локализации.
        /// </summary>
        /// <returns>Если строка не интерполирована вернёт – значение <see cref="Value"/>,
        /// иначе заменит все интерполяционные выражения в строке
        /// <see cref="InterpolationExpressions"/> на плейсхолдеры.</returns>
        public string ToLocalizationPhraseFormat()
        {
            if (!IsInterpolated) return Value;

            var value = Value;
            for (int i = 0; i < InterpolationExpressions.Count; i++)
            {
                var expression = InterpolationExpressions.ElementAt(i);
                var regex = new Regex(expression);
                value = regex.Replace(value, i.ToString(), 1);
            }

            return value;
        }

        /// <summary>
        /// Создает значение в локализированном формате.
        /// </summary>
        /// <param name="key">Ключ из локализационного файла.</param>
        /// <returns>Значение в локализированном формате,
        /// которое учитывает наличие интерполяции.</returns>
        public string ToLocalizedFormat(string key)
        {
            var localizedBase = $"Localization[\"{key}\"]";
            if (!IsInterpolated)
                return localizedBase;
            var formatArguments = string.Join(", ", InterpolationExpressions);
            return $"{localizedBase}.FormatDefault({formatArguments})";
        }

        /// <summary>
        /// Ищет в интерполированной строке интерполяционные выражения.
        /// </summary>
        /// <returns>Перечисление интерполяционных выражений.</returns>
        private IEnumerable<string> GetInterpolationExpressions()
        {
            var isQuoteScope = false;

            for (int i = 0; i < String.Length; i++)
            {
                var symbol = String[i];
                if (symbol != '{') continue;
                if (String[i + 1] == '{') continue;
                
                i++;
                symbol = String[i];
                var currentExpression = "";
                while (true)
                {
                    if (symbol == '}' && !isQuoteScope)
                    {
                        yield return currentExpression;
                        break;
                    }

                    if (symbol == '"' && String[i - 1] != '\\')
                        isQuoteScope = !isQuoteScope;
                    
                    currentExpression += symbol;
                    i++;
                    symbol = String[i];
                }
            }
        }
    }
}