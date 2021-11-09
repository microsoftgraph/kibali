using System;
using System.Text.Json;

namespace OAuthPermissions
{
    public class Scheme
    {
        public SchemeType Type { get; set; }
        public string Description { get; set; }
        public string ConsentDescription { get; set; }
        public bool RequiresAdminConsent { get; set; }

        public void Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WriteString("type", Type.ToString());

            if (RequiresAdminConsent)
            {
                writer.WriteBoolean("admin", RequiresAdminConsent);
            }

            if (!String.IsNullOrEmpty(Description))
            {
                writer.WriteString("description", Description);
            }

            if (!String.IsNullOrEmpty(ConsentDescription))
            {
                writer.WriteString("consentDescription", ConsentDescription);
            }

            writer.WriteEndObject();
        }
    }

}
