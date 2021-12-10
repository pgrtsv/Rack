using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Microsoft.Xaml.Behaviors;
using ReactiveUI;

namespace Rack.Wpf.Reactive
{
    /// <summary>
    /// Вспомогательный класс, уменьшающий количество бойлерплейт-кода
    /// при создании ReactiveUI биндингов.
    /// </summary>
    public sealed class BindingHelper<TView, TViewModel>
        where TView : DependencyObject, IViewFor<TViewModel>
        where TViewModel : class, IActivatableViewModel
    {
        private readonly CompositeDisposable _cleanUp;
        private readonly TView _view;

        public BindingHelper(TView view, CompositeDisposable cleanUp)
        {
            _view = view;
            _cleanUp = cleanUp;
        }

        public BindingHelper<TView, TViewModel> Bind<TProperty, TVProp>(
            Expression<Func<TViewModel, TProperty>> vmProperty,
            Expression<Func<TView, TVProp>> viewProperty)
        {
            _view.Bind(
                    _view.ViewModel,
                    vmProperty,
                    viewProperty)
                .DisposeWith(_cleanUp);
            return this;
        }

        public BindingHelper<TView, TViewModel> Bind<TProperty, TVProp>(
            Expression<Func<TViewModel, TProperty>> vmProperty,
            Expression<Func<TView, TVProp>> viewProperty,
            Func<TProperty, TVProp> vmToViewConverter,
            Func<TVProp, TProperty> viewToVmConverter)
        {
            _view.Bind(
                    _view.ViewModel,
                    vmProperty,
                    viewProperty,
                    vmToViewConverter,
                    viewToVmConverter)
                .DisposeWith(_cleanUp);
            return this;
        }

        public BindingHelper<TView, TViewModel> Bind<TProperty, TVProp, TDontCare>(
            Expression<Func<TViewModel, TProperty>> vmProperty,
            Expression<Func<TView, TVProp>> viewProperty,
            IObservable<TDontCare> signalViewUpdate)
        {
            _view.Bind(
                    _view.ViewModel,
                    vmProperty,
                    viewProperty,
                    signalViewUpdate)
                .DisposeWith(_cleanUp);
            return this;
        }

        public BindingHelper<TView, TViewModel> Bind<TProperty, TVProp, TDontCare>(
            Expression<Func<TViewModel, TProperty>> vmProperty,
            Expression<Func<TView, TVProp>> viewProperty,
            IObservable<TDontCare> signalViewUpdate,
            Func<TProperty, TVProp> vmToViewConverter,
            Func<TVProp, TProperty> viewToVmConverter)
        {
            _view.Bind(
                    _view.ViewModel,
                    vmProperty,
                    viewProperty,
                    signalViewUpdate,
                    vmToViewConverter,
                    viewToVmConverter)
                .DisposeWith(_cleanUp);
            return this;
        }

        public BindingHelper<TView, TViewModel> OneWayBind<TProperty, TVProp>(
            Expression<Func<TViewModel, TProperty>> vmProperty,
            Expression<Func<TView, TVProp>> viewProperty)
        {
            _view.OneWayBind(
                    _view.ViewModel,
                    vmProperty,
                    viewProperty)
                .DisposeWith(_cleanUp);
            return this;
        }

        public BindingHelper<TView, TViewModel> OneWayBind<TProperty, TVProp>(
            Expression<Func<TViewModel, TProperty>> vmProperty,
            Expression<Func<TView, TVProp>> viewProperty,
            Func<TProperty, TVProp> selector)
        {
            _view.OneWayBind(
                    _view.ViewModel,
                    vmProperty,
                    viewProperty,
                    selector)
                .DisposeWith(_cleanUp);
            return this;
        }

        public BindingHelper<TView, TViewModel> BindCommand<TCommand, TControl>(
            Expression<Func<TViewModel, TCommand>> propertyName,
            Expression<Func<TView, TControl>> controlName)
            where TCommand : ICommand
        {
            _view.BindCommand(_view.ViewModel, propertyName, controlName)
                .DisposeWith(_cleanUp);
            return this;
        }

        public BindingHelper<TView, TViewModel> BindCommand<TCommand, TControl, TParameter>(
            Expression<Func<TViewModel, TCommand>> propertyName,
            Expression<Func<TView, TControl>> controlName,
            Expression<Func<TViewModel, TParameter>> withParameter)
            where TCommand : ICommand
        {
            _view.BindCommand(_view.ViewModel, propertyName, controlName, withParameter)
                .DisposeWith(_cleanUp);
            return this;
        }

        public BindingHelper<TView, TViewModel> BindCommand<TCommand, TControl, TParameter>(
            Expression<Func<TViewModel, TCommand>> propertyName,
            Expression<Func<TView, TControl>> controlName,
            IObservable<TParameter> withParameter)
            where TCommand : ICommand
        {
            _view.BindCommand(_view.ViewModel, propertyName, controlName, withParameter)
                .DisposeWith(_cleanUp);
            return this;
        }

        public BindingHelper<TView, TViewModel> BindHint<TControl>(
            Expression<Func<TViewModel, string>> hintPropertyName,
            Expression<Func<TView, TControl>> control)
            where TControl : DependencyObject
        {
            _view.BindHint(
                    _view.ViewModel,
                    hintPropertyName,
                    control)
                .DisposeWith(_cleanUp);
            return this;
        }

        public BindingHelper<TView, TViewModel> BindValidationError<TValidatableObject, TProperty>(
            Expression<Func<TViewModel, TValidatableObject>> objectToValidateName,
            Expression<Func<TValidatableObject, TProperty>> propertyToValidate,
            Func<TView, UIElement> uiElementDelegate)
            where TValidatableObject : class, INotifyDataErrorInfo
        {
            _view.BindValidationError(
                    _view.ViewModel,
                    objectToValidateName,
                    propertyToValidate,
                    uiElementDelegate)
                .DisposeWith(_cleanUp);
            return this;
        }

        public BindingHelper<TView, TViewModel> Do(Action action)
        {
            action.Invoke();
            return this;
        }

        /// <summary>
        /// Прикрепляет поведения <see cref="behaviors"/> к элементу <see cref="viewProperty"/>.
        /// При деактивации представления автоматически открепляет поведения.
        /// </summary>
        /// <typeparam name="TViewProperty">Тип элемента представления.</typeparam>
        /// <param name="viewProperty">Элемент представления.</param>
        /// <param name="behaviors">Поведения.</param>
        public BindingHelper<TView, TViewModel> AttachBehaviors<TViewProperty>(
            Expression<Func<TView, TViewProperty>> viewProperty,
            params Behavior[] behaviors) 
            where TViewProperty : DependencyObject
        {
            var dependencyObject = viewProperty.Compile().Invoke(_view);
            var objectBehaviors = Interaction.GetBehaviors(dependencyObject);
            objectBehaviors.Clear();
            foreach (var behavior in behaviors)
                objectBehaviors.Add(behavior);
            Disposable.Create(() =>
                {
                    foreach (var behavior in behaviors)
                        objectBehaviors.Remove(behavior);
                })
                .DisposeWith(_cleanUp);
            return this;
        }

        /// <summary>
        /// Прикрепляет поведения <see cref="behaviors"/> ко всем дочерним элементам представления
        /// типа <see cref="TViewProperty"/>.
        /// При деактивации представления автоматически открепляет поведения.
        /// </summary>
        /// <typeparam name="TViewProperty">Тип дочерних элементов представления.</typeparam>
        /// <param name="behaviors">Провайдеры поведений.</param>
        public BindingHelper<TView, TViewModel> AttachBehaviors<TViewProperty>(
            params Func<Behavior>[] behaviors)
            where TViewProperty : DependencyObject
        {
            foreach (var child in _view.FindChildren<TViewProperty>())
            {
                var objectBehaviors = Interaction.GetBehaviors(child);
                var materialisedBehaviors = behaviors
                    .Select(x => x.Invoke())
                    .ToArray();
                foreach (var behavior in materialisedBehaviors)
                    objectBehaviors.Add(behavior);
                Disposable.Create(() =>
                    {
                        foreach (var behavior in materialisedBehaviors)
                            objectBehaviors.Remove(behavior);
                    })
                    .DisposeWith(_cleanUp);
            }
            
            return this;
        }
    }
}