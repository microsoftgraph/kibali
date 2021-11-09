using System;
using System.Collections.Generic;
using System.Text.Json;

namespace OAuthPermissions
{
    public class Path
    {
        public List<String> ExcludeProperties { get; set; } = new List<String>();
        public void Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            if (ExcludeProperties.Count > 0)
            {
                writer.WritePropertyName("excludeProperties");
                writer.WriteStartArray();
                foreach (var prop in ExcludeProperties)
                {
                    writer.WriteStringValue(prop);
                }
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }
    }

}
