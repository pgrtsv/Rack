using System;
using System.ComponentModel.DataAnnotations;

namespace Rack.Shared.Attributes.Validation
{
    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RangeWithPropertyErrorRepresentationAttribute : RangeAttribute
    {
        /// <summary>
        /// Представление имени свойства в сообщение об ошибке валидации.
        /// </summary>
        private readonly string _proertyErrorMesageRepresentation;

        /// <inheritdoc />
        public RangeWithPropertyErrorRepresentationAttribute(int minimum, int maximum) : base(minimum, maximum)
        {
        }

        /// <inheritdoc />
        public RangeWithPropertyErrorRepresentationAttribute(double minimum, double maximum) : base(minimum, maximum)
        {
        }

        /// <inheritdoc />
        public RangeWithPropertyErrorRepresentationAttribute(Type type, string minimum, string maximum) : base(type,
            minimum, maximum)
        {
        }

        /// <inheritdoc />
        /// <param name="proertyErrorMesageRepresentation">Строковое представление имени свойства, вызвавшего сбой проверки.</param>
        public RangeWithPropertyErrorRepresentationAttribute(int minimum, int maximum,
            string proertyErrorMesageRepresentation) : base(minimum, maximum)
        {
            _proertyErrorMesageRepresentation = proertyErrorMesageRepresentation;
        }

        /// <inheritdoc />
        /// <param name="proertyErrorMesageRepresentation">Строковое представление имени свойства, вызвавшего сбой проверки.</param>
        public RangeWithPropertyErrorRepresentationAttribute(double minimum, double maximum,
            string proertyErrorMesageRepresentation) : base(minimum, maximum)
        {
            _proertyErrorMesageRepresentation = proertyErrorMesageRepresentation;
        }

        /// <inheritdoc />
        /// <summary>
        /// Переопределенный метод: подставляет, если задано, значение <see cref="_proertyErrorMesageRepresentation" />
        /// вместо строкового представления свойства в тексте об ошибке валидации.
        /// </summary>
        /// <param name="name">Имя свойства, вызвавшее сбой проверки.</param>
        /// <returns>Отформатированное сообщение об ошибке.</returns>
        public override string FormatErrorMessage(string name)
        {
            return base.FormatErrorMessage(!string.IsNullOrEmpty(_proertyErrorMesageRepresentation)
                ? _proertyErrorMesageRepresentation
                : name);
        }
    }
}