using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;

namespace Rack.Shared.FluentValidation
{
    public class ValidationTemplate<T> : IDataErrorInfo, INotifyDataErrorInfo
        where T : INotifyPropertyChanged
    {
        private readonly Action _onErrorsChanged;
        private readonly T _target;
        private readonly IValidator _validator;
        private ValidationResult _validationResult;

        public ValidationTemplate(T target, IValidator validator)
        {
            _target = target;
            _validator = validator;
            _validationResult = _validator.Validate(new ValidationContext<T>(target));
            target.PropertyChanged += Validate;
        }

        public ValidationTemplate(
            T target,
            IValidator validator,
            Action onErrorsChanged)
            : this(target, validator) =>
            _onErrorsChanged = onErrorsChanged;

        public string Error
        {
            get
            {
                var strings = _validationResult.Errors.Select(x => x.ErrorMessage)
                    .ToArray();
                return string.Join(Environment.NewLine, strings);
            }
        }

        public string this[string propertyName]
        {
            get
            {
                var strings = _validationResult.Errors.Where(x => x.PropertyName == propertyName)
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

        private void Validate(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(HasErrors)) return;
            var hadErrors = HasErrors;
            var oldValidationResult = _validationResult;
            _validationResult = _validator.Validate(new ValidationContext<T>(_target));
            foreach (var error in _validationResult.Errors) RaiseErrorsChanged(error.PropertyName);
            foreach (var oldError in oldValidationResult.Errors
                .Except(_validationResult.Errors, ValidationFailureComparer.Instance))
                RaiseErrorsChanged(oldError.PropertyName);
            if (hadErrors != HasErrors)
                _onErrorsChanged?.Invoke();
        }

        private void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(_target, new DataErrorsChangedEventArgs(propertyName));
        }

        private class ValidationFailureComparer : IEqualityComparer<ValidationFailure>
        {
            public static ValidationFailureComparer Instance { get; } = new ValidationFailureComparer();

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