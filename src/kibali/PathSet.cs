using System;
using System.Collections.Generic;
using System.Security;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kibali;

public class PathSet
{
    private SortedDictionary<string, string> paths = new SortedDictionary<string, string>();

    [JsonPropertyName("schemeKeys")]
    public SortedSet<string> SchemeKeys { get; set; } = new SortedSet<string>();

    [JsonPropertyName("methods")]
    public SortedSet<string> Methods { get; set; } = new SortedSet<string>();

    [JsonPropertyName("alsoRequires")]
    public string AlsoRequires { get; set; }

    [JsonPropertyName("excludedProperties")]
    public SortedSet<string> ExcludedProperties { get; set; } = new SortedSet<string>();

    [JsonPropertyName("includedProperties")]
    public SortedSet<string> IncludedProperties { get; set; } = new SortedSet<string>();

    [JsonPropertyName("paths")]
    public SortedDictionary<string, string> Paths
    {
        get => paths;
        set => paths = value;
    }
}