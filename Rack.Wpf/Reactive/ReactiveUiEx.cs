using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using Rack.Reflection;
using ReactiveUI;

namespace Rack.Wpf.Reactive
{
    public static class ReactiveUiEx
    {
        /// <summary>
        /// Связывает строку с подсказкой MaterialDesignInXaml.
        /// </summary>
        /// <param name="view">Представление.</param>
        /// <param name="viewModel">Вью-модель представления.</param>
        /// <param name="viewModelProperty">Свойство, содержащее текст подсказки.</param>
        /// <param name="viewProperty">Свойство, содержащее элемент с подсказкой.</param>
        /// <returns></returns>
        public static IDisposable BindHint<TView, TViewModel, TControl>(
            this TView view,
            TViewModel viewModel,
            Expression<Func<TViewModel, string>> viewModelProperty,
            Expression<Func<TView, TControl>> viewProperty)
            where TView : class, IViewFor<TViewModel>
            where TViewModel : class
            where TControl : DependencyObject
        {
            return viewModel.WhenAnyValue(viewModelProperty)
                .Subscribe(hint => HintAssist.SetHint(
                    viewProperty.Compile().Invoke(view),
                    hint));
        }

        private static void ValidateProperty(
            INotifyDataErrorInfo objectToValidate,
            string propertyName,
            DependencyObject uiElement,
            ref string lastError)
        {
            var error = objectToValidate
                .GetErrors(propertyName)
                .Cast<string>()
                .FirstOrDefault();
            if (lastError == error) return;
            lastError = error;
            ValidationHelper.ClearValidationErrors(uiElement);
            if (string.IsNullOrEmpty(error))
                return;
            ValidationHelper.AddValidationError(
                new ValidationError(new NotifyDataErrorValidationRule(), objectToValidate, error,
                    null),
                uiElement);
        }

        public static IDisposable BindValidationError
            <TView, TViewModel, TValidatableObject, TProperty>(
                this TView view,
                TViewModel viewModel,
                Expression<Func<TViewModel, TValidatableObject>> objectToValidateName,
                Expression<Func<TValidatableObject, TProperty>> propertyToValidate,
                Func<TView, DependencyObject> uiElementDelegate)
            where TViewModel : class
            where TView : IViewFor<TViewModel>
            where TValidatableObject : class, INotifyDataErrorInfo
        {
            string lastError = null;
            var propertyToValidateName = propertyToValidate.GetPropertyName();
            return viewModel.WhenAnyValue(objectToValidateName)
                .Do(objectToValidate =>
                {
                    var uiElement = uiElementDelegate.Invoke(view);
                    if (objectToValidate == null)
                    {
                        ValidationHelper.ClearValidationErrors(uiElement);
                        return;
                    }

                    ValidateProperty(
                        objectToValidate,
                        propertyToValidateName,
                        uiElement,
                        ref lastError);
                })
                .Select(objectToValidate => objectToValidate != null
                    ? Observable.FromEventPattern<DataErrorsChangedEventArgs>(objectToValidate,
                        nameof(objectToValidate.ErrorsChanged))
                    : Observable.Empty<EventPattern<DataErrorsChangedEventArgs>>())
                .Switch()
                .Subscribe(eventArgs =>
                {
                    if (eventArgs.EventArgs.PropertyName != propertyToValidateName)
                        return;
                    var objectToValidate = (INotifyDataErrorInfo) eventArgs.Sender;
                    var uiElement = uiElementDelegate.Invoke(view);
                    ValidateProperty(
                        objectToValidate,
                        propertyToValidateName,
                        uiElement,
                        ref lastError);
                });
        }

        public static IDisposable BindValidationError
            <TView, TViewModel, TProperty>(
                this TView view,
                TViewModel viewModel,
                Expression<Func<TViewModel, TProperty>> propertyToValidate,
                Func<TView, DependencyObject> uiElementDelegate)
            where TViewModel : class, INotifyDataErrorInfo
            where TView : IViewFor<TViewModel>
        {
            string lastError = null;
            var propertyToValidateName = propertyToValidate.GetPropertyName();
            var uiElement = uiElementDelegate.Invoke(view);
            ValidateProperty(
                viewModel,
                propertyToValidateName,
                uiElement,
                ref lastError);
            return Observable.FromEventPattern<DataErrorsChangedEventArgs>(
                    x => viewModel.ErrorsChanged += x,
                    x => viewModel.ErrorsChanged -= x)
                .Subscribe(eventArgs =>
                {
                    if (eventArgs.EventArgs.PropertyName != propertyToValidateName)
                        return;
                    var uiElement = uiElementDelegate.Invoke(view);
                    ValidateProperty(
                        viewModel,
                        propertyToValidateName,
                        uiElement,
                        ref lastError);
                });
        }
    }
}