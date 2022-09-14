using Kibali;
using System;
using System.IO;
using System.Threading.Tasks;

namespace KibaliTool;

internal class ValidateCommandParameters
{
    public string SourcePermissionsFile;
    public string SourcePermissionsFolder;
}

internal class ValidateCommand
{
    public static async Task<int> Execute(ValidateCommandParameters validateCommandParameters)
    {
        PermissionsDocument doc;
        if (validateCommandParameters.SourcePermissionsFile != null)
        {
            using var stream = new FileStream(validateCommandParameters.SourcePermissionsFile, FileMode.Open);
            doc = PermissionsDocument.Load(stream);
        }
        else if (validateCommandParameters.SourcePermissionsFolder != null)
        {
            doc = PermissionsDocument.LoadFromFolder(validateCommandParameters.SourcePermissionsFolder);
        }
        else
        {
            throw new ArgumentException("Please provide a source permissions file or folder");
        }

        var authZChecker = new AuthZChecker();
        var errors = authZChecker.Validate(doc);

        return 0;
    }
}
