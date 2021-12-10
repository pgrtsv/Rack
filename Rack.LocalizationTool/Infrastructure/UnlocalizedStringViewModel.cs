using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using Rack.LocalizationTool.Models.LocalizationData;
using Rack.LocalizationTool.Models.LocalizationProblem;
using Rack.LocalizationTool.Models.ResolveOptions;
using Rack.LocalizationTool.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Rack.LocalizationTool.Infrastructure
{
    /// <summary>
    /// Способ локализации нелокализированной строки.
    /// </summary>
    public enum UnlocalizedResolveMode
    {
        /// <summary>
        /// Не определён.
        /// </summary>
        Undefined,
        /// <summary>
        /// Замена абсолютно новым ключом (пара ключ-фраза не существует ни в одной локализации).
        /// </summary>
        ReplaceWithNewKeyPhrase,
        /// <summary>
        /// Замена существующим ключом (но пара ключ-фраза не существует в выбранной локализацией).
        /// </summary>
        ReplaceWithExtendKeyPhrases,
        /// <summary>
        /// Замена существующим ключом (пара ключ-фраза существует в выбранной локализацией).
        /// </summary>
        ReplaceWithExistedKeyPhrase
    }

    public sealed class UnlocalizedStringViewModel : ReactiveObject, IDisposable
    {
        private readonly ObservableAsPropertyHelper<string> _replacementPhrase;
        private readonly CompositeDisposable _cleanUp;

        public UnlocalizedStringViewModel(
            IUnlocalizedString unlocalizedString,
            MoveUnlocalizedStringOptions options,
            UnlocalizedStringsService service)
        {
            UnlocalizedString = unlocalizedString;
            Options = options;
            _cleanUp = new CompositeDisposable();

            this.WhenAnyValue(x => x.Options.Key)
                .CombineLatest(service.ConnectToKeyPhrases.QueryWhenChanged(),
                    (key, keyPhrases) => (key, keyPhrases))
                .Subscribe(tuple =>
                {
                    var (key, keyPhrases) = tuple;

                    if (string.IsNullOrEmpty(key) || !keyPhrases.Items.Any())
                    {
                        Key = null;
                        return;
                    }

                    var keyPhrase = keyPhrases.Items.FirstOrDefault(x => x.Key == key);
                    if (keyPhrase != null)
                        Key = keyPhrase;
                })
                .DisposeWith(_cleanUp);

            _replacementPhrase = this.WhenAnyValue(x => x.Options.LocalizationFile,
                    x => x.Key,
                    (file, key) => key == null ? unlocalizedString.Value
                        : key.GetLocalizations.FirstOrDefault(x => x.FilePath == file?.FilePath)?
                              .LocalizedValues[key.Key]
                          ?? unlocalizedString.Value)
                .ToProperty(this, nameof(ReplacementPhrase))
                .DisposeWith(_cleanUp);

            this.WhenAnyValue(x => x.Options.Key,
                    x => x.Options.LocalizationFile,
                    x => x.Key,
                    (key, localization, keyPhrases) => (key, localization, keyPhrases))
                .ObserveOnDispatcher()
                .Subscribe(tuple =>
                {
                    var (key, localization, keyPhrases) = tuple;
                    if (string.IsNullOrEmpty(key))
                        ResolveMode = UnlocalizedResolveMode.Undefined;
                    else if (keyPhrases == null)
                        ResolveMode = UnlocalizedResolveMode.ReplaceWithNewKeyPhrase;
                    else
                    {
                        var phrase =
                            keyPhrases.Phrases.FirstOrDefault(x =>
                                x.LocalizationFile.FilePath == localization.FilePath);
                        ResolveMode = phrase == null
                            ? UnlocalizedResolveMode.ReplaceWithExtendKeyPhrases
                            : UnlocalizedResolveMode.ReplaceWithExistedKeyPhrase;
                    }
                })
                .DisposeWith(_cleanUp);

            this.WhenAnyValue(x => x.ResolveMode)
                .Subscribe(x =>
                {
                    ResolveModeDescription = x switch
                    {
                        UnlocalizedResolveMode.Undefined => "Не определен",
                        UnlocalizedResolveMode.ReplaceWithExistedKeyPhrase => "Замена существующей фразой",
                        UnlocalizedResolveMode.ReplaceWithExtendKeyPhrases => "Добавление новой фразы",
                        UnlocalizedResolveMode.ReplaceWithNewKeyPhrase => "Создание новой пары ключ-фраза",
                        _ => throw new NotImplementedException(),
                    };
                })
                .DisposeWith(_cleanUp);

            LocalizeValue = ReactiveCommand.Create(() =>
                {
                    if(ResolveMode == UnlocalizedResolveMode.ReplaceWithNewKeyPhrase ||
                       ResolveMode == UnlocalizedResolveMode.ReplaceWithExtendKeyPhrases)
                        service.MoveStringToLocalizationFile(options);
                    else if (ResolveMode == UnlocalizedResolveMode.ReplaceWithExistedKeyPhrase)
                        service.ReplaceStringWithExistedKey(options);
                }, this.WhenAnyValue(x => x.ResolveMode)
                .Select(x => x != UnlocalizedResolveMode.Undefined)).DisposeWith(_cleanUp);
        }

        public IUnlocalizedString UnlocalizedString { get; }

        public MoveUnlocalizedStringOptions Options { get; }

        /// <summary>
        /// Итоговая фраза локализации: если пара ключ-фраза уже существует в выбранной
        /// локализации <see cref="MoveUnlocalizedStringOptions.LocalizationFile"/> – будет
        /// использована фраза из этой пары, иначе - значение нелокализированной
        /// строки <see cref="IUnlocalizedString.Value"/>.
        /// </summary>
        public string ReplacementPhrase => _replacementPhrase.Value;

        /// <summary>
        /// Ключ и все его фразы соответствующий введённому для замены ключу <see cref="MoveUnlocalizedStringOptions.Key"/>:
        /// если по введённому значение ключа не присутствует ни в одной локализации – значение
        /// свойства будет <see langword="null"/>.
        /// </summary>
        [Reactive]
        public KeyPhrase Key { get; private set; }

        /// <summary>
        /// Способ локализации.
        /// </summary>
        [Reactive]
        public UnlocalizedResolveMode ResolveMode { get; private set; }

        /// <summary>
        /// Описание текущего способа локализации <see cref="ResolveMode"/>.
        /// </summary>
        [Reactive]
        public string ResolveModeDescription { get; private set; }

        /// <summary>
        /// Локализирует нелокализированную строку.
        /// </summary>
        public ReactiveCommand<Unit, Unit> LocalizeValue { get; }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}