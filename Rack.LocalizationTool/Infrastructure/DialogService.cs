using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;
using Rack.Navigation;

namespace Rack.LocalizationTool.Infrastructure
{
    public sealed class DialogService : IDialogService
    {
        private readonly MainWindow _mainWindow;

        public DialogService(
            MainWindow mainWindow) =>
            _mainWindow = mainWindow;

        /// <inheritdoc />
        public async Task<IReadOnlyDictionary<string, object>> ShowAsync<TViewModel>(
            IReadOnlyDictionary<string, object> dialogParameters)
            where TViewModel : class, IDialogViewModel => throw new NotImplementedException();

        /// <inheritdoc />
        public async Task<IReadOnlyDictionary<string, object>> ShowDialogAsync<TViewModel>(
            IReadOnlyDictionary<string, object> dialogParameters)
            where TViewModel : class, IDialogViewModel => throw new NotImplementedException();

        /// <inheritdoc />
        public async Task ShowHelpAsync() => throw new NotImplementedException();

        /// <inheritdoc />
        public async Task ShowAppSettingsAsync() => throw new NotImplementedException();

        /// <inheritdoc />
        public async Task<IDialogService.YesNoAnswer> ShowYesNoQuestionDialogAsync(string title,
            string content) => throw new NotImplementedException();

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