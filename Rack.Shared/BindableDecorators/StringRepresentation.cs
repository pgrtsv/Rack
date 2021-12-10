namespace Rack.Shared.BindableDecorators
{
    public sealed class StringRepresentation<T> where T : class
    {
        private readonly string _nullText;

        public StringRepresentation(T instance, string nullText = null)
        {
            Instance = instance;
            _nullText = nullText;
        }

        public T Instance { get; }

        public override string ToString()
        {
            return Instance == null ? _nullText : Instance.ToString();
        }
    }
}