using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiPermissions
{
    public class PermissionsDocument
    {
        private Dictionary<string,Permission> permissions = new Dictionary<string,Permission>();

        public Dictionary<string,Permission> Permissions { get => permissions; }

        public async Task WriteAsync(FileStream outStream)
        {
            var writer = new Utf8JsonWriter(outStream, new JsonWriterOptions() { Indented = true });
            Write(writer);
            await writer.FlushAsync();
        }

        private void Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
                writer.WriteString("$schema", "../../../../permissionsSchema.json");
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
    }

    public enum SchemeType
    {
        DelegatedWork,
        DelegatedPersonal,
        Application,
        ResourceSpecificConsent
    }

}
