﻿using Microsoft.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Services;
using Microsoft.OpenApi.Writers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kibali
{
    public class AuthZChecker
    {
        private readonly Dictionary<string, ProtectedResource> resources = new Dictionary<string, ProtectedResource>();
        private OpenApiUrlTreeNode urlTree;

        public Dictionary<string, ProtectedResource> Resources { get
            {
                return resources;
            }
        }

        public bool LenientMatch { get; set; }
        public void Load(PermissionsDocument permissionsDocument)
        {
            InvertPermissionsDocument(permissionsDocument);
        }

        public ProtectedResource Lookup(string url)
        {
            var parsedUrl = new Uri(new Uri("https://example.org/"), url, true);
            var segments = parsedUrl.AbsolutePath.Split("/").Skip(1);
            return Find(UrlTree, segments);
        }

        public ProtectedResource FindResource(string url)
        {
            if (LenientMatch)
            {
                url = CleanRequestUrl(url);
                return Lookup(url);
            }
            else
            {
                return Lookup(url);
            }
        }

        public IEnumerable<AcceptableClaim> GetRequiredPermissions(string url, string method, string scheme)
        {
            var resource = FindResource(url);
            if (resource == null)
            {
                return new List<AcceptableClaim>();
            }
            if (!resource.SupportedMethods.TryGetValue(method, out var supportedSchemes))
            {
                return new List<AcceptableClaim>();
            }

            if (!supportedSchemes.TryGetValue(scheme, out var acceptableClaims))
            {
                return new List<AcceptableClaim>();
            }

            return acceptableClaims;
        }

        public AccessRequestResult CanAccess(string url, string method, string scheme, string[] providedPermissions)
        {
            var resource = FindResource(url);
            if (resource == null)
            {
                return AccessRequestResult.MissingResource;
            }
            if (!resource.SupportedMethods.TryGetValue(method, out var supportedSchemes)) {
                return AccessRequestResult.UnsupportedMethod;
            }

            if (!supportedSchemes.TryGetValue(scheme, out var acceptableClaims)) {
                return AccessRequestResult.UnsupportedScheme;
            }

            if (acceptableClaims.Any(claim => claim.IsAuthorized(providedPermissions)))
            {
                return AccessRequestResult.Success;
            }

            return AccessRequestResult.InsufficientPermissions;
        }

        public HashSet<PermissionsError> Validate(PermissionsDocument permissionsDocument)
        {
            return InvertPermissionsDocument(permissionsDocument, validate: true);
        }

        private HashSet<PermissionsError> InvertPermissionsDocument(PermissionsDocument permissionsDocument, bool validate = false)
        {
            // Walk permissions, find each pathSet and add path to dictionary
            var errors = new HashSet<PermissionsError>();
            foreach (var permission in permissionsDocument.Permissions)
            {
                foreach (var pathSet in permission.Value.PathSets)
                {
                    foreach (var path in pathSet.Paths)
                    {
                        var pathKey = this.LenientMatch ? CleanRequestUrl(path.Key) : path.Key;
                        if (!resources.TryGetValue(pathKey, out var resource))
                        {
                            resource = new ProtectedResource(pathKey);
                            resources.Add(pathKey, resource);
                        }
                        var leastPrivilegedPermissionSchemes = ParseLeastPrivilegeSchemes(path.Value);
                        resource.AddRequiredClaims(permission.Key, pathSet, leastPrivilegedPermissionSchemes);
                        if (validate)
                        {
                            errors.UnionWith(resource.ValidateLeastPrivilegePermissions(permission.Key, pathSet, leastPrivilegedPermissionSchemes));
                        }
                    }
                }
            }
            return errors;
        }

        private ProtectedResource Find(OpenApiUrlTreeNode urlTree, IEnumerable<string> segments)
        {
            
            var segment = segments.FirstOrDefault();
            if (string.IsNullOrEmpty(segment))
            {
                return (urlTree.PathItems.First().Value.Extensions["x-permissions"] as OpenApiProtectedResource).Resource;  // Can the root have a permission?
            }

            if (urlTree.Children.TryGetValue(segment, out var urlTreeNode))
            {
                return Find(urlTreeNode, segments.Skip(1));
            }
            else
            {
                var parameterSegment = urlTree.Children.Where(k => k.Key.StartsWith("{")).FirstOrDefault();
                if (parameterSegment.Key == null) return null;
                return Find(parameterSegment.Value, segments: segments.Skip(1));
            }
        }

        private OpenApiUrlTreeNode UrlTree
        {
            get
            {
                if (urlTree == null)
                {
                    urlTree = CreateUrlTree(this.resources);
                }
                return urlTree;
            }
        }

        private OpenApiUrlTreeNode CreateUrlTree(Dictionary<string, ProtectedResource> resources)
        {
            var tree = OpenApiUrlTreeNode.Create();

            foreach (var resource in resources)
            {
                var pathItem = new OpenApiPathItem();

                var openApiResource = new OpenApiProtectedResource(resource.Value);
                pathItem.AddExtension("x-permissions", openApiResource);
                
                //foreach (var method in resource.Value.SupportedMethods)
                //{
                //    var op = new OpenApiOperation();
                //    var sr = new OpenApiSecurityRequirement();

                //    foreach (var scheme in method.Value)
                //    {
                //        sr[new OpenApiSecurityScheme() { Name = scheme.Key }] = scheme.Value.Select(ac => ac.Permission).ToArray();

                //    }
                //    op.Security = new List<OpenApiSecurityRequirement>() { sr };

                //    pathItem.Operations.Add(GetOperationTypeFromMethod(method.Key), new OpenApiOperation());
                //}
                tree.Attach(resource.Key, pathItem, "!");
            }

            return tree;
        }
        private static string[] ParseLeastPrivilegeSchemes(string pathValue)
        {
            var defaultLeastPrivilege = Array.Empty<string>();
            if (string.IsNullOrEmpty(pathValue))
            {
                return defaultLeastPrivilege;
            }
            var parsedPathValue = ParsingHelpers.ParseProperties(pathValue);
            parsedPathValue.TryGetValue("least", out string privilegeString);
            var leastPrivilegedPermissionSchemes = privilegeString != null ? privilegeString.Split(",") : defaultLeastPrivilege;
            return leastPrivilegedPermissionSchemes;
        }

        private static string CleanRequestUrl(string requestUrl)
        {
            if (string.IsNullOrEmpty(requestUrl))
            {
                return requestUrl;
            }

            var parensRemoved = Regex.Replace(requestUrl.ToLowerInvariant(), @"\/\(.*?\)", string.Empty, RegexOptions.None, TimeSpan.FromSeconds(5)).Replace(@"//", "/");
            var braceValuesReplaced = Regex.Replace(parensRemoved, @"\{.*?\}", "{id}", RegexOptions.None, TimeSpan.FromSeconds(5));
            return braceValuesReplaced;
        }
    }

    public class OpenApiProtectedResource : IOpenApiExtension, IOpenApiAny
    {
        public OpenApiProtectedResource(ProtectedResource resource)
        {
            Resource = resource;
        }

        public ProtectedResource Resource { get; }

        public AnyType AnyType => AnyType.Object;

        public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
        {
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

    public enum PermissionsErrorCode
    {
        DuplicateLeastPrivilegeScopes,
        InvalidLeastPrivilegeScheme,
    }
}
