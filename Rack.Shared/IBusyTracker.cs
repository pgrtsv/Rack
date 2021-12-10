using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Rack.Shared
{
    /// <summary>
    /// Объект, отслеживающий блокирующие операции.
    /// </summary>
    [Obsolete]
    public interface IBusyTracker : INotifyPropertyChanged
    {
        /// <summary>
        /// true, если выполняется блокирующая операция.
        /// </summary>
        bool IsBusy { get; }

        /// <summary>
        /// Статус-пояснение природы выполняющейся операции.
        /// </summary>
        string Status { get; }

        /// <summary>
        /// Выполняет блокирующую операцию.
        /// </summary>
        /// <param name="status">
        /// Статус-пояснение. Рекомендуется использовать глагол 1-го лица настоящего времени,
        /// заканчивать предложение многоточием.
        /// </param>
        /// <param name="action">Блокирующая операция.</param>
        void Do(string status, Action action);

        Task DoAsync(string status, Func<Task> action);
    }
}