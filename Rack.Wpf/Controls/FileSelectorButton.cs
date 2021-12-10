using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace Rack.Wpf.Controls
{
    /// <summary>
    /// Кнопка, позволяющая выбрать файл(-ы) с помощью модального диалога Windows и выполнить соотвествующую
    /// команду.
    /// </summary>
    [Obsolete("Используйте методы-расширения IDialogService в Rack.Shared/DialogServiceExtensions.cs.")]
    public class FileSelectorButton : Button
    {
        public enum SelectionMode
        {
            Save,
            Open
        }

        public static readonly DependencyProperty MultiSelectionProperty = DependencyProperty.Register(
            "MultiSelection", typeof(bool), typeof(FileSelectorButton));

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
            "Filter", typeof(string), typeof(FileSelectorButton));

        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
            "Mode", typeof(SelectionMode), typeof(FileSelectorButton), new PropertyMetadata(SelectionMode.Open));

        public static readonly DependencyProperty DefaultFileNameProperty = DependencyProperty.Register(
            "DefaultFileName", typeof(string), typeof(FileSelectorButton));

        public string DefaultFileName
        {
            get => (string) GetValue(DefaultFileNameProperty);
            set => SetValue(DefaultFileNameProperty, value);
        }

        public SelectionMode Mode
        {
            get => (SelectionMode) GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        /// <summary>
        /// true, если можно выбрать несколько файлов.
        /// </summary>
        public bool MultiSelection
        {
            get => (bool) GetValue(MultiSelectionProperty);
            set => SetValue(MultiSelectionProperty, value);
        }

        /// <summary>
        /// Фильтр файлов.
        /// </summary>
        public string Filter
        {
            get => (string) GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        protected override void OnClick()
        {
            ICommand command;
            switch (Mode)
            {
                case SelectionMode.Save:
                    var saveDialog = new SaveFileDialog {Filter = Filter, FileName = DefaultFileName};
                    if (!saveDialog.ShowDialog().Value) return;
                    command = Command;
                    if (command == null) return;
                    command.Execute(saveDialog.FileName);
                    break;
                case SelectionMode.Open:
                    var openDialog = new OpenFileDialog
                        {Multiselect = MultiSelection, Filter = Filter, FileName = DefaultFileName};
                    if (!openDialog.ShowDialog().Value) return;
                    command = Command;
                    if (command == null) return;
                    foreach (var filename in openDialog.FileNames)
                        command.Execute(filename);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}