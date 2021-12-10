using ReactiveUI;

namespace Rack.LogViewer
{
    public sealed class Level : ReactiveObject
    {
        private bool _isSelected = true;

        public Level(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        private bool Equals(Level other)
        {
            return Value == other.Value;
        }

        private bool Equals(string other)
        {
            return Value == other;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj)
                   || obj is Level other && Equals(other)
                   || obj is string text && Equals(text);
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }
    }
}