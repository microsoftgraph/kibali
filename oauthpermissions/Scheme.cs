using System;
using System.Text.Json;

namespace ApiPermissions
{
    public class Scheme
    {
        public string AdminDisplayName { get; set; }
        public string AdminDescription { get; set; }
        public string UserDisplayName { get; set; }
        public string UserDescription { get; set; }
        public bool RequiresAdminConsent { get; set; }

        public void Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            if (!String.IsNullOrEmpty(AdminDisplayName)) writer.WriteString("adminDisplayName", AdminDisplayName);
            if (!String.IsNullOrEmpty(AdminDescription)) writer.WriteString("adminDescription", AdminDescription);
            if (!String.IsNullOrEmpty(UserDisplayName)) writer.WriteString("userDisplayName", UserDisplayName);
            if (!String.IsNullOrEmpty(UserDescription)) writer.WriteString("userDescription", UserDescription);
            if (RequiresAdminConsent == true) writer.WriteBoolean("requiresAdminConsent", RequiresAdminConsent);

            writer.WriteEndObject();
        }
    }

}
