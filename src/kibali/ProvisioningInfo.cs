using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kibali;

public class ProvisioningInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("scheme")]
    public string Scheme { get; set; }

    [JsonPropertyName("isHidden")]
    public bool IsHidden { get; set; }

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; }

    [JsonPropertyName("environment")]
    public string Environment { get; set; }

    [JsonPropertyName("resourceAppId")]
    public string ResourceAppId { get; set; }
}
