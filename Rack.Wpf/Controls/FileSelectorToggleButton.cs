using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;

namespace Rack.Wpf.Controls
{
    /// <summary>
    /// Кнопка-переключатель, позволяющая выбрать файл(-ы) с помощью модального окна Windows и выполнить
    /// соответствующую команду.
    /// </summary>
    [Obsolete("Используйте методы-расширения IDialogService в Rack.Shared/DialogServiceExtensions.cs.")]
    public class FileSelectorToggleButton : ToggleButton
    {
        public static readonly DependencyProperty MultiSelectionProperty = DependencyProperty.Register(
            "MultiSelection", typeof(bool), typeof(FileSelectorToggleButton));

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
            "Filter", typeof(string), typeof(FileSelectorToggleButton));

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
            var dialog = new OpenFileDialog {Multiselect = MultiSelection, Filter = Filter};
            if (!dialog.ShowDialog().Value) return;
            var command = Command;
            if (command == null) return;
            foreach (var filename in dialog.FileNames)
                command.Execute(filename);
        }
    }
}