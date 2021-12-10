using System;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Rack.Wpf.Controls
{
    [Obsolete("Используйте методы-расширения IDialogService в Rack.Shared/DialogServiceExtensions.cs.")]
    public class DirectorySelectorButton : Button
    {
        protected override void OnClick()
        {
            using (var dialog = new CommonOpenFileDialog {IsFolderPicker = true})
            {
                if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;

                Command?.Execute(dialog.FileName);
            }
        }
    }
}