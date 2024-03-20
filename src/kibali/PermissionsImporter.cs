using Kibali;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kibali
{
    public class PermissionsImporter
    {

        public async Task<PermissionsDocument> Import(string inputFile, string permissionsFile)
        {
            var doc = new PermissionsDocument();
            JsonElement rootObject;
            JsonElement permissionsObject;

            using (var client = new HttpClient())
            {
                rootObject = await FetchAndParseJsonAsync(inputFile, client);
                permissionsObject = await FetchAndParseJsonAsync(permissionsFile, client);
            }

            var apiPermissions = rootObject.GetProperty("ApiPermissions");

            CreatePermissions(doc, permissionsObject);

            var entries = CreatePermissionsEntries(apiPermissions);

            var permissionsInfoList = entries.GroupBy(pe => pe.Permission)
                        .Select(gr => new { Permission = gr.Key, Paths = gr.GroupBy(p => p.Path) })
                        .OrderBy(gr => gr.Permission);

            foreach (var permissionInfo in permissionsInfoList)
            {
                if (!doc.Permissions.ContainsKey(permissionInfo.Permission))
                {
                    doc.Permissions.Add(permissionInfo.Permission, new Permission());
                }

                var perm = doc.Permissions[permissionInfo.Permission];
                foreach (var pathDetails in permissionInfo.Paths)
                {
                    SortedSet<string> methods = new SortedSet<string>();
                    SortedSet<string> schemes = new SortedSet<string>();
                    foreach (var entry in pathDetails)
                    {
                        methods.Add(entry.Method);
                        schemes.Add(entry.Scheme);
                    }
                    var pathSet = GetOrCreatePathSet(perm, methods, schemes);
                    pathSet.Paths.Add(pathDetails.Key, string.Empty);
                }
            }
            return doc;
        }

        private void CreatePermissions(PermissionsDocument doc, JsonElement permissionsObject)
        {
            var delegatedElement = permissionsObject.GetProperty("delegatedScopesList");
            foreach (var entry in delegatedElement.EnumerateArray())
            {
                var name = entry.GetProperty("value").GetString();
                if (!doc.Permissions.TryGetValue(name, out var permission))
                {
                    permission = new Permission();
                }

                var scheme = new Scheme
                {
                    RequiresAdminConsent = entry.GetProperty("isAdmin").GetBoolean(),
                    AdminDescription = entry.GetProperty("adminConsentDescription").GetString(),
                    AdminDisplayName = entry.GetProperty("adminConsentDisplayName").GetString(),
                    UserDescription = entry.GetProperty("consentDescription").GetString(),
                    UserDisplayName = entry.GetProperty("consentDisplayName").GetString()
                };

                permission.Schemes["DelegatedWork"] = scheme;

                doc.Permissions.Add(name, permission);
            }


            var applicationElement = permissionsObject.GetProperty("applicationScopesList");
            foreach (var entry in applicationElement.EnumerateArray())
            {
                var name = entry.GetProperty("value").GetString();
                if (!doc.Permissions.TryGetValue(name, out var permission))
                {
                    permission = new Permission();
                    doc.Permissions.Add(name, permission);
                }

                var scheme = new Scheme
                {
                    RequiresAdminConsent = entry.GetProperty("isAdmin").GetBoolean(),
                    AdminDescription = entry.GetProperty("consentDescription").GetString(),
                    AdminDisplayName = entry.GetProperty("consentDisplayName").GetString(),
                };

                permission.Schemes["Application"] = scheme;
            }

        }

        private List<PermissionEntry> CreatePermissionsEntries(JsonElement apiPermissions)
        {
            List<PermissionEntry> entries = new();

            foreach (var path in apiPermissions.EnumerateObject())
            {
                foreach (var method in path.Value.EnumerateObject())
                {
                    foreach (var scheme in method.Value.EnumerateObject())
                    {
                        foreach (var permission in scheme.Value.EnumerateArray())
                        {
                            entries.Add(new PermissionEntry()
                            {
                                Permission = permission.GetString(),
                                Path = path.Name,
                                Method = method.Name,
                                Scheme = scheme.Name
                            });
                        }
                    }
                }
            }
            return entries;
        }

        private async Task<JsonElement> FetchAndParseJsonAsync(string path, HttpClient client)
        {
            Stream inputStream = path.StartsWith("http")
                ? await client.GetStreamAsync(path)
                : new FileStream(path, FileMode.Open);

            using (inputStream)
            {
                var jsonDoc = await JsonDocument.ParseAsync(inputStream);
                return jsonDoc.RootElement;
            }
        }

        private PathSet GetOrCreatePathSet(Permission perm, SortedSet<string> methods, SortedSet<string> schemes)
        {
            var pathSet = perm.PathSets.FirstOrDefault(x => x.SchemeKeys.SetEquals(schemes) && x.Methods.SetEquals(methods));
            if (pathSet != null)
                return pathSet;

            var newPathSet = new PathSet()
            {
                Methods = methods,
                SchemeKeys = schemes
            };
            perm.PathSets.Add(newPathSet);
            return newPathSet;
        }
    }
    public class PermissionEntry
    {
        public string Path { get; set; }
        public string Method { get; set; }
        public string Scheme { get; set; }
        public string Permission { get; set; }
    }
}

