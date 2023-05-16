namespace Kibali;

public class PermissionsStubGenerator
{
    private readonly PermissionsDocument document;
    private readonly string path;
    private readonly string method;
    private readonly bool generateDefault;
    private readonly bool lenientMatch;
    public PermissionsStubGenerator(PermissionsDocument document, string path, string method, bool generateDefault = false, bool lenientMatch = false)
    {
        this.document = document;
        this.path = path;
        this.method = method;
        this.generateDefault = generateDefault;
        this.lenientMatch = lenientMatch;
    }
    
    public PermissionsDocument Document { get; set; }

    public string Path { get; set; }

    public string Method { get; set; }

    public bool LenientMatch { get; set; }

    public string GenerateTable()
    {
        var authZChecker = new AuthZChecker() { LenientMatch = this.lenientMatch };
        authZChecker.Load(this.document);

        var resource = authZChecker.FindResource(this.path);
        var table = this.generateDefault ? this.UnsupportedPermissionsStub() : string.Empty;
        if (resource == null)
        {
            return table;
        }
        
        if (!string.IsNullOrEmpty(this.method))
        {
            if (resource.SupportedMethods.TryGetValue(this.method, out var supportedSchemes))
            {
                  table = resource.GeneratePermissionsTable(this.method, supportedSchemes);
            }
        }
        return table;
    }

    private string UnsupportedPermissionsStub(){
        var permissionsStub = "Not supported.";
        var markdownBuilder = new MarkDownBuilder();
        markdownBuilder.StartTable("Permission type", "Least privileged permissions", "Higher privileged permissions");

        markdownBuilder.AddTableRow("Delegated (work or school account)", permissionsStub, permissionsStub);

        markdownBuilder.AddTableRow("Delegated (personal Microsoft account)", permissionsStub, permissionsStub);

        markdownBuilder.AddTableRow("Application", permissionsStub, permissionsStub);
        markdownBuilder.EndTable();
        return markdownBuilder.ToString();
    }
}
