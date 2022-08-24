using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Binding;

namespace KibaliTool
{

    class Program
    {
        // CommandLine
        // vibali import -s {oldPermissionsFile} -o {outFolder} [--single-file] // backfill
        // vibali query -u {url} -m {method} [-s {scheme}]  // GE + Docs
        // vibali export -s {permissionFileSpec} -o {csdlFile}   

        static async Task Main(string[] args)
        {

            Command importCommand = new Command("import") { 
                ImportCommandBinder.PermissionDescriptionOption,
                ImportCommandBinder.PermissionFileOption,
                ImportCommandBinder.OutFolderOption,
                ImportCommandBinder.SingleFileOption
            };
            importCommand.SetHandler(ImportCommand.Execute, new ImportCommandBinder());

            Command queryCommand = new Command("query") {
                QueryCommandBinder.PermissionFileOption,
                QueryCommandBinder.UrlOption,
                QueryCommandBinder.MethodOption,
                QueryCommandBinder.SchemeOption
            };
            
            queryCommand.SetHandler(QueryCommand.Execute, new QueryCommandBinder());
            
            Command exportCommand = new Command("export");
            
            var rootCommand = new RootCommand()
            {
                importCommand,
                queryCommand,
                exportCommand
            };
            

            await rootCommand.InvokeAsync(args);
            
            //var doc = new PermissionsDocument();
            ////   ParseFromMerillCSV(doc, "../../../../permissions.csv");
            //await ImportCommand.ParseFromGEPermissions(doc, "https://raw.githubusercontent.com/microsoftgraph/microsoft-graph-devx-content/dev/permissions/permissions-beta.json", "https://raw.githubusercontent.com/microsoftgraph/microsoft-graph-devx-content/dev/permissions/permissions-descriptions.json");
            ////   await WriteDocuments(doc, "./output");
            //await ImportCommand.WriteSingleDocument(doc, "./output");
        }
    }

    internal class ImportCommandBinder : BinderBase<ImportCommandParameters>
    {
        public static Option<string> PermissionDescriptionOption = new (new[] { "--permissionDescription", "--pd" }, "GE Permission Description File");
        public static Option<string> PermissionFileOption = new (new[] { "--permissionFile", "--pf" }, "GE Permissions File");
        public static Option<string> OutFolderOption = new (new[] { "--outFolder", "--of" }, "Output folder");
        public static Option<bool> SingleFileOption = new (new[] { "--singleFile", "--sf" }, "Single file");

        public ImportCommandBinder()
        {
            PermissionDescriptionOption.SetDefaultValue(@"https://raw.githubusercontent.com/microsoftgraph/microsoft-graph-devx-content/dev/permissions/permissions-descriptions.json");
            PermissionFileOption.SetDefaultValue(@"https://raw.githubusercontent.com/microsoftgraph/microsoft-graph-devx-content/dev/permissions/permissions-beta.json");
            OutFolderOption.SetDefaultValue(@"./output");
            SingleFileOption.SetDefaultValue(true);
        }
        protected override ImportCommandParameters GetBoundValue(BindingContext bindingContext)
        {
            return new ImportCommandParameters()
            {
                SourceDescriptionsFile = bindingContext.ParseResult.GetValueForOption(PermissionDescriptionOption),
                SourcePermissionsFile = bindingContext.ParseResult.GetValueForOption(PermissionFileOption),
                OutFolder = bindingContext.ParseResult.GetValueForOption(OutFolderOption),
                SingleFile = bindingContext.ParseResult.GetValueForOption(SingleFileOption)
            };
        }
    }


    internal class QueryCommandBinder : BinderBase<QueryCommandParameters>
    {
        public static Option<string> PermissionFileOption = new (new[] { "--sourcePermissionFile", "--pf" }, "Permission File");
        public static Option<string> UrlOption = new (new[] { "--url", "-u" }, "Test Url");
        public static Option<string> MethodOption = new (new[] { "--method", "-m" }, "Method");
        public static Option<string> SchemeOption = new( new[] { "--scheme", "-s" }, "Scheme");

        protected override QueryCommandParameters GetBoundValue(BindingContext bindingContext)
        {
            return new QueryCommandParameters()
            {
                SourcePermissionsFile = bindingContext.ParseResult.GetValueForOption(PermissionFileOption),
                Url = bindingContext.ParseResult.GetValueForOption(UrlOption),
                Method = bindingContext.ParseResult.GetValueForOption(MethodOption),
                Scheme = bindingContext.ParseResult.GetValueForOption(SchemeOption)
            };
        }
    }
}
