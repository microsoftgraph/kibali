using Kibali;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace KibaliTool
{
    internal class ReplayLogCommandParameters
    {
        public string SourcePermissionsFolder;
        public string LogFile;
        public bool LenientMatch;
        public int Count = 100;
    }

    internal class ReplayLogCommand
    {

    
        public static async Task<int> Execute(ReplayLogCommandParameters replayLogCommandParameters)
        {
            var doc = PermissionsDocument.LoadFromFolder(replayLogCommandParameters.SourcePermissionsFolder);

            var authZChecker = new AuthZChecker() { LenientMatch = replayLogCommandParameters.LenientMatch };
            authZChecker.Load(doc);

            // Read the JSON log file using a streaming API
            using var logstream = new FileStream(replayLogCommandParameters.LogFile, FileMode.Open);

            IEnumerable<LogEntry> entries = LoadLogEntries(logstream, replayLogCommandParameters.Count).ToList();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            using var writer = new Utf8JsonWriter(Console.OpenStandardOutput(), new JsonWriterOptions() { Indented = true,SkipValidation = true });
            writer.WriteStartArray();

            int successRequests = 0;

            foreach (var entry in entries)
            {
                string failReason = null;
                Dictionary<string , List<AcceptableClaim>> supportedSchemes = null;
                List<AcceptableClaim> acceptableClaims = null;
                List<string> acceptablePermissions = new List<string>();
                List<string> relevantPermissions = new List<string>();
                var resource = authZChecker.FindResource(entry.Url);

                if (resource == null)
                {
                    failReason = "No matching resource";
                }
                if (failReason == null && !resource.SupportedMethods.TryGetValue(entry.Method, out supportedSchemes))
                {
                    failReason = "No matching method";
                }

                if (failReason == null && !supportedSchemes.TryGetValue(entry.Scheme, out acceptableClaims))
                {
                    failReason = "No matching scheme"; 
                }

                if (failReason == null )
                {
                    acceptablePermissions = acceptableClaims.Select(c => c.Permission).ToList();
                    relevantPermissions = entry.Permissions.Where(claim => acceptablePermissions.Contains(claim)).ToList();
                }
                
                if (failReason == null && !relevantPermissions.Any())
                {
                    failReason = "No matching permissions";
                }
                if (failReason != null) {
                    writer.WriteStartObject();
                    writer.WriteString("failReason", failReason);
                    writer.WriteString("url", entry.Url);
                    writer.WriteString("method", entry.Method);
                    writer.WriteString("scheme", entry.Scheme);
                    if (failReason == "No matching permissions") {
                        writer.WriteString("loggedClaims", String.Join(",", entry.Permissions));
                        writer.WriteString("requiredClaims", String.Join(",", acceptablePermissions));
                    }
                    writer.WriteEndObject();
                } else {
                    successRequests++;
                }
            }
            stopwatch.Stop();   
            

            writer.WriteEndArray();
            await writer.FlushAsync();

            Console.WriteLine($"Elapsed time: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Total success requests: {successRequests}");
            return 0;

        }


        public static IEnumerable<LogEntry> LoadLogEntries(Stream stream, int count)
        {
            var document = JsonDocument.Parse(stream);
            var root = document.RootElement;
            var rows = root.GetProperty("Rows");
            return rows.EnumerateArray().Take(count).Select(row => LogEntry.Load(row));
        }

    }
}
