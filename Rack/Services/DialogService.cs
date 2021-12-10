using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using DryIoc;
using Microsoft.WindowsAPICodePack.Dialogs;
using Rack.Navigation;
using Rack.ViewModels;
using Rack.Views;
using Rack.Wpf.Controls;
using ReactiveUI;
using IContainer = DryIoc.IContainer;

namespace Rack.Services
{
    public sealed class DialogService : IDialogService
    {
        private readonly IContainer _container;
        private readonly MainWindow _mainWindow;

        public DialogService(IContainer container,
            MainWindow mainWindow)
        {
            _container = container;
            _mainWindow = mainWindow;
        }

        public async Task<IReadOnlyDictionary<string, object>> ShowAsync<TViewModel>(
            IReadOnlyDictionary<string, object> dialogParameters)
            where TViewModel : class, IDialogViewModel
        {
            var viewModel = _container.Resolve<TViewModel>();
            var view = _container.Resolve<IViewFor<TViewModel>>();
            view.ViewModel = viewModel;
            var dialogWindow = new DefaultDialogWindow
            {
                Content = view,
                Owner = _mainWindow
            };

            IReadOnlyDictionary<string, object> result = null;

            void CloseRequestedHandler(IReadOnlyDictionary<string, object> results)
            {
                result = results;
                dialogWindow.Close();
            }

            viewModel.RequestClose += CloseRequestedHandler;

            void ClosedHandler(object sender, EventArgs e)
            {
                dialogWindow.Closed -= ClosedHandler;
                viewModel.RequestClose -= CloseRequestedHandler;
                result ??= viewModel.OnDialogClosed();
                if (viewModel is IDisposable disposable) disposable.Dispose();
                dialogWindow.Content = null;
                dialogWindow.DataContext = null;
            }

            void DialogLoadedHandler(object sender, EventArgs e)
            {
                dialogWindow.Loaded -= DialogLoadedHandler;
                viewModel.OnDialogOpened(dialogParameters);
            }

            dialogWindow.Closed += ClosedHandler;
            dialogWindow.Loaded += DialogLoadedHandler;
            using var canCloseWindowBinding = viewModel
                .WhenAnyValue(x => x.CanClose)
                .Subscribe(canClose => dialogWindow.IsCloseButtonEnabled = canClose);
            dialogWindow.Show();
            await dialogWindow.Events().Closed.FirstAsync();
            return result;
        }

        public async Task<IReadOnlyDictionary<string, object>> ShowDialogAsync<TViewModel>(
            IReadOnlyDictionary<string, object> dialogParameters)
            where TViewModel : class, IDialogViewModel
        {
            var viewModel = _container.Resolve<TViewModel>();
            var view = _container.Resolve<IViewFor<TViewModel>>();
            view.ViewModel = viewModel;
            var dialogWindow = new DefaultDialogWindow
            {
                Content = view,
                Owner = _mainWindow
            };
            IReadOnlyDictionary<string, object> result = null;

            void CloseRequestedHandler(IReadOnlyDictionary<string, object> results)
            {
                result = results;
                dialogWindow.Close();
            }

            viewModel.RequestClose += CloseRequestedHandler;

            void ClosedHandler(object sender, EventArgs e)
            {
                dialogWindow.Closed -= ClosedHandler;
                viewModel.RequestClose -= CloseRequestedHandler;
                result ??= viewModel.OnDialogClosed();
                if (viewModel is IDisposable disposable) disposable.Dispose();
                dialogWindow.Content = null;
                dialogWindow.DataContext = null;
            }

            void DialogLoadedHandler(object sender, EventArgs e)
            {
                dialogWindow.Loaded -= DialogLoadedHandler;
                viewModel.OnDialogOpened(dialogParameters);
            }

            dialogWindow.Closed += ClosedHandler;
            dialogWindow.Loaded += DialogLoadedHandler;
            using var canCloseWindowBinding = viewModel
                .WhenAnyValue(x => x.CanClose)
                .Subscribe(canClose => dialogWindow.IsCloseButtonEnabled = canClose);
            await Observable.Start(
                () => dialogWindow.ShowDialog(),
                RxApp.MainThreadScheduler).FirstAsync();
            return result;
        }

        public Task ShowHelpAsync() => ShowAsync<HelpViewModel>(new Dictionary<string, object>());

        public Task ShowAppSettingsAsync() =>
            ShowAsync<AppSettingsViewModel>(new Dictionary<string, object>());

        public async Task<IDialogService.YesNoAnswer> ShowYesNoQuestionDialogAsync(
            string title,
            string content)
        {
            var results = await ShowDialogAsync<YesNoViewModel>(new Dictionary<string, object>
                {{"Title", title}, {"Content", content}});
            if (results["IsCancelled"] is true) return IDialogService.YesNoAnswer.Cancel;
            return results["IsYesAnswered"] is true
                ? IDialogService.YesNoAnswer.Yes
                : IDialogService.YesNoAnswer.No;
        }

        public string ShowOpenFileDialog(string title, IDictionary<string, string> filters)
        {
            using var dialog = new CommonOpenFileDialog {Title = title, Multiselect = false};
            foreach (var (key, value) in filters)
                dialog.Filters.Add(new CommonFileDialogFilter(key, value));
            return dialog.ShowDialog() switch
            {
                CommonFileDialogResult.Ok => dialog.FileName,
                CommonFileDialogResult.Cancel => string.Empty,
                CommonFileDialogResult.None => string.Empty,
                _ => throw new InvalidEnumArgumentException()
            };
        }

        public IEnumerable<string> ShowOpenFilesDialog(string title,
            IDictionary<string, string> filters)
        {
            using var dialog = new CommonOpenFileDialog {Title = title, Multiselect = true};
            foreach (var (key, value) in filters)
                dialog.Filters.Add(new CommonFileDialogFilter(key, value));
            return dialog.ShowDialog() switch
            {
                CommonFileDialogResult.Ok => dialog.FileNames,
                CommonFileDialogResult.Cancel => Enumerable.Empty<string>(),
                CommonFileDialogResult.None => Enumerable.Empty<string>(),
                _ => throw new InvalidEnumArgumentException()
            };
        }

        public string ShowOpenDirectoryDialog(string title)
        {
            using var dialog = new CommonOpenFileDialog {IsFolderPicker = true, Title = title};
            return dialog.ShowDialog() switch
            {
                CommonFileDialogResult.Ok => dialog.FileName,
                CommonFileDialogResult.Cancel => string.Empty,
                CommonFileDialogResult.None => string.Empty,
                _ => throw new InvalidEnumArgumentException()
            };
        }

        public string ShowSaveFileDialog(string title, string defaultFileName,
            IDictionary<string, string> filters)
        {
            using var dialog = new CommonSaveFileDialog
                {Title = title, DefaultFileName = defaultFileName};
            foreach (var (key, value) in filters)
                dialog.Filters.Add(new CommonFileDialogFilter(key, value));
            return dialog.ShowDialog() switch
            {
                CommonFileDialogResult.Ok => dialog.FileName,
                CommonFileDialogResult.Cancel => string.Empty,
                CommonFileDialogResult.None => string.Empty,
                _ => throw new InvalidEnumArgumentException()
            };
        }
    }
}