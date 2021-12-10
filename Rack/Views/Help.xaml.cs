using System;
using System.Reactive.Disposables;
using System.Windows.Controls;
using Rack.Shared.Help;
using Rack.ViewModels;
using Rack.Wpf.Reactive;
using ReactiveUI;

namespace Rack.Views
{
    public partial class Help : ReactiveUserControl<HelpViewModel>
    {
        public Help()
        {
            InitializeComponent();
            IDisposable activationCleanUp = null;
            activationCleanUp = this.WhenActivated(cleanUp =>
            {
                // ReSharper disable once AccessToModifiedClosure
                activationCleanUp.DisposeWith(cleanUp);

                new BindingHelper<Help, HelpViewModel>(
                        this,
                        cleanUp)
                    .OneWayBind(
                        x => x.Modules,
                        x => x.ModulesListBox.ItemsSource)
                    .Bind(
                        x => x.SelectedModule,
                        x => x.ModulesListBox.SelectedItem)

                    .OneWayBind(
                        x => x.Localization["Pages"],
                        x => x.PagesGroupBox.Header)

                    .Do(() =>
                    {
                        PagesListBox.DisplayMemberPath = nameof(HelpPage.Header);
                    })
                    .OneWayBind(
                        x => x.PagesForSelectedModule,
                        x => x.PagesListBox.ItemsSource)
                    .Bind(
                        x => x.SelectedPage,
                        x => x.PagesListBox.SelectedItem)

                    .Do(() =>
                    {
                        HelpPageMarkdownViewer.UCRootPath = "HelpFiles/";
                        ViewModel.WhenAnyValue(x => x.SelectedPage)
                            .Subscribe(_ => HelpPageMarkdownViewer.BringIntoView())
                            .DisposeWith(cleanUp);
                    })
                    .OneWayBind(
                        x => x.SelectedPage.Content,
                        x => x.HelpPageMarkdownViewer.Markdown)

                    ;
            });
        }
    }
}
