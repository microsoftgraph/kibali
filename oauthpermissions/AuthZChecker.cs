using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiPermissions
{
    public class AuthZChecker
    {
        private readonly List<PermissionsDocument> permissionsDocuments = new List<PermissionsDocument>();
        private readonly Dictionary<string, ProtectedResource> resources = new Dictionary<string, ProtectedResource>();


        public Dictionary<string, ProtectedResource> Resources { get
            {
                return resources;
            } 
        }
        public void Load(PermissionsDocument permissionsDocument)
        {
            this.permissionsDocuments.Add(permissionsDocument);
            InvertPermissionsDocument(permissionsDocument);
        }

        private void InvertPermissionsDocument(PermissionsDocument permissionsDocument)
        {
            // Walk permissions, find each pathSet and add path to dictionary
            foreach(var permission in permissionsDocument.Permissions )
            {
                foreach(var pathSet in permission.Value.PathSets) {
                    foreach( var path in pathSet.Paths )
                    {
                        ProtectedResource resource;
                        if (resources.ContainsKey(path.Key))
                        {
                            resource = resources[path.Key];
                            
                        } else
                        {
                            resource = new ProtectedResource(path.Key);
                            resources.Add(path.Key, resource);
                        }
                        resource.AddRequiredClaims(permission.Key,pathSet);
                    }
                }
            }
        }

        public AccessRequestResult CanAccess(string url, string method, string scheme, string[] providedPermissions)
        {
            var resource = FindResource(url);
            if (resource == null)
            {
                return AccessRequestResult.MissingResource; 
            }
            if (!resource.SupportedMethods.TryGetValue(method,out var supportedSchemes)) {
                return AccessRequestResult.UnsupportedMethod;
            }

            if (!supportedSchemes.TryGetValue(scheme, out var acceptableClaims)) {
                return AccessRequestResult.UnsupportedScheme;
            }

            foreach (var claim in acceptableClaims)
            {
                if (claim.IsAuthorized(providedPermissions))
                {
                    return AccessRequestResult.Success;
                }
            }
            return AccessRequestResult.InsufficientPermissions;
        }


        private ProtectedResource FindResource(string url)
        {
            this.resources.TryGetValue(url, out var protectedResource);  // Todo: replace with template matching.
            return protectedResource;
        }
    }

    public enum AccessRequestResult
    {
        Success,
        MissingResource,
        UnsupportedMethod,
        UnsupportedScheme,
        InsufficientPermissions
    }
           

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
        public Dictionary<string, Dictionary<string,List<AcceptableClaim>>> SupportedMethods { get; set; } = new Dictionary<string, Dictionary<string, List<AcceptableClaim>>>();

        public ProtectedResource(string url)
        {
            Url = url;
        }

        public void AddRequiredClaims(string permission, PathSet pathSet)
        {
            foreach (var supportedMethod in pathSet.Methods)
            {
                var supportedSchemes = new Dictionary<string, List<AcceptableClaim>>();
                foreach (var supportedScheme in pathSet.Schemes)
                {
                    if (!supportedSchemes.ContainsKey(supportedScheme))
                    {
                        supportedSchemes.Add(supportedScheme, new List<AcceptableClaim>());
                    }
                    supportedSchemes[supportedScheme].Add(new AcceptableClaim(permission, pathSet.AlsoRequires));
                }
                this.SupportedMethods.Add(supportedMethod, supportedSchemes);
            }
        }
    }

    public class AcceptableClaim
    {
        public AcceptableClaim(string permission, string alsoRequires)
        {
            this.Permission = permission;
            this.AlsoRequires = alsoRequires;
        }
        public string Permission { get; }
        public string AlsoRequires { get;  }

        internal bool IsAuthorized(string[] providedPermissions)
        {
            return providedPermissions.Contains(this.Permission);  //TODO: add support for alsoRequires
        }
    }
}
