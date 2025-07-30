using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kibali;

public class Scheme
{
    [JsonPropertyName("adminDisplayName")]
    public string AdminDisplayName { get; set; }

    [JsonPropertyName("adminDescription")]
    public string AdminDescription { get; set; }

    [JsonPropertyName("userDisplayName")]
    public string UserDisplayName { get; set; }

    [JsonPropertyName("userDescription")]
    public string UserDescription { get; set; }

    [JsonPropertyName("requiresAdminConsent")]
    public bool RequiresAdminConsent { get; set; }

    [JsonPropertyName("isPreauthorizationOnly")]
    public bool IsPreauthorizationOnly { get; set; }

    [JsonPropertyName("privilegeLevel")]
    public int PrivilegeLevel { get; set; }
}