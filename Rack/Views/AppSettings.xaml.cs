using System;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Rack.ViewModels;
using Rack.Wpf.Reactive;
using ReactiveUI;

namespace Rack.Views
{
    public partial class AppSettings : ReactiveUserControl<AppSettingsViewModel>
    {
        public AppSettings()
        {
            InitializeComponent();
            this.WhenActivated(cleanUp =>
            {
                new BindingHelper<AppSettings, AppSettingsViewModel>(
                    this,
                    cleanUp)
                    .OneWayBind(
                        x => x.Localization["SaveSettings"],
                        x => x.SaveButton.ToolTip)
                    .BindCommand(
                        x => x.SaveSettings,
                        x => x.SaveButton)

                    .OneWayBind(
                        x => x.Localization["ResetSettings"],
                        x => x.ResetButton.ToolTip)
                    .BindCommand(
                        x => x.ResetSettings,
                        x => x.ResetButton)

                    .Do(() =>
                    {
                        AppVersionTextBlock.Text = $"v{ViewModel.AppVersion.ToString(3)}";
                    })

                    .OneWayBind(
                        x => x.Localization["FontSize"],
                        x => x.FontModeTextBlock.Text)
                    .OneWayBind(
                        x => x.Localization["FontMode.Regular"],
                        x => x.RegularFontModeIcon.ToolTip)
                    .OneWayBind(
                        x => x.Localization["FontMode.Big"],
                        x => x.BigFontModeIcon.ToolTip)
                    .Do(() =>
                    {
                        FontModeListBox.Events().SelectionChanged
                            .Subscribe(x =>
                            {
                                if (x.AddedItems.Count == 0)
                                    return;
                                var selectedIcon = x.AddedItems[0];
                                if (selectedIcon == RegularFontModeIcon)
                                    ViewModel.ApplicationSettings.ModeIdentifier =
                                        ApplicationSettings.FontModeIdentifier.Regular;
                                else if (selectedIcon == BigFontModeIcon)
                                    ViewModel.ApplicationSettings.ModeIdentifier =
                                        ApplicationSettings.FontModeIdentifier.Big;
                                else
                                    throw new ArgumentOutOfRangeException();
                            })
                            .DisposeWith(cleanUp);
                        ViewModel.WhenAnyValue(x => x.ApplicationSettings.Mode)
                            .Subscribe(mode =>
                            {
                                if (mode == ApplicationSettings.FontMode.Regular)
                                    FontModeListBox.SelectedItem = RegularFontModeIcon;
                                else if (mode == ApplicationSettings.FontMode.Big)
                                    FontModeListBox.SelectedItem = BigFontModeIcon;
                                else
                                    throw new ArgumentOutOfRangeException();
                            })
                            .DisposeWith(cleanUp);
                    })
                    //.OneWayBind(
                    //    x => x.Localization["FontSize"],
                    //    x => x.FontSizeGroupBox.Header)
                    //.Do(() =>
                    //{
                    //    FontSizeSlider.Minimum = 12;
                    //    FontSizeSlider.Maximum = 30;
                    //})
                    //.Bind(
                    //    x => x.ApplicationSettings.FontSize,
                    //    x => x.FontSizeSlider.Value)

                    .OneWayBind(
                        x => x.Localization["Settings"],
                        x => x.SettingsGroupBox.Header)

                    .BindHint(
                        x => x.Localization["Language"],
                        x => x.LanguageComboBox)
                    .Do(() =>
                    {
                        LanguageComboBox.ItemsSource = ViewModel.AvailableLanguages;
                    })
                    .Bind(
                        x => x.ApplicationSettings.Language,
                        x => x.LanguageComboBox.SelectedItem)

                    .OneWayBind(
                        x => x.Localization["Modules"],
                        x => x.ModulesGroupBox.Header)
                    .Do(() =>
                    {
                        ModulesItemsControl.DisplayMemberPath =
                            nameof(AppSettingsViewModel.ModuleViewModel.Name);
                    })
                    .OneWayBind(
                        x => x.Modules,
                        x => x.ModulesItemsControl.ItemsSource)
                    ;
            });
        }
    }
}