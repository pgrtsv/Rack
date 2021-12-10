using System;
using System.Collections.Generic;
using System.Reactive;
using Rack.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Rack.Wpf.Controls
{
    public sealed class ConfirmationViewModel : ReactiveObject, IDialogViewModel
    {
        public ConfirmationViewModel()
        {
            ConfirmCommand = ReactiveCommand.Create(() =>
                RequestClose?.Invoke(new Dictionary<string, object> {{"IsConfirmed", true}}));
        }

        public ReactiveCommand<Unit, Unit> ConfirmCommand { get; }

        [Reactive] public string ConfirmationContent { get; private set; }

        [Reactive] public string Title { get; private set; }

        public bool CanClose => true;

        public event Action<IReadOnlyDictionary<string, object>> RequestClose;

        public IReadOnlyDictionary<string, object> OnDialogClosed() =>
            new Dictionary<string, object> {{"IsConfirmed", false}};

        public void OnDialogOpened(IReadOnlyDictionary<string, object> parameters)
        {
            Title = (string) parameters["Title"];
            ConfirmationContent = (string) parameters["Content"];
        }
    }
}