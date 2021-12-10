using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Rack.ViewModels;
using Rack.Wpf.Reactive;
using ReactiveUI;

namespace Rack.Views
{
    public partial class Changelogs : ReactiveUserControl<ChangelogsViewModel>
    {
        public Changelogs()
        {
            InitializeComponent();
            IDisposable activationCleanUp = null;
            activationCleanUp = this.WhenActivated(cleanUp =>
            {
                // ReSharper disable once AccessToModifiedClosure
                activationCleanUp.DisposeWith(cleanUp);

                new BindingHelper<Changelogs, ChangelogsViewModel>(
                        this,
                        cleanUp)
                    .OneWayBind(
                        x => x.Localization["Changelogs.Modules"],
                        x => x.ModulesGroupBox.Header)

                    .Do(() =>
                    {
                        ModulesListBox.DisplayMemberPath =
                            nameof(ChangelogsViewModel.ModuleViewModel.Name);
                    })
                    .OneWayBind(
                        x => x.Modules,
                        x => x.ModulesListBox.ItemsSource)
                    .Bind(
                        x => x.SelectedModule,
                        x => x.ModulesListBox.SelectedItem)
                    .Do(() =>
                    {
                        ModulesListBox.SelectedItem = ModulesListBox.Items[0];
                    })

                    .Do(() =>
                    {
                        SelectedChangelogMarkdownViewer.UCRootPath = "Changelogs/";
                    })
                    .OneWayBind(
                        x => x.SelectedChangelog,
                        x => x.SelectedChangelogMarkdownViewer.Markdown)
                    ;
            });
        }
    }
}