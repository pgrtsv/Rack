using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Rack.Shared.ValidationRules
{
    public class DigitNumbersValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var regexRule = new Regex("[^\\d,.; ]"); // Разрешены только цифры и разделители.
            if (regexRule.IsMatch((string) value))
                return new ValidationResult(false, "Разрешены только цифры и разделители.");
            return new ValidationResult(true, null);
        }
    }
}