using System;

namespace Kibali;

public class PermissionsStubGenerator
{
    private readonly PermissionsDocument document;
    private readonly string path;
    private readonly string method;
    public PermissionsStubGenerator(PermissionsDocument document, string path, string method)
    {
        this.document = document;
        this.path = path;
        this.method = method;
    }

    public PermissionsDocument Document { get; set; }

    public string Path { get; set; }

    public string Method { get; set; }

    public string GenerateTable()
    {
        var authZChecker = new AuthZChecker();
        authZChecker.Load(this.document);

        var resource = authZChecker.FindResource(this.path);
        if (resource == null)
        {
            return string.Empty;
        }
        
        var table = string.Empty;
        if (!string.IsNullOrEmpty(this.method))
        {
            if (resource.SupportedMethods.TryGetValue(this.method, out var supportedSchemes))
            {
                table = resource.GeneratePermissionsTable(supportedSchemes);
            }
            else
            {
                throw new ArgumentException($"Unknown method {this.method}");
            }
        }
        return table;
    }
}
