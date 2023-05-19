using Kibali;
using oauthpermissions;
using System.IO;
using System.Threading.Tasks;

namespace KibaliTool
{
    internal class ExportCommand
    {

        public async Task<int> Execute(string sourcePermissionsFile, string outFile)
        {
            var doc = PermissionsDocument.Load(new FileStream(sourcePermissionsFile, FileMode.Open));
            
            CsdlExporter.Export(outFile, doc);

            return 0;
        }

    }
}
