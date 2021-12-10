using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;

namespace Rack.Shared.FluentValidation
{
    public sealed class ReactiveValidationTemplate<T> : IDataErrorInfo, INotifyDataErrorInfo
    {
        private ValidationResult _validationResult = new ValidationResult();

        public ReactiveValidationTemplate(
            AbstractValidator<T> validator,
            IObservable<T> validationTrigger)
        {
            validationTrigger.Subscribe(objectToValidate =>
            {
                var oldValidationResult = _validationResult;
                _validationResult = validator.Validate(new ValidationContext<T>(objectToValidate));
                foreach (var error in _validationResult.Errors)
                    ErrorsChanged?.Invoke(
                        objectToValidate,
                        new DataErrorsChangedEventArgs(error.PropertyName));
                foreach (var oldError in oldValidationResult.Errors
                    .Except(_validationResult.Errors, new ValidationFailureComparer()))
                    ErrorsChanged?.Invoke(
                        objectToValidate,
                        new DataErrorsChangedEventArgs(oldError.PropertyName));
            });
        }

        public string Error
        {
            get
            {
                var strings = _validationResult.Errors.Select(x => x.ErrorMessage)
                    .ToArray();
                return string.Join(Environment.NewLine, strings);
            }
        }

        public string this[string columnName]
        {
            get
            {
                var strings = _validationResult.Errors.Where(x => x.PropertyName == columnName)
                    .Select(x => x.ErrorMessage)
                    .ToArray();
                return string.Join(Environment.NewLine, strings);
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return _validationResult.Errors
                    .Select(x => x.ErrorMessage);
            return _validationResult.Errors
                .Where(x => x.PropertyName == propertyName)
                .Select(x => x.ErrorMessage);
        }

        public bool HasErrors => _validationResult.Errors.Count > 0;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private sealed class ValidationFailureComparer : IEqualityComparer<ValidationFailure>
        {
            public bool Equals(ValidationFailure x, ValidationFailure y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.PropertyName, y.PropertyName) && string.Equals(x.ErrorMessage, y.ErrorMessage);
            }

            public int GetHashCode(ValidationFailure obj)
            {
                unchecked
                {
                    return ((obj.PropertyName != null ? obj.PropertyName.GetHashCode() : 0) * 397) ^
                           (obj.ErrorMessage != null ? obj.ErrorMessage.GetHashCode() : 0);
                }
            }
        }
    }
}