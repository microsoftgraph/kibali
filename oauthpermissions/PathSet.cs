using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ApiPermissions
{
    public class PathSet
    {
        public HashSet<string> Schemes { get; set; } = new HashSet<string>();
        public HashSet<string> Methods { get; set; } = new HashSet<string>();
        public string AlsoRequires { get; set; }
        public List<String> ExcludedProperties { get; set; } = new List<String>();
        public List<String> IncludedProperties { get; set; } = new List<String>();


        public Dictionary<string, PathConstraints> Paths
        {
            get
            {
                if (paths == null)
                {
                    paths = new Dictionary<string, PathConstraints>();
                };
                return paths;
            }
            set { paths = value; }
        }
        private Dictionary<string, PathConstraints> paths;

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
