using System;
using System.ComponentModel;

namespace Rack.Shared.BindableDecorators
{
    /// <summary>
    /// Обёртка, добавляющая в класс состояние "Выбран\не выбран" с помощью свойства <see cref="IsChecked" />.
    /// </summary>
    /// <typeparam name="T">Тип обёртываемого класса.</typeparam>
    public class Checked<T>: INotifyPropertyChanged where T : class
    {
        private readonly Action<bool, T> _onChanged;
        private bool _isChecked;

        /// <summary>
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="onChanged">bool isChecked, T instance</param>
        /// <param name="isChecked"></param>
        public Checked(T instance, Action<bool, T> onChanged = null, bool isChecked = false)
        {
            _onChanged = onChanged;
            Instance = instance;
            IsChecked = isChecked;
        }

        /// <summary>
        /// Объект обёртываемого класса.
        /// </summary>
        public T Instance { get; }

        /// <summary>
        /// true, если объект класса выбран.
        /// </summary>
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked.Equals(value)) return;
                _isChecked = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
                _onChanged?.Invoke(IsChecked, Instance);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}