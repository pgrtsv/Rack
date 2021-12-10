using System;
using System.ComponentModel.DataAnnotations;

namespace Rack.Shared.Attributes.Validation
{
    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RequiredWithPropertyErrorRepresentationAttribute : RequiredAttribute
    {
        /// <summary>
        /// Представление имени свойства в сообщение об ошибке валидации.
        /// </summary>
        private readonly string _propertyErrorMesageRepresentation;

        /// <inheritdoc />
        /// <param name="propertyErrorMesageRepresentation">Строковое представление имени свойства, вызвавшего сбой проверки.</param>
        public RequiredWithPropertyErrorRepresentationAttribute(string propertyErrorMesageRepresentation = null)
        {
            _propertyErrorMesageRepresentation = propertyErrorMesageRepresentation;
        }

        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            /*Если было передано в параметры строковое представление свойства для сообщения об ошибке, 
             * то изменяем стандартное сообщение об ошибке.*/
            if (!string.IsNullOrEmpty(_propertyErrorMesageRepresentation))
                ErrorMessage = GetErrorMessage(_propertyErrorMesageRepresentation);
            return base.IsValid(value, validationContext);
        }

        /// <inheritdoc />
        /// <summary>
        /// Переопределенный метод: подставляет, если задано, значение <see cref="_propertyErrorMesageRepresentation" />
        /// вместо строкового представления свойства в тексте об ошибке валидации.
        /// </summary>
        /// <param name="name">
        /// Имя свойства, вызвавшее сбой проверки.
        /// Если данный параметр будет передан, то сообщение об ошибке будет изменено,
        /// даже если сообщение также было передано в параметры атрибута.
        /// </param>
        /// <returns>Отформатированное сообщение об ошибке.</returns>
        public override string FormatErrorMessage(string name)
        {
            return base.FormatErrorMessage(!string.IsNullOrEmpty(_propertyErrorMesageRepresentation)
                ? _propertyErrorMesageRepresentation
                : name);
        }

        /// <summary>
        /// Возвращает сообщение в соответствии с переданным строковым представлением валидируемого свойства.
        /// Рекомендуется использовать сообщение об ошибке полученное данным методом.
        /// </summary>
        /// <param name="propertyErrorMesageRepresentation"></param>
        /// <returns></returns>
        private string GetErrorMessage(string propertyErrorMesageRepresentation)
        {
            return $"Поле {propertyErrorMesageRepresentation} не должно быть пустым.";
        }
    }
}