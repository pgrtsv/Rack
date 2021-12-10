using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using GongSolutions.Wpf.DragDrop;
using Rack.LocalizationTool.Infrastructure;
using Rack.LocalizationTool.Models.LocalizationData;
using Rack.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Rack.LocalizationTool
{
    public sealed class MainWindowViewModel : ReactiveObject, IDisposable, IDropTarget
    {
        private readonly CompositeDisposable _compositeDisposable;
        private readonly ObservableAsPropertyHelper<string> _currentProjectPath;
        private readonly ObservableAsPropertyHelper<bool> _isProjectViewModelSelected;
        private readonly IDialogService _dialogService;

        public MainWindowViewModel(MainWindow mainWindow)
        {
            _compositeDisposable = new CompositeDisposable();
            _dialogService = new DialogService(mainWindow);

            _currentProjectPath = this.WhenAnyValue(x => x.SelectedProjectViewModel)
                .Where(x => x != null)
                .Select(x => x.WhenAnyValue(y => y.ProjectPath).StartWith(x.ProjectPath))
                .Switch()
                .Select(x => x)
                .ToProperty(this, nameof(CurrentProjectPath))
                .DisposeWith(_compositeDisposable);

            _isProjectViewModelSelected = this.WhenAnyValue(x => x.SelectedProjectViewModel, selector: x => x != null)
                .ToProperty(this, nameof(IsProjectViewModelSelected));

            ProjectViewModels = new ObservableCollection<ProjectViewModel>();

            SetProject = ReactiveCommand.CreateFromTask<string>(async filePath =>
            {
                var currentProject = await ProjectLocalizationData
                    .InitProjectLocalizationData(filePath, RxApp.MainThreadScheduler);
                if (currentProject == null)
                    return;

                var existingViewModel = ProjectViewModels.FirstOrDefault(x =>
                    x.ProjectPath.Equals(filePath, StringComparison.OrdinalIgnoreCase));
                if (existingViewModel != null)
                {
                    SelectedProjectViewModel = existingViewModel;
                    if (await existingViewModel.AnalyzeProject.CanExecute.FirstAsync())
                        existingViewModel.AnalyzeProject.Execute().Subscribe();
                    return;
                }

                var viewModel = new ProjectViewModel(currentProject)
                    .DisposeWith(_compositeDisposable);
                ProjectViewModels.Add(viewModel);
                SelectedProjectViewModel = viewModel;
                if (await viewModel.AnalyzeProject.CanExecute.FirstAsync())
                    viewModel.AnalyzeProject.Execute().Subscribe();
            }).DisposeWith(_compositeDisposable);

            SetProjectWithDialog = ReactiveCommand.CreateFromObservable(() =>
            {
                var fileName = _dialogService.ShowOpenFileDialog("Выберите проект",
                    new Dictionary<string, string> {{".csproj files", "*.csproj"}});
                if (string.IsNullOrEmpty(fileName)) return Observable.Empty<Unit>();
                return SetProject.Execute(fileName);
            }).DisposeWith(_compositeDisposable);

            CloseProject = ReactiveCommand.Create<ProjectViewModel>(viewModel =>
            {
                ProjectViewModels.Remove(viewModel);
                viewModel.Dispose();
            });
        }

        [Reactive] public ProjectViewModel SelectedProjectViewModel { get; set; }

        public ObservableCollection<ProjectViewModel> ProjectViewModels { get; }

        public string CurrentProjectPath => _currentProjectPath.Value;

        public bool IsProjectViewModelSelected => _isProjectViewModelSelected.Value;

        public ReactiveCommand<Unit, Unit> SetProjectWithDialog { get; }

        public ReactiveCommand<string, Unit> SetProject { get; }

        public ReactiveCommand<ProjectViewModel, Unit> CloseProject { get; }

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is DataObject dataObject &&
                dataObject.GetDataPresent(DataFormats.FileDrop) &&
                dataObject.ContainsFileDropList() &&
                Path.GetExtension(dataObject.GetFileDropList()[0]) == ".csproj")
                dropInfo.Effects = DragDropEffects.Copy;
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is DataObject dataObject &&
                dataObject.ContainsFileDropList() &&
                Path.GetExtension(dataObject.GetFileDropList()[0]) == ".csproj")
                SetProject.Execute(dataObject.GetFileDropList()[0]).Subscribe();
        }
    }
}