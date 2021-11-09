using OAuthPermissions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OAuthTool
{

    class Program
    {

        static async Task Main(string[] args)
        {
            var doc = new PermissionsDocument();

         //   ParseFromMerillCSV(doc, "../../../../permissions.csv");
            await ParseFromGEPermissions(doc, "../../../../permissions-beta.json");

            await WriteDocuments(doc, "./output");
        }

        private static void ParseFromMerillCSV(PermissionsDocument doc, string inputfile)
        {
            var reader = new StreamReader(inputfile);
            reader.ReadLine(); // Skip titles.
            string line;
            string[] row;
            while ((line = reader.ReadLine()) != null)
            {
                row = line.Split(',');

                var name = row[0];

                CreatePath(doc, name, path: row[5], method: row[4].TrimEnd(), GetSchemeType(bool.Parse(row[1]), bool.Parse(row[2])));
            }
        }

        private static async Task ParseFromGEPermissions(PermissionsDocument doc, string inputfile)
        {

            var jsonDoc = await JsonDocument.ParseAsync(new FileStream(inputfile, FileMode.Open));

            var rootObject = jsonDoc.RootElement;

            var apiPermissions = rootObject.GetProperty("ApiPermissions");

            var permissionSchemes = rootObject.GetProperty("PermissionSchemes");

            ProcessPermissionsSchemes(SchemeType.DelegatedPersonal,
                                        permissionSchemes.GetProperty("DelegatedPersonal"), doc);
            ProcessPermissionsSchemes(SchemeType.DelegatedWork,
                                        permissionSchemes.GetProperty("DelegatedWork"), doc);
            ProcessPermissionsSchemes(SchemeType.Application,
                                        permissionSchemes.GetProperty("Application"), doc);
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
                    HashSet<string> methods = new HashSet<string>();
                    HashSet<string> schemes = new HashSet<string>();
                    foreach (var entry in pathDetails)
                    {
                        methods.Add(entry.Method);
                        schemes.Add(entry.Scheme);
                    }
                    var pathSet = GetOrCreatePathSet(perm, methods, schemes);
                    pathSet.Paths.Add(pathDetails.Key, new OAuthPermissions.Path());
                }
            }


        }

        private static PathSet GetOrCreatePathSet(Permission perm, HashSet<string> methods, HashSet<string> schemes)
        {
            foreach (var pathSet in perm.PathSets)
            {
                if (pathSet.Schemes.SetEquals(schemes) && pathSet.Methods.SetEquals(methods))
                {
                    return pathSet;
                }
            }
            var newPathSet = new PathSet()
            {
                Methods = methods,
                Schemes = schemes
            };
            perm.PathSets.Add(newPathSet);
            return newPathSet;
        }

        private static async Task WriteDocuments(PermissionsDocument doc, string outputPath)
        {
            PermissionsDocument tempDoc = new PermissionsDocument();
            string currentResource = string.Empty;
            Directory.CreateDirectory(outputPath);
            foreach (var permPair in doc.Permissions.OrderBy(p => p.Key))
            {
                var resource = permPair.Key.Split('.').Take(1).FirstOrDefault();
                if (String.IsNullOrEmpty(currentResource))
                {
                    currentResource = resource;
                }
                if (resource != currentResource)
                {
                    if (tempDoc != null)
                    {
                        Console.WriteLine("Outputing " + currentResource);
                        using (var outStream = new FileStream($"{outputPath}/{currentResource}.json", FileMode.Create))
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

        private static List<PermissionEntry> CreatePermissionsEntries(JsonElement apiPermissions)
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
                            entries.Add(new PermissionEntry() { 
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

        private static void ProcessPermissionsSchemes(SchemeType schemeType, JsonElement schemes, PermissionsDocument doc)
        {
            foreach( var scheme in schemes.EnumerateArray())
            {
                Permission perm;
                var name = scheme.GetProperty("Name").GetString();
                // Use name to see if permission exists, if not create it
                if (!doc.Permissions.TryGetValue(name, out perm))
                {
                    perm = new Permission();
                    doc.Permissions.Add(name, perm);
                }

                // add scheme of schemeType, set descriptions
                var newScheme = new Scheme
                {
                    RequiresAdminConsent = scheme.GetProperty("Grant").GetString() == "admin",
                    Description = scheme.GetProperty("Description").GetString(),
                    ConsentDescription = scheme.GetProperty("CosentDescription").GetString()
                };
                if (!perm.Schemes.ContainsKey(schemeType)) {
                    perm.Schemes.Add(schemeType, newScheme);
                } else {
                    Console.WriteLine($"Duplicate entry for {name} in scheme {schemeType}");
                }
            }
        }

        private static SchemeType GetSchemeType(bool isApplication, bool isDelegatedWork)
        {
            if (isApplication)
            {
                // Add ApplicationPermission
                return SchemeType.Application;
            }
            if (isDelegatedWork)
            {
                // Add DelegatePermission
             return SchemeType.DelegatedWork;
            }
            return SchemeType.DelegatedPersonal;
        }

        private static void CreatePath(PermissionsDocument doc, string name, string path, string method, SchemeType type)
        {
            if (doc.Permissions.TryGetValue(name, out Permission perm))
            {

                if (!perm.Schemes.ContainsKey(type))
                {
                    perm.Schemes.Add(type, new Scheme() { Type = type });
                }
                PathSet pathSet;
                if (perm.PathSets.Count == 0)
                {
                    pathSet = new PathSet();
                    perm.PathSets.Add(pathSet);
                } else
                {
                    pathSet = perm.PathSets[0];
                }

                pathSet.Methods.Add(method);
                pathSet.Paths.TryAdd(path, new OAuthPermissions.Path());
            }
            else
            {
                var newPermission = new Permission();
                doc.Permissions.Add(name, newPermission);
            }
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
