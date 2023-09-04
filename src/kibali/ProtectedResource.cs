using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Kibali
{
    public class ProtectedResource
    {
        // Permission -> (Methods,Scheme) -> Path  (Darrel's format)
        // (Schemes -> Permissions) -> restriction -> target  (Kanchan's format)
        // target -> restrictions -> schemes -> Ordered Permissions (CSDL Format) 

        // path -> Method -> Schemes -> Permissions  (Inverted format) 

        // (Path, Method) -> Schemes -> Permissions (Docs)
        // (Path, Method) -> Scheme(delegated) -> Permissions (Graph Explorer Tab)
        // Permissions(delegated) (Graph Explorer Permissions List)
        // Schemas -> Permissions ( AAD Onboarding)
        public string Url { get; set; }
        public Dictionary<string, Dictionary<string, List<AcceptableClaim>>> SupportedMethods { get; set; } = new Dictionary<string, Dictionary<string, List<AcceptableClaim>>>();

        public Dictionary<(string, string), HashSet<string>> PermissionMethods {get; set;} = new();
        public ProtectedResource(string url)
        {
            Url = url;
        }

        public void AddRequiredClaims(string permission, PathSet pathSet, string[] leastPrivilegedPermissionSchemes, List<ProvisioningInfo> provisioningData)
        {
            Dictionary<string, ProvisioningInfo> schemeProvisioning = provisioningData?.ToDictionary(info => info.Scheme, info => info) ?? new();
            foreach (var supportedMethod in pathSet.Methods)
            {
                var supportedSchemes = new Dictionary<string, List<AcceptableClaim>>();
                foreach (var schemeKey in pathSet.SchemeKeys)
                {
                    if(!supportedSchemes.TryGetValue(schemeKey, out var acceptableClaims))
                    {
                        acceptableClaims = new List<AcceptableClaim>();
                        supportedSchemes.Add(schemeKey, acceptableClaims);
                    }

                    var permissionMethodKey = (permission, schemeKey);
                    if (!this.PermissionMethods.TryAdd(permissionMethodKey, new HashSet<string> { supportedMethod }))
                    {
                        this.PermissionMethods[permissionMethodKey].Add(supportedMethod);
                    }

                    var isLeastPrivilege = leastPrivilegedPermissionSchemes.Contains(schemeKey);
                    var claim = new AcceptableClaim(permission, pathSet.AlsoRequires, isLeastPrivilege);
                    if (schemeProvisioning.TryGetValue(schemeKey, out ProvisioningInfo provisioningInfo))
                    {
                        claim.IsHidden = provisioningInfo.IsHidden;
                        claim.SupportedEnvironments = provisioningInfo.Environment?.Split(";").ToList();
                        claim.IsEnabled = provisioningInfo.IsEnabled;
                    }
                    acceptableClaims.Add(claim);
                }

                if (!this.SupportedMethods.TryGetValue(supportedMethod, out var existingSupportedSchemes))
                {
                    this.SupportedMethods.Add(supportedMethod, supportedSchemes);
                }
                else
                {
                    Update(existingSupportedSchemes, supportedSchemes);
                }
            }
        }

        public IEnumerable<PermissionsError> ValidateLeastPrivilegePermissions(string permission, PathSet pathSet, string[] leastPrivilegedPermissionSchemes)
        {
            var mismatchedSchemes = ValidateMismatchedSchemes(permission, pathSet, leastPrivilegedPermissionSchemes);
            var duplicateErrors = new HashSet<PermissionsError> ();
            var privs = this.FetchLeastPrivilege();
            foreach (var methodPrivs in privs)
            {
                var method = methodPrivs.Key;
                foreach (var schemePrivs in methodPrivs.Value)
                {
                    var scheme = schemePrivs.Key;
                    if (schemePrivs.Value.Count > 1)
                    {
                        duplicateErrors.Add(new PermissionsError
                        {
                            Path = this.Url,
                            ErrorCode = PermissionsErrorCode.DuplicateLeastPrivilegeScopes,
                            Message = string.Format(StringConstants.DuplicateLeastPrivilegeSchemeErrorMessage, string.Join(", ", schemePrivs.Value), scheme, method),
                        });
                    }
                }
            }
            
            return mismatchedSchemes.Union(duplicateErrors);
        }

        
        private HashSet<PermissionsError> ValidateMismatchedSchemes(string permission, PathSet pathSet, IEnumerable<string> leastPrivilegePermissionSchemes)
        {
            var mismatchedPrivilegeSchemes = leastPrivilegePermissionSchemes.Except(pathSet.SchemeKeys);
            var errors = new HashSet<PermissionsError>();
            if (mismatchedPrivilegeSchemes.Any())
            {
                var invalidSchemes = string.Join(", ", mismatchedPrivilegeSchemes);
                var expectedSchemes = string.Join(", ", pathSet.SchemeKeys);
                errors.Add(new PermissionsError
                {
                    Path = this.Url,
                    ErrorCode = PermissionsErrorCode.InvalidLeastPrivilegeScheme,
                    Message = string.Format(StringConstants.UnexpectedLeastPrivilegeSchemeErrorMessage, invalidSchemes, permission, expectedSchemes),
                });
            }
            return errors;
        }

        private void Update(Dictionary<string, List<AcceptableClaim>> existingSchemes, Dictionary<string, List<AcceptableClaim>> newSchemes)
        {
            
            foreach(var newScheme in newSchemes)
            {
                if (existingSchemes.TryGetValue(newScheme.Key, out var existingScheme))
                {
                    existingScheme.AddRange(newScheme.Value);
                } 
                else
                {
                    existingSchemes[newScheme.Key] = newScheme.Value;
                }
            }
        }

        public void Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("url");
            writer.WriteStringValue(Url);
            writer.WritePropertyName("methods");
            WriteSupportedMethod(writer, this.SupportedMethods);
            
            writer.WriteEndObject();
        }

        private void WriteSupportedMethod(Utf8JsonWriter writer, Dictionary<string, Dictionary<string, List<AcceptableClaim>>> supportedMethods)
        {
            writer.WriteStartObject();
            foreach (var item in supportedMethods)
            {
                writer.WritePropertyName(item.Key);
                WriteSupportedSchemes(writer, item.Value);
            }
            writer.WriteEndObject();
        }

        public void WriteSupportedSchemes(Utf8JsonWriter writer, Dictionary<string, List<AcceptableClaim>> methodClaims)
        {
            writer.WriteStartObject();
            foreach (var item in methodClaims)
            {
                writer.WritePropertyName(item.Key);
                WriteAcceptableClaims(writer, item.Value);
            }
            writer.WriteEndObject();
        }

        public void WriteAcceptableClaims(Utf8JsonWriter writer, List<AcceptableClaim> schemes)
        {
            writer.WriteStartArray();
            foreach (var item in schemes.OrderByDescending(c => c.Least))
            {
                item.Write(writer);
            }
            writer.WriteEndArray();
        }

        public string GeneratePermissionsTable(string method, Dictionary<string, List<AcceptableClaim>> methodClaims)
        {
            var leastPrivilege = this.FetchLeastPrivilege(method);
            var markdownBuilder = new MarkDownBuilder();
            markdownBuilder.StartTable("Permission type", "Least privileged permissions", "Higher privileged permissions");
 
            (var least, var higher) = GetTableScopes("DelegatedWork", methodClaims, leastPrivilege[method]);
            markdownBuilder.AddTableRow("Delegated (work or school account)", least, higher);

            (least, higher) = GetTableScopes("DelegatedPersonal", methodClaims, leastPrivilege[method]);
            markdownBuilder.AddTableRow("Delegated (personal Microsoft account)", least, higher);

            (least, higher) = GetTableScopes("Application", methodClaims, leastPrivilege[method]);
            markdownBuilder.AddTableRow("Application", least, higher);
            
            markdownBuilder.EndTable();
            return markdownBuilder.ToString();
        }

        public Dictionary<string, Dictionary<string, HashSet<string>>> FetchLeastPrivilege(string method = null, string scheme = null)
        {
            var leastPrivilege = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            if (method != null && scheme != null)
            {
                leastPrivilege.TryAdd(method, new Dictionary<string, HashSet<string>>());
                var permissions = this.SupportedMethods[method][scheme].Where(p => p.Least).Select(p => p.Permission).ToHashSet();
                PopulateLeastPrivilege(leastPrivilege, method, scheme, permissions);
            }
            if (method != null && scheme == null)
            {
                this.SupportedMethods.TryGetValue(method, out var supportedSchemes);
                if (supportedSchemes == null)
                {
                    return leastPrivilege;
                }
                foreach (var supportedScheme in supportedSchemes.OrderBy(s => Enum.Parse(typeof(SchemeType), s.Key)))
                {
                    leastPrivilege.TryAdd(method, new Dictionary<string, HashSet<string>>());
                    var permissions = supportedScheme.Value.Where(p => p.Least).Select(p => p.Permission).ToHashSet();
                    PopulateLeastPrivilege(leastPrivilege, method, supportedScheme.Key, permissions);
                }
            }
            if (method == null && scheme != null)
            {
                foreach (var supportedMethod in this.SupportedMethods.OrderBy(s => s.Key))
                {
                    supportedMethod.Value.TryGetValue(scheme, out var supportedSchemeClaims);
                    if (supportedSchemeClaims == null)
                    {
                        continue;
                    }
                    leastPrivilege.TryAdd(supportedMethod.Key, new Dictionary<string, HashSet<string>>());
                    var permissions = supportedSchemeClaims.Where(p => p.Least).Select(p => p.Permission).ToHashSet();
                    PopulateLeastPrivilege(leastPrivilege, supportedMethod.Key, scheme, permissions);
                }
            }
            if (method == null && scheme == null)
            {
                foreach (var supportedMethod in this.SupportedMethods.OrderBy(s => s.Key))
                {
                    foreach (var supportedScheme in supportedMethod.Value.OrderBy(s => Enum.Parse(typeof(SchemeType), s.Key)))
                    {
                        leastPrivilege.TryAdd(supportedMethod.Key, new Dictionary<string, HashSet<string>>());
                        var permissions = supportedScheme.Value.Where(p => p.Least).Select(p => p.Permission).ToHashSet();
                        PopulateLeastPrivilege(leastPrivilege, supportedMethod.Key, supportedScheme.Key, permissions);
                    }
                }
            }
            return leastPrivilege;
        }

        public string WriteLeastPrivilegeTable(Dictionary<string, Dictionary<string, HashSet<string>>> leastPrivilege)
        {
            string output;
            var builder = new StringBuilder();
            foreach (var methodEntry in leastPrivilege)
            {
                builder.AppendLine();
                builder.AppendLine(methodEntry.Key);
                foreach (var schemeEntry in methodEntry.Value)
                {
                    builder.AppendLine($"|{schemeEntry.Key} |{string.Join(";", schemeEntry.Value)}|");
                    builder.AppendLine();
                }
                builder.AppendLine();
            }
            output = builder.ToString();
            return output;
        }
 
        private (string least, string higher) GetTableScopes(string scheme, Dictionary<string, List<AcceptableClaim>> methodClaims, Dictionary<string, HashSet<string>> leastPrivilege)
        {
            var permissionsStub = new List<string>();

            var delegatedWorkScopes = methodClaims.TryGetValue(scheme, out List<AcceptableClaim> claims) ? claims.OrderByDescending(c => c.Least).Select(c => c.Permission) : permissionsStub;
            leastPrivilege.TryGetValue(scheme, out HashSet<string> scopes);
            (var least, var higher) = ExtractScopes(delegatedWorkScopes, scopes);
            return (least, higher);
        }

        private void PopulateLeastPrivilege(Dictionary<string, Dictionary<string, HashSet<string>>> leastPrivilege, string method, string scheme, HashSet<string> permissions)
        {
            if (permissions.Count == 0)
            {
                return;
            }
            leastPrivilege[method][scheme] = Disambiguate(method, scheme, permissions);
        }

        private HashSet<string> Disambiguate(string method, string scheme, HashSet<string> permissions)
        {
            // If more than one permission exists as the least privilege due to grouping of the methods
            if (permissions.Count > 1)
            {
                var exclusivePrivilegeCount = 0;
                foreach (var perm in permissions)
                {
                    if ((this.PermissionMethods.TryGetValue((perm, scheme), out HashSet<string> supportedMethods) && supportedMethods.Count == 1))
                    {
                        exclusivePrivilegeCount++;
                    }
                }
                if (exclusivePrivilegeCount > 1)
                {
                    return permissions;
                }


                // Check for the permission supports the provided method only as the least privilege
                foreach (var perm in permissions)
                {
                    if (!(this.PermissionMethods.TryGetValue((perm, scheme), out HashSet<string> supportedMethods) && supportedMethods.Count == 1))
                    {
                        continue;
                    }
                    if (supportedMethods.First() == method)
                    {
                        return new HashSet<string> { perm };
                    }
                }

                var permissionMethodsCount = 100;
                var leastPrivilegePermission = permissions.First();
                // Check for the permission that supports the fewest number of methods as least privilege
                // TODO: Use permission risk levels once they get added to the model.
                foreach (var perm in permissions)
                {
                    if (!this.PermissionMethods.TryGetValue((perm, scheme), out HashSet<string> supportedMethods))
                    {
                        continue;
                    }
                    if (supportedMethods.Count < permissionMethodsCount && supportedMethods.Contains(method))
                    {
                        leastPrivilegePermission = perm;
                        permissionMethodsCount = supportedMethods.Count;
                    }
                }

                return new HashSet<string> { leastPrivilegePermission };
            }
            return permissions;
        }
        
        private (string least, string higher) ExtractScopes(IEnumerable<string> orderedScopes, HashSet<string> leastPrivilege)
        {
            var least = leastPrivilege != null && leastPrivilege.Any() ? leastPrivilege.First() : "Not supported.";
            var filteredScopes = orderedScopes.Where(s => s!= least);
            var higher = filteredScopes.Any() ? string.Join(", ", filteredScopes) : leastPrivilege != null && leastPrivilege.Any() ? "Not available." : "Not supported.";
            return (least, higher);
        }
    }

}
