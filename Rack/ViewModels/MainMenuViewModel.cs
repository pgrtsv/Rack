using System.Reactive.Linq;
using Rack.Localization;
using Rack.Shared;
using ReactiveUI;

namespace Rack.ViewModels
{
    /// <summary>
    /// Модель представления главного меню приложения.
    /// </summary>
    public sealed class MainMenuViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<ILocalization> _localization;

        public MainMenuViewModel(
            ApplicationTabs applicationTabs, 
            ILocalizationService localizationService)
        {
            ApplicationTabs = applicationTabs;
            _localization = localizationService.CurrentLanguage
                .Select(x => localizationService.GetLocalization(App.Name))
                .ToProperty(this, nameof(Localization));
        }

        /// <summary>
        /// Команды уровня приложения, отображаемые в меню.
        /// </summary>
        public ApplicationTabs ApplicationTabs { get; }

        public ILocalization Localization => _localization.Value;
    }
}