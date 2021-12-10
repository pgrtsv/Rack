using System;
using System.ComponentModel.DataAnnotations;

namespace Rack.Shared.Attributes.Validation
{
    /// <summary>
    /// Атрибут для сравнения одной даты с другой датой
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [Obsolete("Рекомендуется использовать CompareAttribute")]
    public sealed class CompareDateAttribute : ValidationAttribute
    {
        /// <summary>
        /// Имя свойства даты для сравнения.
        /// </summary>
        private readonly string _comparsionProperty;

        /// <summary>
        /// Тип сравнения дат.
        /// </summary>
        private readonly ComparsionType _comparsionType;

        /// <summary>
        /// Строка, представляющие свойство валидируемой даты, в тексте об ошибке.
        /// </summary>
        private readonly string _leftDateErrorMesageRepresentation;

        /// <summary>
        /// Строка, представляющие свойство даты для сравнения, в тексте об ошибке.
        /// </summary>
        private readonly string _rightDateErrorMesageRepresentation;

        /// <summary>
        /// Задает ограничение по дате, относительно значение другой сравниваемой даты.
        /// </summary>
        /// <param name="comparsionProperty">Имя свойства, представляющее дату с которой производится сравнение.</param>
        /// <param name="comparsionType">Тип сравнения.</param>
        /// <param name="leftDateErrorMesageRepresentation">
        /// Задает строку, для представления в тексте об ошибке проверки, свойства,
        /// представляющее дату, подвергающуюся проверки. Если задан данный атрибут, то должен быть задан
        /// аналогичный параметр и для даты сравнения, иначе сообщение не будет изменено.
        /// </param>
        /// <param name="rightDateErrorMesageRepresentation">
        /// Задает строку, для представления в тексте об ошибке проверки, свойства,
        /// представляющее дату для сравнения. Если оба параметра, для представления свойств дат в сообщение об ошибке заданы,
        /// то сообщение об ошибке будет изменено в соответствии с этими параметрами.
        /// </param>
        public CompareDateAttribute(string comparsionProperty, ComparsionType comparsionType,
            string leftDateErrorMesageRepresentation = null, string rightDateErrorMesageRepresentation = null)
        {
            _comparsionProperty = comparsionProperty;
            _comparsionType = comparsionType;
            _leftDateErrorMesageRepresentation = leftDateErrorMesageRepresentation;
            _rightDateErrorMesageRepresentation = rightDateErrorMesageRepresentation;
        }

        /// <summary>
        /// Определяет, является ли допустимым значения проверяемой даты, относительно другой даты.
        /// </summary>
        /// <param name="value">Значение объекта, для проверки.</param>
        /// <param name="validationContext">Контекст валидации.</param>
        /// <returns>true если указанное значение является допустимым; в противном случае — false.</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var leftDate = value as DateTime?; //Значение проверяемой даты.

            var property = validationContext.ObjectType.GetProperty(_comparsionProperty); //Свойство даты для сравнения.
            if (property == null)
                throw new ArgumentException("Свойство \"EndDate\" не найдено в контексте валидации.");
            var rightDate =
                property.GetValue(validationContext.ObjectInstance) as DateTime?; //Значение даты для сравнения.
            /*Случай, когда одна из дат null – не считаем за ошибку.*/
            if (leftDate == null || rightDate == null)
                return ValidationResult.Success;

            bool isValid;

            switch (_comparsionType)
            {
                case ComparsionType.IsLess:
                    isValid = leftDate < rightDate;
                    break;
                case ComparsionType.IsLessOrEqual:
                    isValid = leftDate <= rightDate;
                    break;
                case ComparsionType.IsMore:
                    isValid = leftDate > rightDate;
                    break;
                case ComparsionType.IsMoreOrEqual:
                    isValid = leftDate >= rightDate;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(_comparsionType.ToString(),
                        "Такой тип сравнения не поддерживается.");
            }

            return isValid
                ? ValidationResult.Success
                : new ValidationResult(
                    !string.IsNullOrEmpty(_leftDateErrorMesageRepresentation) &&
                    !string.IsNullOrEmpty(_rightDateErrorMesageRepresentation)
                        ? GetErrorMessage()
                        : ErrorMessageString,
                    new[]
                    {
                        validationContext.MemberName,
                        _comparsionProperty
                    });
        }

        /// <summary>
        /// Составляет сообщение об ошибке в соответствии с переданными в конструкторе строковыми параметрами,
        /// представляющие свойства дат в сообщении об ошибке.
        /// </summary>
        private string GetErrorMessage()
        {
            if (string.IsNullOrEmpty(_leftDateErrorMesageRepresentation) &&
                string.IsNullOrEmpty(_rightDateErrorMesageRepresentation))
                throw new ArgumentNullException(
                    "Метод не может сконструировать сообщение об ошибке, когда строки, представляющие в тексте сообщения об ошибке свойства сравниваемых дат – null или являются пустыми строками.");
            switch (_comparsionType)
            {
                case ComparsionType.IsLess:
                    return
                        $"Дата {_leftDateErrorMesageRepresentation} должна быть меньше, чем дата {_rightDateErrorMesageRepresentation}.";
                case ComparsionType.IsLessOrEqual:
                    return
                        $"Дата {_leftDateErrorMesageRepresentation} должна быть меньше или равна, чем дата {_rightDateErrorMesageRepresentation}.";
                case ComparsionType.IsMore:
                    return
                        $"Дата {_leftDateErrorMesageRepresentation} должна быть больше, чем дата {_rightDateErrorMesageRepresentation}.";
                case ComparsionType.IsMoreOrEqual:
                    return
                        $"Дата {_leftDateErrorMesageRepresentation} должна быть больше или равна, чем дата {_rightDateErrorMesageRepresentation}.";
                default:
                    throw new ArgumentOutOfRangeException(_comparsionType.ToString(),
                        "Такой тип сравнения не поддерживается.");
            }
        }
    }
}