using System;
using System.Collections.Generic;
using System.Security;
using System.Text.Json;

namespace Kibali
{
    public class PathSet
    {
        public HashSet<string> SchemeKeys { get; set; } = new HashSet<string>();
        public HashSet<string> Methods { get; set; } = new HashSet<string>();
        public string AlsoRequires { get; set; }
        public List<string> ExcludedProperties { get; set; } = new List<string>();
        public List<string> IncludedProperties { get; set; } = new List<string>();
        

        public Dictionary<string, string> Paths
        {
            get
            {
                if (paths == null)
                {
                    paths = new Dictionary<string, string>();
                };
                return paths;
            }
            set { paths = value; }
        }
        private Dictionary<string, string> paths;

        public void Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            if (!String.IsNullOrEmpty(AlsoRequires))
            {
                writer.WriteString("alsoRequires", AlsoRequires);
            }

            if (ExcludedProperties.Count > 0)
            {
                writer.WritePropertyName("excludedProperties");
                writer.WriteStartArray();
                foreach (var prop in ExcludedProperties)
                {
                    writer.WriteStringValue(prop);
                }
                writer.WriteEndArray();
            }

            if (IncludedProperties.Count > 0)
            {
                writer.WritePropertyName("includedProperties");
                writer.WriteStartArray();
                foreach (var prop in IncludedProperties)
                {
                    writer.WriteStringValue(prop);
                }
                writer.WriteEndArray();
            }



            writer.WritePropertyName("schemeKeys");
            writer.WriteStartArray();
            foreach (var scheme in SchemeKeys)
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
                writer.WriteStringValue(path.Value);
            }
            writer.WriteEndObject();


            writer.WriteEndObject();
        }

        public static PathSet Load(JsonElement value)
        {
            var pathSet = new PathSet();
            ParsingHelpers.ParseMap(value, pathSet, handlers);
            return pathSet;
        }

        private static FixedFieldMap<PathSet> handlers = new()
        {
            { "alsoRequires", (o,v) => {o.AlsoRequires = v.GetString();  } },
            { "methods", (o,v) => {o.Methods = ParsingHelpers.GetHashSetOfString(v);  } },
            { "schemeKeys", (o,v) => {o.SchemeKeys = ParsingHelpers.GetHashSetOfString(v);  } },
            { "paths", (o,v) => {o.Paths = ParsingHelpers.GetMap(v, x => x.ToString()); } },
            { "includedProperties", (o,v) => {o.IncludedProperties = ParsingHelpers.GetListOfString(v);  } },
            { "excludedProperties", (o,v) => {o.ExcludedProperties = ParsingHelpers.GetListOfString(v);  } },
        };
    }

}
