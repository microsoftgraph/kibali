using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ApiPermissions
{
    public class PathConstraints
    {
        private List<string> leastPrivilegedPermission;

        public List<string> LeastPrivilegedPermission
        {
            get
            {
                if (leastPrivilegedPermission == null)
                {
                    leastPrivilegedPermission = new List<string>();
                }
                return leastPrivilegedPermission;
            }
            set => leastPrivilegedPermission = value;
        }

        public void Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            if (LeastPrivilegedPermission.Count > 0)
            {
                writer.WritePropertyName("leastPrivilegedPermission");
                writer.WriteStartArray();
                foreach (var perm in LeastPrivilegedPermission)
                {
                    writer.WriteStringValue(perm);
                }
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }

        public static PathConstraints Load(JsonElement value)
        {
            if (value.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            var pathConstraints = new PathConstraints();
            ParsingHelpers.ParseMap(value, pathConstraints, handlers);
            return pathConstraints;
        }

        private static FixedFieldMap<PathConstraints> handlers = new()
        {
            { "leastPrivilegedPermission", (o,v) => {o.LeastPrivilegedPermission = ParsingHelpers.GetListOfString(v);  } },
        };

    }

}
