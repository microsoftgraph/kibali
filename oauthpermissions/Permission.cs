using System;
using System.Collections.Generic;
using System.Text.Json;

namespace OAuthPermissions
{
    public class Permission
    {
        public string AlsoRequires { get; set; }
        public Dictionary<SchemeType, Scheme> Schemes { get; set; } = new Dictionary<SchemeType, Scheme>();
        public List<PathSet> PathSets { get; set; } = new List<PathSet>();

        public void Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            if (!String.IsNullOrEmpty(AlsoRequires))
            {
                writer.WriteString("alsoRequires", AlsoRequires);
            }

            writer.WritePropertyName("schemes");
            writer.WriteStartObject();
            foreach (var scheme in Schemes)
            {
                writer.WritePropertyName(scheme.Key.ToString());
                scheme.Value.Write(writer);
            }
            writer.WriteEndObject();

            writer.WritePropertyName("pathSets");
            writer.WriteStartArray();
            foreach (var pathSet in PathSets)
            {
                pathSet.Write(writer);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }

}
