using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rack.Shared.Attributes.Validation
{
    /// <summary>
    /// Атрибут для задания в строке фамилии, имени и отчества в полном виде.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IsFullNameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(value as string)) return ValidationResult.Success;
            var stringValue = value.ToString();
            var rule = new Regex("[^\\w -0-9]"); // Разрешены алфавитные знаки, пробелы и дефисы.

            if (rule.IsMatch(stringValue))
                return new ValidationResult("В поле ФИО разрешены только алфавитные знаки, пробелы и дефисы.");
            var words = stringValue.Split(' ');
            if (words.Length != 3 || !words.Select(x => x.Length > 0).Aggregate((x, y) => x && y))
                return new ValidationResult("Введите фамилию, имя и отчество.", new[] {validationContext.MemberName});
            if (char.IsLower(words[0][0])) return new ValidationResult("Фамилия должна начинаться с заглавной буквы.");
            if (char.IsLower(words[1][0])) return new ValidationResult("Имя должно начинаться с заглавной буквы.");
            if (char.IsLower(words[2][0])) return new ValidationResult("Отчество должно начинаться с заглавной буквы.");
            return ValidationResult.Success;
        }
    }
}