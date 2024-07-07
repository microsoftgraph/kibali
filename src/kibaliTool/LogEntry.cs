using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace KibaliTool
{
        /*  To produce the log file you can use this Kusto query and then Export to JSON

        AggregatorServiceLogEvent 
        | where env_time > ago(1h)
        | where tagId == 30746268
        | where responseStatusCode >= 200 and responseStatusCode < 300 
        | project correlationId, requestMethod, incomingUri, tokenClaims, responseStatusCode,tagId
        | limit 10000
        */


        // "TableName":"Results",
        // "Columns":[  {"ColumnName":"correlationId","DataType":"String"},
        //              {"ColumnName":"requestMethod","DataType":"String"},
        //              {"ColumnName":"incomingUri","DataType":"String"},
        //              {"ColumnName":"tokenClaims","DataType":"String"},
        //              {"ColumnName":"responseStatusCode","DataType":"Int64"},
        //              {"ColumnName":"tagId","DataType":"String"}]

    public class LogEntry
    {

        public string Method;
        public string Scheme;
        public string Url;
        public string Claims;

        private Regex scpRegex = new Regex(@"scp=([^\;]+)", RegexOptions.Compiled);
        private Regex rolesRegex = new Regex(@"roles=([^\;]+)", RegexOptions.Compiled);
        private Regex roleRegex = new Regex("\"(.*?)\"", RegexOptions.Compiled);

        public string[] Permissions;

        private void Process() {

            Url = NormalIzeUrl(Url);
        

            // Calculate Scheme
            Scheme = Claims.Contains("role") ? "Application" : "DelegatedWork";
            // Calculate Permissions

            string[] permissionList = System.Array.Empty<string>();

            if (Scheme == "DelegatedWork")
            {
                Match match = scpRegex.Match(Claims);

                if (match.Success)
                {
                    string scpValues = match.Groups[1].Value.Trim();
                    permissionList = scpValues.Split(' ');
                }
            }
            else
            {
                Match match = rolesRegex.Match(Claims);

                if (match.Success)
                {
                    string roleValues = match.Groups[1].Value.Trim();
                    MatchCollection matches = roleRegex.Matches(roleValues);
                    permissionList = matches.Select(m => m.Groups[1].Value).ToArray();
                }
            }
            Permissions = permissionList;
        }

        private string NormalIzeUrl(string url)
        {
            // Use regeg to tranform the url to use / as a segment separator 
            url = Regex.Replace(url, @"\(([^)]*)\)", "/$1").ToLower();

            // Remove $value
            url = Regex.Replace(url, @"\/\$value", string.Empty);

            return url;
        }

        public static LogEntry Load(JsonElement row) {
            var logentry = new LogEntry()
            {
                Method = row[RequestMethod].GetString(),
                Url = row[IncomingUri].GetString(),
                Claims = row[TokenClaims].GetString()
            };
            logentry.Process();
            return logentry;
        }
        
        private const int CorrelationId = 0;
        private const int RequestMethod = 1;
        private const int IncomingUri = 2;
        
        private const int TokenClaims = 3;
        private const int ResponseStatusCode = 4;
        private const int TagId = 5;
    }
}