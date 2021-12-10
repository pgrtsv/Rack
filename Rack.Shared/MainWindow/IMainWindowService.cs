using System;

namespace Rack.Shared.MainWindow
{
    public interface IMainWindowService
    {
        /// <summary>
        /// Посылает пользователю сообщение в UI.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        void SendMessage(Message message);

        IObservable<Message> Message { get; }

        /// <summary>
        /// Изменяет заголовок главного окна.
        /// </summary>
        /// <param name="header">Новый заголовок.</param>
        /// <param name="moduleName">Универсальное название модуля.</param>
        void ChangeHeader(string header, string moduleName = "");

        IObservable<string> Header { get; }

    }
}