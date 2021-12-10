using System;
using Rack.LocalizationTool.Models.LocalizationProblem;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Rack.LocalizationTool.Infrastructure
{
    public sealed class FilesFilter : ReactiveObject
    {
        public FilesFilter(string header, Func<FileWithUnlocalizedStrings, bool> filter, bool isChecked)
        {
            Header = header;
            Filter = filter;
            IsChecked = isChecked;
        }

        public string Header { get; }

        public Func<FileWithUnlocalizedStrings, bool> Filter { get; }

        [Reactive] public bool IsChecked { get; set; }
    }
}