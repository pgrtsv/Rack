using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using MaterialDesignThemes.Wpf;
using Rack.Localization;
using Rack.Navigation;
using Rack.Shared.MainWindow;
using Rack.Shared.Updates;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Rack.ViewModels
{
    /// <summary>
    /// Модель представления главного окна приложения.
    /// </summary>
    public sealed class MainWindowViewModel : ReactiveObject, IScreen, IActivatableViewModel
    {
        private readonly ObservableAsPropertyHelper<bool> _isBigMessageDialogOpen;
        private readonly ObservableAsPropertyHelper<bool> _isSmallMessageSnackbarActive;
        private readonly ObservableAsPropertyHelper<bool> _isUpdating;
        private readonly ObservableAsPropertyHelper<string> _title;

        public MainWindowViewModel(
            IMainWindowService mainWindowService,
            IDialogService dialogService,
            IUpdateService updateService,
            ILocalizationService localizationService,
            Lazy<LoginViewModel> loginViewModel,
            Lazy<ChangelogsViewModel> changelogsViewModel,
            Lazy<MainMenuViewModel> mainMenuViewModel)
        {
            _localization = localizationService.CurrentLanguage
                .Select(x => localizationService.GetLocalization(App.Name))
                .ToProperty(this, nameof(Localization), deferSubscription: true);

            Activator = new ViewModelActivator();

            MainMenuViewModel = mainMenuViewModel.Value;

            mainWindowService.Message
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(message =>
                {
                    switch (message.RepresentationType)
                    {
                        case RepresentationType.SmallMessage:
                            AddSmallMessageCommand.Execute(message).Subscribe();
                            break;
                        case RepresentationType.BigMessage:
                            AddBigMessageCommand.Execute(message).Subscribe();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                })
                .Subscribe();

            var updateProgress = new Progress<int>(x => UpdateProgress = x);

            _isBigMessageDialogOpen = this.WhenAnyValue(
                    x => x.CurrentBigMessage,
                    message => !message.IsEmpty())
                .ToProperty(this, nameof(IsBigMessageDialogOpen));
            _isSmallMessageSnackbarActive = this.WhenAnyValue(
                    x => x.CurrentSmallMessage,
                    message => !message.IsEmpty())
                .ToProperty(this, nameof(IsSmallMessageSnackbarActive));
            _title = mainWindowService.Header
                .StartWith(string.Empty)
                .Select(x => string.IsNullOrEmpty(x) ? "Rack" : "Rack - " + x)
                .ToProperty(this, nameof(Title));

            AddSmallMessageCommand = ReactiveCommand.Create<Message>(message =>
            {
                SmallMessages.Enqueue(message);
                Messages.Insert(0, message);
                CurrentSmallMessage = message;
            });

            AddBigMessageCommand = ReactiveCommand.Create<Message>(message =>
            {
                BigMessages.Push(message);
                Messages.Insert(0, message);
                CurrentBigMessage = message;
            });

            SetCurrentBigMessageAsReadCommand = ReactiveCommand.Create(() =>
            {
                CurrentBigMessage.InteractionCallback?.Invoke();
                if (BigMessages.Count == 0) return;
                BigMessages.Pop();
                if (BigMessages.Count == 0)
                {
                    CurrentBigMessage = default;
                    return;
                }

                CurrentBigMessage = BigMessages.Peek();
            });

            ShowChangelogs = ReactiveCommand.CreateFromObservable(() =>
                Router.NavigateAndReset.Execute(changelogsViewModel.Value)
                    .Select(_ => Unit.Default));

            ShowAppSettings = ReactiveCommand.CreateFromTask(dialogService.ShowAppSettingsAsync);

            ShowAppHelp = ReactiveCommand.CreateFromTask(dialogService.ShowHelpAsync);

            Authenticate = ReactiveCommand.Create(() =>
            {
                Router.NavigateAndReset.Execute(loginViewModel.Value).Subscribe();
            });

            ShowMessagesJournal = ReactiveCommand.Create(() => { IsMessagesJournalOpen = true; });

            CopyMessagesToClipboard = ReactiveCommand.Create<IEnumerable>(messages =>
            {
                if (messages == null) return;
                var stringBuilder = new StringBuilder();
                foreach (Message message in messages)
                    stringBuilder.AppendLine(message.MessageType.ToString() +
                                             message.CreationDateTime.ToString(
                                                 " dd.MM.yyyy HH:mm:ss: ") +
                                             message.Text);
                Clipboard.SetText(stringBuilder.ToString());
            });

            CopyCurrentBigMessageToClipboard = ReactiveCommand.Create(() =>
            {
                if (CurrentBigMessage.IsEmpty()) return;
                Clipboard.SetText(CurrentBigMessage.Text);
                mainWindowService.SendMessage(new Message(Localization["MessageCopiedToBuffer"]));
            });

            CloseMessagesJournalCommand = ReactiveCommand.Create(() =>
            {
                IsMessagesJournalOpen = false;
            });

            CheckForUpdates = ReactiveCommand.CreateFromObservable(updateService.CheckForUpdates);
            Update = ReactiveCommand.CreateFromObservable(() =>
                updateService.Update(updateProgress));
            CheckForUpdates.Subscribe(hasNewUpdates =>
            {
                if (!hasNewUpdates)
                    mainWindowService.SendMessage(new Message(Localization["ApplicationUpToDate"]));
            });

            _isUpdating = Update.IsExecuting.ToProperty(this, nameof(IsUpdating));

            if (updateService.CanCheckForUpdates.FirstAsync().Wait())
                CheckForUpdates.Execute().Subscribe(x =>
                {
                    if (x) Update.Execute().Subscribe();
                });
            Update.ThrownExceptions.Subscribe(exception =>
            {
                mainWindowService.SendMessage(new Message(Localization["UpdateFailed"],
                    MessageType.Error));
            });
            Update.Subscribe(_ =>
            {
                mainWindowService.SendMessage(new Message(
                    Localization["ApplicationUpdated"],
                    MessageType.Notification,
                    RepresentationType.BigMessage));
            });

            Router = new RoutingState();
        }

        [Reactive]
        public bool IsMessagesJournalOpen { get; set; }

        [Reactive]
        public int UpdateProgress { get; private set; }

        public MainMenuViewModel MainMenuViewModel { get; }

        /// <summary>
        /// Заголовок окна.
        /// </summary>
        public string Title => _title.Value;

        /// <summary>
        /// Стэк непрочитанных уведомлений для пользователя.
        /// </summary>
        public SnackbarMessageQueue SmallMessages { get; } =
            new SnackbarMessageQueue(TimeSpan.FromSeconds(5));

        public ObservableCollection<Message> Messages { get; } =
            new ObservableCollection<Message>();

        /// <summary>
        /// Очередь непрочитанных ошибок и предупреждения для пользователя.
        /// </summary>
        public Stack<Message> BigMessages { get; set; } = new Stack<Message>();

        /// <summary>
        /// Текущая непрочитанная ошибка или предупреждение.
        /// </summary>
        [Reactive]
        public Message CurrentBigMessage { get; private set; }

        /// <summary>
        /// Текущее непрочитанное уведомление.
        /// </summary>
        [Reactive]
        public Message CurrentSmallMessage { get; private set; }

        public bool IsUpdating => _isUpdating.Value;

        /// <summary>
        /// true, если открыто диалоговое окно с текстом ошибки или предупреждения.
        /// </summary>
        public bool IsBigMessageDialogOpen
        {
            get => _isBigMessageDialogOpen.Value;
            set
            {
                if (value == false) SetCurrentBigMessageAsReadCommand.Execute().Subscribe();
            }
        }

        /// <summary>
        /// true, если показывается снэкбар с уведомлениями.
        /// </summary>
        public bool IsSmallMessageSnackbarActive => _isSmallMessageSnackbarActive.Value;

        private readonly ObservableAsPropertyHelper<ILocalization> _localization;

        public ILocalization Localization => _localization.Value;

        #region Команды

        /// <summary>
        /// Отмечает текущую ошибку или предупреждение как прочитанное.
        /// </summary>
        public ReactiveCommand<Unit, Unit> SetCurrentBigMessageAsReadCommand { get; }

        public ReactiveCommand<Unit, Unit> CloseMessagesJournalCommand { get; }
        public ReactiveCommand<Unit, bool> CheckForUpdates { get; }
        public ReactiveCommand<Unit, Unit> Update { get; }
        public ReactiveCommand<Unit, Unit> ShowChangelogs { get; }
        public ReactiveCommand<Unit, Unit> ShowAppSettings { get; }
        public ReactiveCommand<Unit, Unit> ShowAppHelp { get; }
        public ReactiveCommand<Message, Unit> AddBigMessageCommand { get; }
        public ReactiveCommand<Message, Unit> AddSmallMessageCommand { get; }
        public ReactiveCommand<Unit, Unit> Authenticate { get; }
        public ReactiveCommand<Unit, Unit> ShowMessagesJournal { get; }
        public ReactiveCommand<IEnumerable, Unit> CopyMessagesToClipboard { get; }
        public ReactiveCommand<Unit, Unit> CopyCurrentBigMessageToClipboard { get; }

        #endregion

        public RoutingState Router { get; }

        /// <inheritdoc />
        public ViewModelActivator Activator { get; }
    }
}