using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Kibali;

public class OwnerInfo
{
    [JsonPropertyName("ownerSecurityGroup")]
    public string OwnerSecurityGroup { get; set; }
}
