using Azure.Core;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using ApiPermissions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using oauthpermissions;
using System.Net.Http;

namespace OAuthTool
{

    class Program
    {

        static async Task Main(string[] args)
        {
            var doc = new PermissionsDocument();

            //   ParseFromMerillCSV(doc, "../../../../permissions.csv");
            await ParseFromGEPermissions(doc, "https://raw.githubusercontent.com/microsoftgraph/microsoft-graph-devx-content/dev/permissions/permissions-beta.json", "https://raw.githubusercontent.com/microsoftgraph/microsoft-graph-devx-content/dev/permissions/permissions-descriptions.json");

            //CsdlExporter.Export("csdl.xml", doc);

            //   await WriteDocuments(doc, "./output");

            await WriteSingleDocument(doc, "./output");
        }

        private static async Task TestGraphAPI(PermissionsDocument permissionDocument)
        {
            var stream = new MemoryStream();
            var result = await new OpenApiStreamReader().ReadAsync(stream);
            var openApiDoc = result.OpenApiDocument;
            var authZChecker = new AuthZChecker();
            authZChecker.Load(permissionDocument);

            //Create Application

            //Create credentials to use for testing delegated permissions
            TokenCredential delegatedCredential = null;
            //Create credentials to use for testing application-only permissions
            TokenCredential appOnlyCredential = null;
            
            foreach (var (name,permission) in permissionDocument.Permissions)
            {
                // Consent app to the permission
                
                TestPaths(openApiDoc, (string url) => AccessRequestResult.Success == authZChecker.CanAccess(url, "GET", "DelegatedWork", new string[] { name } ), delegatedCredential);
                TestPaths(openApiDoc, (string url) => AccessRequestResult.Success == authZChecker.CanAccess(url, "GET", "Application", new string[] { name }), appOnlyCredential);

                // Remove consent from the app
            } 

            // Delete the application
        }

        private static void TestPaths(OpenApiDocument openApiDoc, Func<string,bool> canAccess, TokenCredential credential)
        {
            foreach (var (url, pathItem) in openApiDoc.Paths)
            {
                foreach (var (method, operation) in pathItem.Operations)
                {
                    List<string> allowedUrls = null;
                    HttpStatusCode status = HttpStatusCode.OK;
                    switch (method)
                    {
                        case Microsoft.OpenApi.Models.OperationType.Get:
                            status = TestGetOperation(operation);
                            break;
                        case Microsoft.OpenApi.Models.OperationType.Put:
                            break;
                        case Microsoft.OpenApi.Models.OperationType.Post:
                            break;
                        case Microsoft.OpenApi.Models.OperationType.Delete:
                            break;
                        case Microsoft.OpenApi.Models.OperationType.Patch:
                            break;
                        default:
                            break;
                    }

                    if (canAccess(url) )
                    {
                        if (status == HttpStatusCode.Forbidden)
                        {
                            // This is a problem
                        }
                    } else
                    {
                        if (status != HttpStatusCode.Forbidden)
                        {
                            // This is a problem
                        }
                    }

                    if (!allowedUrls.Contains(url) && status != HttpStatusCode.Forbidden)
                    {
                        // Danger will
                    }

                }
            }
        }

        private static HttpStatusCode TestGetOperation(OpenApiOperation operation)
        {
            throw new NotImplementedException();
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

        private static async Task ParseFromGEPermissions(PermissionsDocument doc, string inputfile, string permissionsFile)
        {
            Stream input = null;
            if (inputfile.StartsWith("http"))
            {
                var client = new HttpClient();
                input = await client.GetStreamAsync(inputfile);
            }
            else
            {
                input = new FileStream(inputfile, FileMode.Open);
            }

            var jsonDoc = await JsonDocument.ParseAsync(input);
            var rootObject = jsonDoc.RootElement;


            Stream permissionsInput = null;
            if (permissionsFile.StartsWith("http"))
            {
                var client = new HttpClient();
                permissionsInput = await client.GetStreamAsync(permissionsFile);
            }
            else
            {
                permissionsInput = new FileStream(permissionsFile, FileMode.Open);
            }
            var permissionsDoc = await JsonDocument.ParseAsync(permissionsInput);
            var permissionsObject = permissionsDoc.RootElement;

            var apiPermissions = rootObject.GetProperty("ApiPermissions");

            //CreatePermissions(doc, rootObject);
            CreatePermissions2(doc, permissionsObject);

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
                    pathSet.Paths.Add(pathDetails.Key, new ApiPermissions.PathConstraints());
                }
            }
        }

        private static void CreatePermissions2(PermissionsDocument doc, JsonElement permissionsObject)
        {
            var delegatedElement = permissionsObject.GetProperty("delegatedScopesList");
            foreach (var entry in delegatedElement.EnumerateArray())
            {
                var name = entry.GetProperty("value").GetString();
                if (!doc.Permissions.TryGetValue(name, out var permission))
                {
                    permission = new Permission();
                }
                permission.IsHidden = !entry.GetProperty("isEnabled").GetBoolean();

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
                permission.IsHidden = !entry.GetProperty("isEnabled").GetBoolean();

                var scheme = new Scheme
                {
                    RequiresAdminConsent = entry.GetProperty("isAdmin").GetBoolean(),
                    AdminDescription = entry.GetProperty("consentDescription").GetString(),
                    AdminDisplayName = entry.GetProperty("consentDisplayName").GetString(),
                };

                permission.Schemes["Application"] = scheme;
            }

        }

        private static void CreatePermissionObjects(PermissionsDocument doc, JsonElement delegatedElement, string schemeName)
        {
        }

        private static void CreatePermissions(PermissionsDocument doc, JsonElement rootObject)
        {
            var permissionSchemes = rootObject.GetProperty("PermissionSchemes");

            ProcessPermissionsSchemes("DelegatedPersonal",
                                        permissionSchemes.GetProperty("DelegatedPersonal"), doc);
            ProcessPermissionsSchemes("DelegatedWork",
                                        permissionSchemes.GetProperty("DelegatedWork"), doc);
            ProcessPermissionsSchemes("Application",
                                        permissionSchemes.GetProperty("Application"), doc);
        }

        private static PathSet GetOrCreatePathSet(Permission perm, HashSet<string> methods, HashSet<string> schemes)
        {
            foreach (var pathSet in perm.PathSets)
            {
                if (pathSet.SchemeKeys.SetEquals(schemes) && pathSet.Methods.SetEquals(methods))
                {
                    return pathSet;
                }
            }
            var newPathSet = new PathSet()
            {
                Methods = methods,
                SchemeKeys = schemes
            };
            perm.PathSets.Add(newPathSet);
            return newPathSet;
        }

        private static async Task WriteSingleDocument(PermissionsDocument doc, string outputPath)
        {
            doc.Permissions = doc.Permissions.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
            Directory.CreateDirectory(outputPath);
            var filename = "GraphPermissions";
            using (var outStream = new FileStream($"{outputPath}/{filename}.json", FileMode.Create))
            {
                await doc.WriteAsync(outStream);
            }
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

        private static void ProcessPermissionsSchemes(string schemeType, JsonElement schemes, PermissionsDocument doc)
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
                    UserDescription = scheme.GetProperty("Description").GetString(),
                    AdminDescription = scheme.GetProperty("ConsentDescription").GetString()
                };
                if (!perm.Schemes.ContainsKey(schemeType))
                {
                    perm.Schemes.Add(schemeType, newScheme);
                }
                else
                {
                    Console.WriteLine($"Duplicate entry for {name} in scheme {schemeType}");
                }
            }
        }

        private static string GetSchemeType(bool isApplication, bool isDelegatedWork)
        {
            if (isApplication)
            {
                // Add ApplicationPermission
                return "Application";
            }
            if (isDelegatedWork)
            {
                // Add DelegatePermission
             return "DelegatedWork";

            }
            return "DelegatedPersonal";
        }

        private static void CreatePath(PermissionsDocument doc, string name, string path, string method, string type)
        {
            if (doc.Permissions.TryGetValue(name, out Permission perm))
            {

                if (!perm.Schemes.ContainsKey(type))
                {
                    perm.Schemes.Add(type, new Scheme() );
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
                pathSet.Paths.TryAdd(path, new ApiPermissions.PathConstraints());
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
