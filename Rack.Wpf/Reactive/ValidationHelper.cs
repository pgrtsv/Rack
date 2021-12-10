using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Rack.Wpf.Reactive
{
    /// <summary>
    /// Вспомогательный класс, позволяющий задавать ошибки валидации для <see cref="DependencyObject"/>.
    /// </summary>
    public static class ValidationHelper
    {
        private static readonly MethodInfo AddValidationErrorMethod =
            typeof(Validation).GetMethod("AddValidationError", BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly MethodInfo RemoveValidationErrorMethod =
            typeof(Validation).GetMethod("RemoveValidationError", BindingFlags.NonPublic | BindingFlags.Static);

        public static void AddValidationError(
            ValidationError validationError,
            DependencyObject targetElement)
        {
            AddValidationErrorMethod
                .Invoke(null, new object[] { validationError, targetElement, true });
        }

        public static void ClearValidationErrors(DependencyObject targetElement)
        {
            foreach (var error in Validation.GetErrors(targetElement).ToArray())
                RemoveValidationErrorMethod
                    .Invoke(null, new object[] { error, targetElement, true });
        }
    }
}
