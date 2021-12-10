using System;
using System.Collections.Generic;

namespace Rack.Shared.INPC
{
    [Obsolete("Маршалинг событий INPC неактуален благодаря Rx.")]
    public class SchemaElement
    {
        public SchemaElement(string observableName, bool isCollection, SchemaElement parent)
        {
            ObservableName = observableName;
            IsCollection = isCollection;
            Parent = parent;
        }

        public string ObservableName { get; }
        public bool IsCollection { get; }
        public SchemaElement Parent { get; }
        public List<SchemaElement> Children { get; } = new List<SchemaElement>();
    }
}