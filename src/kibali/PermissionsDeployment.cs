using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kibali
{
    public class PermissionsDeployment
    {
        [JsonPropertyName("permissionDeployments")]
        public Dictionary<string, List<ProvisioningInfo>> Deployments { get; set; } = new();

        public static PermissionsDeployment Load(string document)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<PermissionsDeployment>(document, options);
        }

        public static PermissionsDeployment Load(Stream documentStream)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<PermissionsDeployment>(documentStream, options);
        }

        public static PermissionsDeployment Load(JsonDocument doc)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<PermissionsDeployment>(doc.RootElement.GetRawText(), options);
        }

        public static PermissionsDeployment Load(JsonElement value)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<PermissionsDeployment>(value.GetRawText(), options);
        }
    }
}