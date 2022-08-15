using ApiPermissions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace oauthpermissions
{
    public static class CsdlExporter
    {
        public static void Export(StringBuilder sb, PermissionsDocument permissions)
        {
            Export(new StringWriter(sb), permissions);
        }

        public static void Export(string fileName, PermissionsDocument permissions)
        {
            var stream = File.CreateText(fileName);
            Export(stream,permissions);
        }

        public static void Export(Stream stream, PermissionsDocument permissions)
        {
            Export(new StreamWriter(stream),permissions);
        }

        public static void Export(TextWriter writer, PermissionsDocument permissionsDocument)
        {
            var xmlWriter = XmlWriter.Create(writer);
            xmlWriter.WriteStartDocument();

            xmlWriter.WriteStartElement("Schema");

                xmlWriter.WriteStartElement("Annotations");
                    xmlWriter.WriteAttributeString("Target", "microsoft.graph.GraphService");
                    // Write out all permissions info...
                    foreach (var permission in permissionsDocument.Permissions)
                    {
                        WritePermissionAnnotations(xmlWriter, permission);
                    }
                xmlWriter.WriteEndElement();

            var authZChecker = new AuthZChecker();
            authZChecker.Load(permissionsDocument);

            
            // Write out all permissions info...
            foreach (var resource in authZChecker.Resources)
            {
                WriteResourceAnnotations(xmlWriter, resource);
            }

            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndDocument();

        }

        private static void WriteResourceAnnotations(XmlWriter xmlWriter, KeyValuePair<string, ProtectedResource> resource)
        {
            xmlWriter.WriteStartElement("Annotations");
            xmlWriter.WriteAttributeString("Target", UrlToTarget(resource.Value.Url));
            xmlWriter.WriteEndElement();

        }

        private static string UrlToTarget(string url)
        {
            var target = urlToTarget.Replace(url, "");
            return "microsoft.graph"+target;
        }
        private static Regex urlToTarget = new Regex("/{[^/]*");

        private static void WritePermissionAnnotations(XmlWriter xmlWriter, KeyValuePair<string, Permission> permission)
        {
            //Todo
        }
    }
}
