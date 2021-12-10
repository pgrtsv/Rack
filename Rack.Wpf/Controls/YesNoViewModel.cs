using System;
using System.Collections.Generic;
using System.Reactive;
using Rack.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Rack.Wpf.Controls
{
    public sealed class YesNoViewModel : ReactiveObject, IDialogViewModel
    {
        public YesNoViewModel()
        {
            YesCommand = ReactiveCommand.Create(() => RequestClose?.Invoke(new Dictionary<string, object>
            {
                ["IsCancelled"] = false,
                ["IsYesAnswered"] = true
            }));
            NoCommand = ReactiveCommand.Create(() => RequestClose?.Invoke(new Dictionary<string, object>
            {
                ["IsCancelled"] = false,
                ["IsYesAnswered"] = false
            }));
        }

        public ReactiveCommand<Unit, Unit> YesCommand { get; }
        public ReactiveCommand<Unit, Unit> NoCommand { get; }

        [Reactive]
        public string Content { get; private set; }

        public IReadOnlyDictionary<string, object> OnDialogClosed() => new Dictionary<string, object>
        {
            ["IsCancelled"] = true
        };

        [Reactive]
        public string Title { get; private set; }

        public bool CanClose => true;
        public event Action<IReadOnlyDictionary<string, object>> RequestClose;

        public void OnDialogOpened(IReadOnlyDictionary<string, object> parameters)
        {
            Title = (string) parameters["Title"];
            Content = (string) parameters["Content"];
        }
    }
}