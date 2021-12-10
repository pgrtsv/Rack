using System;
using System.Collections.Generic;
using System.Linq;
using Rack.Shared.Json.Abstraction;

namespace Rack.Shared.Json
{
    public class ResolverConfiguration<T> : IResolverConfiguration<T> where T : class
    {
        private readonly List<string> _globalIgnoredProperties = new List<string>();

        private readonly Dictionary<Type, List<(string, string)>> _renamedProperties =
            new Dictionary<Type, List<(string, string)>>();

        private readonly Dictionary<Type, List<string>> _typeIgnoredProperties
            = new Dictionary<Type, List<string>>();

        public IReadOnlyCollection<string> GlobalIgnoredProperties => _globalIgnoredProperties;

        public IReadOnlyDictionary<Type, IReadOnlyCollection<string>> TypeIgnoredProperties
        {
            get
            {
                var ret = new Dictionary<Type, IReadOnlyCollection<string>>();
                foreach (var pair in _typeIgnoredProperties)
                    ret[pair.Key] = pair.Value;
                return ret;
            }
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<Type, IReadOnlyCollection<(string, string)>> RenamedProperties
        {
            get
            {
                var ret = new Dictionary<Type, IReadOnlyCollection<(string, string)>>();
                foreach (var pair in _renamedProperties)
                    ret[pair.Key] = pair.Value;
                return ret;
            }
        }

        /// <inheritdoc />
        public IResolverConfiguration<T> IgnoreProperty(string propertyName)
        {
            var key = typeof(T);
            if (!_typeIgnoredProperties.ContainsKey(key))
                _typeIgnoredProperties.Add(key, new List<string> {propertyName});
            else
                _typeIgnoredProperties[key].Add(propertyName);
            return this;
        }

        /// <inheritdoc />
        public IResolverConfiguration<T> IgnorePropertyForAll(string propertyName)
        {
            _globalIgnoredProperties.Add(propertyName);
            return this;
        }

        /// <inheritdoc />
        public IResolverConfiguration<T> RenameProperty(string propertyName, string jsonFileName)
        {
            var key = typeof(T);
            if (!_renamedProperties.ContainsKey(key))
                _renamedProperties.Add(key, new List<(string, string)> {(propertyName, jsonFileName)});
            else
                _renamedProperties[key].Add((propertyName, jsonFileName));
            return this;
        }

        /// <inheritdoc />
        public IResolverConfiguration<T> Merge<TAnother>(IResolverConfiguration<TAnother> configuration)
            where TAnother : class
        {
            _globalIgnoredProperties.AddRange(configuration.GlobalIgnoredProperties.Except(_globalIgnoredProperties));

            foreach (var pair in configuration.TypeIgnoredProperties)
                if (!_typeIgnoredProperties.ContainsKey(pair.Key))
                    _typeIgnoredProperties.Add(pair.Key, pair.Value.ToList());
                else
                    _typeIgnoredProperties[pair.Key].AddRange(pair.Value.Except(_typeIgnoredProperties[pair.Key]));

            foreach (var pair in configuration.RenamedProperties)
                if (!_renamedProperties.ContainsKey(pair.Key))
                    _renamedProperties.Add(pair.Key, pair.Value.ToList());
                else
                    _renamedProperties[pair.Key].AddRange(pair.Value.Except(_renamedProperties[pair.Key]));

            return this;
        }
    }
}