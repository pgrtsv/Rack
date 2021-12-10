using System;
using Rack.Localization;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Rack.CrossSectionUtils.Wpf.Abstractions.Validation.Messages
{
    public abstract class ValidatorMessagesBase : ReactiveObject
    {
        protected ValidatorMessagesBase(ILocalizationService localizationService)
        {
            localizationService.CurrentLanguage
                .Subscribe(_ =>
                    Localization = localizationService.GetLocalization("Rack.CrossSectionUtils.Wpf"));
        }

        [Reactive]
        protected ILocalization Localization { get; private set; }
    }
}