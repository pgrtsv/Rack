using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Rack.LocalizationTool.Infrastructure
{
    /// <summary>
    /// Модель Представления актуальна для инициализации фразы локализации по некоторому ключу в самых простых случаях.
    /// </summary>
    public sealed class NewKeyPhraseViewModel: ReactiveObject
    {
        public NewKeyPhraseViewModel(string key)
        {
            Key = key;
        }

        /// <summary>
        /// Ключ локализации.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Фраза локализации соответсвующая ключу.
        /// </summary>
        [Reactive]
        public string Phrase { get; set; }
    }
}