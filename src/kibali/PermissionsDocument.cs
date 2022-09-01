using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kibali
{
    public class PermissionsDocument
    {
        private Dictionary<string, Permission> permissions = new Dictionary<string, Permission>();
        
        public Dictionary<string, Permission> Permissions { get => permissions; set => permissions = value;  }

        public async Task WriteAsync(FileStream outStream)
        {
            var writer = new Utf8JsonWriter(outStream, new JsonWriterOptions() { Indented = true });
            Write(writer);
            await writer.FlushAsync();
        }

        private void Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteString("$schema", "https://microsoftgraph.github.io/msgraph-metadata/graph-permissions-schema.json");
            writer.WritePropertyName("permissions");
            writer.WriteStartObject();
            foreach (var permissionPair in this.permissions)
            {
                writer.WritePropertyName(permissionPair.Key);
                permissionPair.Value.Write(writer);
            }
            writer.WriteEndObject();
            writer.WriteEndObject();
        }


        public static PermissionsDocument Load(string document) 
        {
            return Load(JsonDocument.Parse(document));
        }

        public static PermissionsDocument Load(Stream documentStream)
        {
            return Load(JsonDocument.Parse(documentStream));
        }
        public static PermissionsDocument Load(JsonDocument doc)
        {
            return Load(doc.RootElement);
        }

        public static PermissionsDocument LoadAndMerge(string documentPath)
        {
            var mergedDoc = new PermissionsDocument();
            foreach(var permissionsFile in Directory.EnumerateFiles(documentPath, "*.json"))
            {
                var doc = Load(new FileStream(permissionsFile, FileMode.Open));
                mergedDoc.Permissions = mergedDoc.Permissions.Concat(doc.Permissions).ToDictionary(x => x.Key, x => x.Value);
            }
            return mergedDoc;
        }

        public static PermissionsDocument Load(JsonElement value)
        {
            var permissionsDocument = new PermissionsDocument();

            ParsingHelpers.ParseMap(value, permissionsDocument, handlers);

            return permissionsDocument;
        }

        private static readonly FixedFieldMap<PermissionsDocument> handlers = new()
        {
            { "permissions", (d,v) => { d.Permissions = ParsingHelpers.GetMap(v,Permission.Load);  } },
            { "$schema", (d,v) => {  } }
        };
        
    }

        

    public enum SchemeType
    {
        DelegatedWork,
        DelegatedPersonal,
        Application,
        ResourceSpecificConsent
    }

}
