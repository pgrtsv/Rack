using System;
using System.ComponentModel.DataAnnotations;

namespace Rack.Shared.Attributes.Validation
{
    /// <summary>
    /// Атрибут для сравнения значения помеченного свойства с другим значением.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CompareAttribute : ValidationAttribute
    {
        private readonly string _comparableProperty;
        private readonly ComparsionType _comparsionType;
        private object _comparableValue;

        public CompareAttribute(ComparsionType comparsionType, object comparableValue)
        {
            _comparableValue = comparableValue;
            _comparsionType = comparsionType;
        }

        public CompareAttribute(string comparableProperty, ComparsionType comparsionType)
        {
            _comparableProperty = comparableProperty;
            _comparsionType = comparsionType;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is null) return ValidationResult.Success;

            if (_comparableProperty != null)
                _comparableValue = validationContext.ObjectType.GetProperty(_comparableProperty)
                    .GetValue(validationContext.ObjectInstance);

            if (!(value is IComparable valueToCompare))
                throw new ArgumentException("value должно реализовывать интерфейс IComparable.");
            var comparableValue = Convert.ChangeType(_comparableValue, valueToCompare.GetType());
            var comparsionResult = valueToCompare.CompareTo(comparableValue);

            var members = _comparableProperty == null
                ? new[] {validationContext.MemberName}
                : new[] {validationContext.MemberName, _comparableProperty};

            string errorMessage = ErrorMessage == null ? null : string.Format(ErrorMessage, _comparableValue);

            switch (_comparsionType)
            {
                case ComparsionType.IsLess:
                    return comparsionResult < 0
                        ? ValidationResult.Success
                        : new ValidationResult(
                            errorMessage ?? $"Значение должно быть меньше {_comparableValue}.", members);
                case ComparsionType.IsLessOrEqual:
                    return comparsionResult <= 0
                        ? ValidationResult.Success
                        : new ValidationResult(
                            errorMessage ?? $"Значение должно быть меньше или равно {_comparableValue}.", members);
                case ComparsionType.IsMore:
                    return comparsionResult > 0
                        ? ValidationResult.Success
                        : new ValidationResult(
                            errorMessage ?? $"Значение должно быть больше {_comparableValue}.", members);
                case ComparsionType.IsMoreOrEqual:
                    return comparsionResult >= 0
                        ? ValidationResult.Success
                        : new ValidationResult(
                            errorMessage ?? $"Значение должно быть больше или равно {_comparableValue}.", members);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}