using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Kibali;

public class PermissionsDocument
{
    private SortedDictionary<string, Permission> permissions = new SortedDictionary<string, Permission>();
    
    [JsonPropertyName("permissions")]
    public SortedDictionary<string, Permission> Permissions { get => permissions; set => permissions = value; }

    [JsonPropertyName("$schema")]
    public string Schema { get; set; } = "https://microsoftgraph.github.io/msgraph-metadata/graph-permissions-schema.json";

    public async Task WriteAsync(FileStream outStream)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        
        await JsonSerializer.SerializeAsync(outStream, this, options);
    }

    public static PermissionsDocument Load(string document) 
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        return JsonSerializer.Deserialize<PermissionsDocument>(document, options);
    }

    public static PermissionsDocument Load(Stream documentStream)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        return JsonSerializer.Deserialize<PermissionsDocument>(documentStream, options);
    }
    
    public static PermissionsDocument Load(JsonDocument doc)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        return JsonSerializer.Deserialize<PermissionsDocument>(doc.RootElement.GetRawText(), options);
    }

    public static PermissionsDocument Load(JsonElement value)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        return JsonSerializer.Deserialize<PermissionsDocument>(value.GetRawText(), options);
    }

    public static PermissionsDocument LoadFromFolder(string documentPath)
    {
        var mergedDoc = new PermissionsDocument();
        var mergedPermissions = new Dictionary<string, Permission>();
        foreach (var permissionsFile in Directory.EnumerateFiles(documentPath, "*.json"))
        {
            if (Path.GetFileName(permissionsFile).Equals("provisioningInfo.json", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            try
            {
                using var stream = new FileStream(permissionsFile, FileMode.Open);
                var doc = Load(stream);
                mergedPermissions = mergedPermissions.Concat(doc.Permissions).ToDictionary(x => x.Key, x => x.Value);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Unable to parse json file {permissionsFile}. {ex.Message}");
            }
        }

        mergedDoc.Permissions = new SortedDictionary<string, Permission>(mergedPermissions);
        return mergedDoc;
    }
}

public enum SchemeType
{
    DelegatedWork,
    DelegatedPersonal,
    Application
}
