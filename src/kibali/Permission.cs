using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Kibali
{
    public class Permission
    {
        public string Note { get; set; }
        public bool Implicit { get; set; } = false;
        public bool IsHidden { get; set; } = false;
        public string OwnerEmail { get; set; }
        public string PrivilegeLevel { get; set; }
        public List<string> RequiredEnvironments { get; set; } = new List<string>();

        public Dictionary<string, Scheme> Schemes { get; set; } = new Dictionary<string, Scheme>();
        public List<PathSet> PathSets { get; set; } = new List<PathSet>();

        public ProvisioningInfo ProvisioningInfo { get; set; } = new();

        public void Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            if (!String.IsNullOrWhiteSpace(Note)) writer.WriteString("note", Note);
            if (Implicit == true) writer.WriteBoolean("implicit", Implicit);
            if (IsHidden == true) writer.WriteBoolean("isHidden", IsHidden);
            if (!String.IsNullOrWhiteSpace(OwnerEmail)) writer.WriteString("ownerEmail", OwnerEmail);

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

            writer.WritePropertyName("provisioningInfo");
            ProvisioningInfo.Write(writer);

            writer.WriteEndObject();
        }

        internal static Permission Load(JsonElement value)
        {
            var permission = new Permission();
            ParsingHelpers.ParseMap(value, permission, handlers);
            return permission;
        }

        private static FixedFieldMap<Permission> handlers = new()
        {
            { "note", (o,v) => {o.Note = v.GetString();  } },
            { "privilegeLevel", (o,v) => {o.PrivilegeLevel= v.GetString();  } },
            { "ownerEmail", (o,v) => {o.OwnerEmail= v.GetString();  } },
            { "requiredEnvironments", (o,v) => {o.RequiredEnvironments= ParsingHelpers.GetListOfString(v); } },
            { "implicit", (o,v) => {o.Implicit = v.GetBoolean();  } },
            { "isHidden", (o,v) => {o.IsHidden = v.GetBoolean();  } },
            { "pathSets", (o,v) => {o.PathSets = ParsingHelpers.GetList(v, PathSet.Load);  } },
            { "schemes", (o,v) => {o.Schemes = ParsingHelpers.GetMap(v, Scheme.Load);  } },
        };
    }

}
