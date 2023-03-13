using Kibali;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace KibaliTool
{
    internal class QueryCommandParameters {
        public string SourcePermissionsFile;
        public string Url;
        public string Method;
        public string Scheme;
        public bool LeastPrivilege;
    }

    internal class QueryCommand
    {
        
        public static async Task<int> Execute(QueryCommandParameters queryCommandParameters)
        {
            var doc = PermissionsDocument.Load(new FileStream(queryCommandParameters.SourcePermissionsFile, FileMode.Open));

            var authZChecker = new AuthZChecker();
            authZChecker.Load(doc);

            var resource = authZChecker.FindResource(queryCommandParameters.Url);

            if(resource == null)
            {
                Console.WriteLine($"Resource {queryCommandParameters.Url} not found in the input file.");
                return 0;
            }

            var writer = new Utf8JsonWriter(Console.OpenStandardOutput(), new JsonWriterOptions() { Indented= true });

            if (!String.IsNullOrEmpty(queryCommandParameters.Scheme))
            {
                if (String.IsNullOrEmpty(queryCommandParameters.Method))
                {
                    throw new ArgumentException("Missing method");
                }
                if (resource.SupportedMethods.TryGetValue(queryCommandParameters.Method, out var supportedSchemes))
                {
                    resource.WriteAcceptableClaims(writer, supportedSchemes[queryCommandParameters.Scheme]);
                } else
                {
                    throw new ArgumentException("Unknown scheme");
                }
            }
            else if (!String.IsNullOrEmpty(queryCommandParameters.Method))
            {
                if (resource.SupportedMethods.TryGetValue(queryCommandParameters.Method, out var supportedSchemes))
                {
                    resource.WriteSupportedSchemes(writer, supportedSchemes);
                } 
                else
                {
                    throw new ArgumentException("Unknown method");
                }
            }
            else
            {
                resource.Write(writer);
            }

            await writer.FlushAsync();

            if (queryCommandParameters.LeastPrivilege)
            {
                Console.WriteLine();
                var leastPrivilege = resource.FetchLeastPrivilege(queryCommandParameters.Method, queryCommandParameters.Scheme);
                Console.WriteLine(leastPrivilege);
            }

            return 0;
        }
    }
}
