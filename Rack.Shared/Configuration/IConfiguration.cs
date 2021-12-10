using System;

namespace Rack.Shared.Configuration
{
    public interface IConfiguration
    {
        Version Version { get; }
    }
}