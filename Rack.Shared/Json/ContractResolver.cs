using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rack.Shared.Json.Abstraction;

namespace Rack.Shared.Json
{
    public class ContractResolver<T> : DefaultContractResolver where T : class
    {
        private readonly IResolverConfiguration<T> _configuration;

        public ContractResolver(IResolverConfiguration<T> configuration)
        {
            _configuration = configuration;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            property.IgnoreIfNeeded(_configuration);
            property.RenameIfNeeded(_configuration);

            return property;
        }
    }
}