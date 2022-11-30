using System;
using System.Linq;
using System.Text.Json;

namespace Kibali
{
    public class AcceptableClaim
    {
        public AcceptableClaim(string permission, string alsoRequires, bool least)
        {
            this.Permission = permission;
            this.AlsoRequires = alsoRequires;
            this.Least = least;
        }
        public string Permission { get; }
        public string AlsoRequires { get; }
        public bool Least { get; }

        internal bool IsAuthorized(string[] providedPermissions)
        {
            return providedPermissions.Contains(this.Permission);  //TODO: add support for alsoRequires
        }

        internal void Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("permission");
            writer.WriteStringValue(this.Permission);
            if (!String.IsNullOrEmpty(this.AlsoRequires))
            {
                writer.WritePropertyName("alsoRequires");
                writer.WriteStringValue(this.AlsoRequires);
            }
            writer.WriteEndObject();
        }
    }
}
