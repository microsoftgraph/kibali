using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ApiPermissions
{
    public class Permission
    {
        public string Note { get; set; }
        public bool Implicit { get; set; } = false;
        public bool IsHidden { get; set; } = false;
        public string OwnerEmail { get; set; }
        public string PrivilegeLevel { get; set; }
        public List<string> RequiredEnvironments { get; set; } = new List<string>();

        public Dictionary<SchemeType, Scheme> Schemes { get; set; } = new Dictionary<SchemeType, Scheme>();
        public List<PathSet> PathSets { get; set; } = new List<PathSet>();

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
            writer.WriteEndObject();
        }
    }

}
