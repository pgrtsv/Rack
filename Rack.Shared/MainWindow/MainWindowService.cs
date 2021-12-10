using System;
using System.Reactive.Subjects;
using Rack.Localization;

namespace Rack.Shared.MainWindow
{
    public sealed class MainWindowService : IMainWindowService, IDisposable
    {
        private readonly ILocalizationService _localizationService;
        private readonly Subject<string> _header;
        private readonly Subject<Message> _message;

        public MainWindowService(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            _header = new Subject<string>();
            _message = new Subject<Message>();
        }

        public void Dispose()
        {
            _header.Dispose();
            _message.Dispose();
        }

        public void SendMessage(Message message)
        {
            _message.OnNext(message);
        }

        public IObservable<Message> Message => _message;

        public void ChangeHeader(string header, string moduleName = "")
        {
            if (string.IsNullOrWhiteSpace(header) && string.IsNullOrWhiteSpace(moduleName))
            {
                _header.OnNext(string.Empty);
                return;
            }

            if (string.IsNullOrWhiteSpace(moduleName))
            {
                _header.OnNext(header);
                return;
            }

            var localizedModuleName = _localizationService.FromAnyLocalization(moduleName);

            if (string.IsNullOrWhiteSpace(header))
            {
                _header.OnNext(localizedModuleName);
                return;
            }

            _header.OnNext(localizedModuleName + " - " + header);
        }

        public IObservable<string> Header => _header;
    }
}