using System;
using System.Collections.Generic;

namespace Rack.Navigation
{
    public interface IDialogViewModel
    {
        /// <summary>
        /// true, если пользователь может закрыть диалоговое окно с помощью стандартной
        /// кнопки.
        /// </summary>
        bool CanClose { get; }

        /// <summary>
        /// Событие происходит, когда диалоговое окно закрывается "из кода".
        /// </summary>
        event Action<IReadOnlyDictionary<string, object>> RequestClose;

        /// <summary>
        /// Метод вызывается при открытии диалогового окна.
        /// </summary>
        /// <param name="parameters"></param>
        void OnDialogOpened(IReadOnlyDictionary<string, object> parameters);

        /// <summary>
        /// Метод вызывается при закрытии диалогового кона с помощью стандартной кнопки. Если <see cref="CanClose" /> всегда false,
        /// метод никогда не вызывается.
        /// </summary>
        /// <returns></returns>
        IReadOnlyDictionary<string, object> OnDialogClosed();

        /// <summary>
        /// Заголовок диалогового окна.
        /// </summary>
        string Title { get; }
    }
}