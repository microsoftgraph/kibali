using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kibali
{
    public class ProvisioningInfo
    {
        public bool IsHidden { get; set; }

        public List<string> RequiredEnvironments { get; set; } = new();

        public string ResourceAppId { get; set; }

        public string OwnerSecurityGroup { get; set; }


        public void Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            
            if (IsHidden) writer.WriteBoolean("isHidden", IsHidden);

            if (RequiredEnvironments.Any())
            {
                writer.WritePropertyName("requiredEnvironments");
                writer.WriteStartArray();
                foreach (var environment in RequiredEnvironments)
                {
                    writer.WriteStringValue(environment);
                }
                writer.WriteEndArray();
            }

            if (!String.IsNullOrWhiteSpace(ResourceAppId)) writer.WriteString("resourceAppId", ResourceAppId);
            if (!String.IsNullOrWhiteSpace(OwnerSecurityGroup)) writer.WriteString("ownerSecurityGroup", OwnerSecurityGroup);

            writer.WriteEndObject();
        }

        public static ProvisioningInfo Load(JsonElement value)
        {
            var provisioningInfo = new ProvisioningInfo();
            ParsingHelpers.ParseMap(value, provisioningInfo, handlers);
            return provisioningInfo;
        }

        private static FixedFieldMap<ProvisioningInfo> handlers = new()
        {
            { "isHidden", (o,v) => {o.IsHidden = v.GetBoolean();  } },
            { "requiredEnvironments", (o,v) => {o.RequiredEnvironments= ParsingHelpers.GetListOfString(v); } },
            { "resourceAppId", (o,v) => {o.ResourceAppId= v.GetString();  } },
            { "ownerSecurityGroup", (o,v) => {o.OwnerSecurityGroup= v.GetString();  } },
        };
    }
}
