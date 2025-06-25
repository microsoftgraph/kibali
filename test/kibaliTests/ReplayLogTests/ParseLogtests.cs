// Create an xunit test that opens the graphlogs.json and parses the first fiew lines and write out the logentries

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Xunit;
using KibaliTool;


namespace KibaliTool.Tests
{
    public class ParseLogTests
    {
        [Fact]
        public void ParseLog()
        {
            var logstream = new FileStream(@"C:\Users\darrmi\src\github\microsoftgraph\kibali\graphlog.json", FileMode.Open);
            using var jsonDocument = JsonDocument.Parse(logstream);
            var root = jsonDocument.RootElement;
            var row = root.GetProperty("Rows").EnumerateArray().First();
            var entry = LogEntry.Load(row);
            Assert.NotNull(entry.Method);
            Assert.NotNull(entry.Url);
            Assert.NotNull(entry.Claims);
            Assert.NotNull(entry.Scheme);
            Assert.NotNull(entry.Permissions);
        }

    }
}