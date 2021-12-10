using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using DynamicData;
using GongSolutions.Wpf.DragDrop;
using Newtonsoft.Json.Linq;
using ReactiveUI;

namespace Rack.LogViewer
{
    public sealed class MainWindowViewModel : ReactiveObject, IDropTarget
    {
        private readonly ObservableAsPropertyHelper<bool> _isBusy;
        private readonly SourceList<JObject> _logs = new SourceList<JObject>();
        private string _filePath;
        private bool _isSortAscending;
        private string _selectedSortProperty;

        public MainWindowViewModel()
        {
            Levels = new[]
            {
                new Level("Fatal"),
                new Level("Error"),
                new Level("Warning"),
                new Level("Debug"),
                new Level("Information")
            };

            SortProperties = new[]
            {
                "Timestamp",
                "Level",
                "Exception"
            };

            SelectedSortProperty = SortProperties.First();

            _logs.Connect()
                .Filter(Levels.AsObservableChangeSet()
                    .AutoRefresh(x => x.IsSelected)
                    .Filter(x => x.IsSelected)
                    .QueryWhenChanged<Level, Func<JObject, bool>>(selectedLevels =>
                        log => selectedLevels.Any(x => x.Equals(log.GetValue("Level").Value<string>()))))
                .Sort(this.WhenAnyValue(x => x.SelectedSortProperty, x => x.IsSortAscending,
                    (sortProperty, isAscending) => new LogsComparer(sortProperty, isAscending)))
                .Bind(out var logs)
                .Subscribe();
            Logs = logs;

            this.WhenAnyValue(x => x.FilePath)
                .Where(file => File.Exists(file) && Path.GetExtension(file).Equals(".json"))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(file => LoadLogsFile.Execute(file).Subscribe())
                .Subscribe();


            LoadLogsFile = ReactiveCommand.CreateFromTask<string>(async (path, cancellationToken) =>
            {
                using var reader = File.OpenText(path);
                _logs.Clear();
                while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                    _logs.Add(JObject.Parse(await reader.ReadLineAsync()));
            });

            _isBusy = LoadLogsFile.IsExecuting.ToProperty(this, nameof(IsBusy));
        }

        public ReactiveCommand<string, Unit> LoadLogsFile { get; }

        public ReadOnlyObservableCollection<JObject> Logs { get; }

        public Level[] Levels { get; }

        public string[] SortProperties { get; }

        public string SelectedSortProperty
        {
            get => _selectedSortProperty;
            set => this.RaiseAndSetIfChanged(ref _selectedSortProperty, value);
        }

        public string FilePath
        {
            get => _filePath;
            set => this.RaiseAndSetIfChanged(ref _filePath, value);
        }

        public bool IsBusy => _isBusy.Value;

        public bool IsSortAscending
        {
            get => _isSortAscending;
            set => this.RaiseAndSetIfChanged(ref _isSortAscending, value);
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is DataObject dataObject &&
                dataObject.GetDataPresent(DataFormats.FileDrop) &&
                dataObject.ContainsFileDropList() &&
                Path.GetExtension(dataObject.GetFileDropList()[0]) == ".json")
                dropInfo.Effects = DragDropEffects.Copy;
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is DataObject dataObject &&
                dataObject.ContainsFileDropList() &&
                Path.GetExtension(dataObject.GetFileDropList()[0]) == ".json")
                FilePath = dataObject.GetFileDropList()[0];
        }

        private sealed class LogsComparer : IComparer<JObject>
        {
            private readonly bool _isAscending;
            private readonly string _propertyName;

            public LogsComparer(string propertyName, bool isAscending)
            {
                _propertyName = propertyName;
                _isAscending = isAscending;
            }

            public int Compare(JObject x, JObject y)
            {
                var xProperty = x.GetValue(_propertyName) as IComparable;
                var yProperty = y.GetValue(_propertyName) as IComparable;
                if (xProperty is null && yProperty is null) return 0;
                if (xProperty is null) return -1;
                if (yProperty is null) return 1;
                return _isAscending ? xProperty.CompareTo(yProperty) : -xProperty.CompareTo(yProperty);
            }
        }
    }
}