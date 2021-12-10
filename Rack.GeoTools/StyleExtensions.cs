using System.IO;
using Newtonsoft.Json;

namespace Rack.GeoTools
{
    public static class StyleExtensions
    {
        public static void SaveJson(this Style style, string path)
        {
            var serializer = new JsonSerializer {Formatting = Formatting.Indented};
            using var stream = File.CreateText(path);
            serializer.Serialize(stream, style);
            stream.Close();
        }

        public static Style LoadJson(string path)
        {
            var serializer = new JsonSerializer();
            using var stream = File.OpenText(path);
            return serializer.Deserialize<Style>(new JsonTextReader(stream));
        }
    }
}