using System.Collections.Generic;
using System.Text.Json;

namespace OAuthPermissions
{
    public class PathSet
    {
        public HashSet<string> Methods { get; set; } = new HashSet<string>();
        public HashSet<string> Schemes { get; set; } = new HashSet<string>();
        public Dictionary<string,Path> Paths { get; set; } = new Dictionary<string, Path>();

        public void Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("schemes");
            writer.WriteStartArray();
            foreach (var scheme in Schemes)
            {
                writer.WriteStringValue(scheme);
            }
            writer.WriteEndArray();


            writer.WritePropertyName("methods");
            writer.WriteStartArray();
            foreach (var method in Methods)
            {
                writer.WriteStringValue(method);
            }
            writer.WriteEndArray();

            writer.WritePropertyName("paths");
            writer.WriteStartObject();
            foreach (var path in Paths)
            {
                writer.WritePropertyName(path.Key);
                path.Value.Write(writer);
            }
            writer.WriteEndObject();


            writer.WriteEndObject();
        }

    }

}
