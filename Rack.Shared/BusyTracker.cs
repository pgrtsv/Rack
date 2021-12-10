using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Rack.Shared
{
    /// <inheritdoc />
    [Obsolete]
    public sealed class BusyTracker : IBusyTracker
    {
        public HashSet<Guid> BusyTokens { get; } = new HashSet<Guid>();

        /// <inheritdoc />
        public bool IsBusy => BusyTokens.Any();

        /// <inheritdoc />
        public string Status { get; private set; }

        /// <inheritdoc />
        public void Do(string status, Action action)
        {
            Status = status;
            if (IsBusy)
                throw new NotImplementedException(
                    "BusyTracker не может использоваться для выполняющихся одновременно операций.");
            var guid = Guid.NewGuid();
            BusyTokens.Add(guid);
            OnPropertyChanged(nameof(IsBusy));
            try
            {
                action?.Invoke();
            }
            finally
            {
                BusyTokens.Remove(guid);
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        public async Task DoAsync(string status, Func<Task> action)
        {
            Status = status;
            var guid = Guid.NewGuid();
            BusyTokens.Add(guid);
            OnPropertyChanged(nameof(IsBusy));
            try
            {
                await action?.Invoke();
            }
            finally
            {
                BusyTokens.Remove(guid);
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}