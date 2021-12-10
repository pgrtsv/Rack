using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Rack.Shared.Attributes.Validation
{
    /// <summary>
    /// Атрибут, задающий ограничения для даты по минимальному и максимальному значению.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [Obsolete("Рекомендуется использовать CompareAttribute")]
    public sealed class DateRangeAttribute : ValidationAttribute
    {
        /// <summary>
        /// Строка, представляющие проверяемое свойство, в тексте сообщения об ошибке.
        /// </summary>
        private readonly string _dateErrorMesageRepresentation;

        /// <summary>
        /// Максимально допустимая дата.
        /// </summary>
        private readonly string _maxDateString;

        /// <summary>
        /// Минимально допустимая дата.
        /// </summary>
        private readonly string _minDateString;

        /// <summary>
        /// Задает ограничение по минимально и максимально допустимому значению даты.
        /// </summary>
        /// <param name="minDateString">Минимально допустимое значение даты, в формате строки «dd.MM.yyyy».</param>
        /// <param name="maxDateString">Максимально допустимое значение даты, в формате строки «dd.MM.yyyy».</param>
        /// <param name="dateErrorMesageRepresentation">Строка, представляющие проверяемое свойство, в тексте сообщения об ошибке.</param>
        public DateRangeAttribute(string minDateString, string maxDateString,
            string dateErrorMesageRepresentation = null)
        {
            _minDateString = minDateString;
            _maxDateString = maxDateString;
            _dateErrorMesageRepresentation = dateErrorMesageRepresentation;
        }

        /// <summary>
        /// Определяет, является ли допустимым значения проверяемой даты, относительно заданных границ.
        /// </summary>
        /// <param name="value">Значение объекта, для проверки.</param>
        /// <param name="validationContext">Контекст валидации.</param>
        /// <returns></returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var currentDate = value as DateTime?;
            var minDateTime = DateTime.ParseExact(_minDateString, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            var maxDateTime = DateTime.ParseExact(_maxDateString, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            /*Значение null – не считаем за ошибку.*/
            if (currentDate == null)
                return ValidationResult.Success;
            return currentDate > minDateTime && currentDate < maxDateTime
                ? ValidationResult.Success
                : new ValidationResult(GetErrorMessage(validationContext.MemberName),
                    new[] {validationContext.MemberName});
        }

        /// <summary>
        /// Возвращает сообщение об ошибке валидации.
        /// </summary>
        /// <param name="propertyName">
        /// Имя свойства. Если строковое представление свойства в тексте ошибке
        /// не было передано в параметры атрибут, то в текст ошибки будет подставлен данный атрибут.
        /// </param>
        /// <returns>Сообщение об ошибке валидации.</returns>
        private string GetErrorMessage(string propertyName)
        {
            var nameInErrorMessage = string.IsNullOrEmpty(_dateErrorMesageRepresentation)
                ? propertyName
                : _dateErrorMesageRepresentation;
            return $"Дата {nameInErrorMessage} должна иметь значение между {_minDateString} и {_maxDateString}.";
        }
    }
}