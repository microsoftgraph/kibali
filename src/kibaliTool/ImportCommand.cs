using Kibali;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace KibaliTool
{
    internal class ImportCommandParameters
    {
        public string SourcePermissionsFile;
        public string SourceDescriptionsFile;
        public string OutFolder;
        public bool SingleFile;
    }

    internal class ImportCommand
    {
        public static async Task<int> Execute(ImportCommandParameters commandOptions)
        {
            var importer = new PermissionsImporter();

            var doc = importer.Import(commandOptions.SourcePermissionsFile, commandOptions.SourceDescriptionsFile).Result;

            if (commandOptions.SingleFile)
            {
                await WriteSingleDocument(doc, commandOptions.OutFolder);
            }
            else
            {
                await WriteDocuments(doc, commandOptions.OutFolder);
            }

            return 0;
        }

        public static async Task WriteSingleDocument(PermissionsDocument doc, string outputPath)
        {
            doc.Permissions = new SortedDictionary<string, Permission>(doc.Permissions.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value));
            Directory.CreateDirectory(outputPath);
            var filename = "GraphPermissions";
            using (var outStream = new FileStream($"{outputPath}/{filename}.json", FileMode.Create))
            {
                await doc.WriteAsync(outStream);
            }
        }

        public static async Task WriteDocuments(PermissionsDocument doc, string outputPath)
        {
            PermissionsDocument tempDoc = new PermissionsDocument();
            string currentResource = string.Empty;
            Directory.CreateDirectory(outputPath);
            foreach (var permPair in doc.Permissions.OrderBy(p => p.Key))
            {
                var resource = permPair.Key.Split('.').Take(1).FirstOrDefault();
                if (string.IsNullOrEmpty(currentResource))
                {
                    currentResource = resource;
                }
                if (resource != currentResource)
                {
                    if (tempDoc != null)
                    {
                        Console.WriteLine("Outputing " + currentResource);
                        var filename = currentResource.Replace("/", "-");
                        using (var outStream = new FileStream($"{outputPath}/{filename}.json", FileMode.Create))
                        {
                            await tempDoc.WriteAsync(outStream);
                        }
                    }
                    tempDoc = new PermissionsDocument();
                    currentResource = resource;
                }
                tempDoc.Permissions.Add(permPair.Key, permPair.Value);
            }
        }

    }

}
