using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Rack.Shared.Json;

namespace Rack.Shared.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        public Dictionary<Type, Dictionary<Version, Action<JObject>>> MigrationsForTypes =
            new Dictionary<Type, Dictionary<Version, Action<JObject>>>();

        private readonly Dictionary<Type, Func<IConfiguration>> _defaultConfigurationBuilder =
            new Dictionary<Type, Func<IConfiguration>>();

        private readonly List<IConfiguration> _loadedConfigurations = new List<IConfiguration>();

        private readonly IContractResolver _versionResolver = new ResolverConfiguration<Version>()
            .IgnoreProperty(x => x.Build)
            .IgnoreProperty(x => x.MajorRevision)
            .IgnoreProperty(x => x.MinorRevision)
            .IgnoreProperty(x => x.Revision)
            .ToResolver();

        public string Folder { get; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Rack", "config");

        public IConfigurationService RegisterDefaultConfiguration<T>(Func<T> builder) where T : class, IConfiguration
        {
            _defaultConfigurationBuilder.Add(typeof(T), builder.Invoke);
            return this;
        }

        public IConfigurationService AddMigration<T>(Version version, Action<JObject> action)
            where T : class, IConfiguration
        {
            if (!MigrationsForTypes.ContainsKey(typeof(T)))
            {
                MigrationsForTypes.Add(typeof(T), new Dictionary<Version, Action<JObject>> {{version, action}});
                return this;
            }

            MigrationsForTypes[typeof(T)].Add(version, action);
            return this;
        }

        public T GetConfiguration<T>() where T : class, IConfiguration
        {
            var configuration = (T) _loadedConfigurations.FirstOrDefault(x => x is T);
            if (configuration != null)
                return configuration;
            var configurationFilePath = GetConfigFullPath<T>();
            if (!File.Exists(configurationFilePath))
            {
                configuration = (T) _defaultConfigurationBuilder[typeof(T)].Invoke();
                _loadedConfigurations.Add(configuration);
                return configuration;
            }
            var parsedObject = JObject.Parse(File.ReadAllText(configurationFilePath));
            if (MigrationsForTypes.ContainsKey(typeof(T)))
            {
                var migrations = MigrationsForTypes[typeof(T)];
                var currentVersion = parsedObject.GetValue("Version")
                    .ToObject<Version>(new JsonSerializer {ContractResolver = _versionResolver});
                if (migrations.All(x => x.Key.Major != currentVersion.Major || x.Key.Minor != currentVersion.Minor))
                    return parsedObject.ToObject<T>(new JsonSerializer());
                foreach (var migration in migrations
                    .Where(x => x.Key >= currentVersion)
                    .OrderBy(x => x.Key))
                    migration.Value.Invoke(parsedObject);
            }

            configuration = parsedObject.ToObject<T>();
            _loadedConfigurations.Add(configuration);
            return configuration;
        }

        public void SaveConfiguration<T>(T configuration) where T : class, IConfiguration
        {
            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);
            File.WriteAllText(GetConfigFullPath<T>(), JsonConvert.SerializeObject(configuration));
            if (!_loadedConfigurations.Any(x => x is T))
                _loadedConfigurations.Add(configuration);
        }

        public string GetConfigFullPath<T>() where T : class, IConfiguration
        {
            return Path.Combine(Folder, GetConfigFileName<T>());
        }

        public string GetConfigFileName<T>() where T : class, IConfiguration
        {
            var type = typeof(T);
            return $"{type.Namespace}.{type.Name}.json";
        }

        public bool FolderExists()
        {
            return Directory.Exists(Folder);
        }
    }
}