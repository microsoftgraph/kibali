using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kibali
{
    public class Permission
    {
        [JsonPropertyName("note")]
        public string Note { get; set; }
        
        [JsonPropertyName("implicit")]
        public bool Implicit { get; set; } = false;
        
        [JsonPropertyName("privilegeLevel")]
        public string PrivilegeLevel { get; set; }
        
        [JsonPropertyName("schemes")]
        public SortedDictionary<string, Scheme> Schemes { get; set; } = new SortedDictionary<string, Scheme>();
        
        [JsonPropertyName("pathSets")]
        public List<PathSet> PathSets { get; set; } = new List<PathSet>();
        
        [JsonPropertyName("ownerInfo")]
        public OwnerInfo OwnerInfo { get; set; } = new();
        
        [JsonPropertyName("authorizationType")]
        public string AuthorizationType { get; set; }
        
        [JsonPropertyName("documentationWebUrl")]
        public string DocumentationWebUrl { get; set; }
    }    
}
