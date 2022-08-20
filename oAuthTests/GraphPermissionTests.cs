using ApiPermissions;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace oAuthTests
{
    public class GraphPermissionTests
    {

        private readonly ITestOutputHelper _output;
        private PermissionsDocument graphPermissions;
        private PermissionsDocument userAuthPermissions;
        public GraphPermissionTests(ITestOutputHelper output)
        {
            _output = output;
            graphPermissions = PermissionsDocument.Load(new FileStream("GraphPermissions.json", FileMode.Open));
            userAuthPermissions = PermissionsDocument.Load(new FileStream("UserAuthenticationMethod.json", FileMode.Open));
        }

        [Fact]
        public void GraphFindMailReadAll()
        {

            var authZChecker = new AuthZChecker();
            authZChecker.Load(graphPermissions);

            var resource = authZChecker.FindResource("/me/messages");

            Assert.True(resource.SupportedMethods.ContainsKey("GET"));
            Assert.True(resource.SupportedMethods.ContainsKey("POST"));
            Assert.Contains(resource.SupportedMethods["GET"]["DelegatedWork"], ac => ac.Permission == "Mail.Read");
        }

        [Fact]
        public void GraphFindUSerAuthenticationMethods()
        {
            var authZChecker = new AuthZChecker();
            authZChecker.Load(userAuthPermissions);

            var resource = authZChecker.FindResource("/users/sdfsdfsdfsdf/authentication/microsoftauthenticatormethods");
            
            Assert.True(resource.SupportedMethods.ContainsKey("GET"));
            Assert.Contains(resource.SupportedMethods["GET"]["DelegatedWork"], ac => ac.Permission == "UserAuthenticationMethod.ReadWrite");
        }

        [Fact]
        public void GraphFindUSerAuthenticationMethods2()
        {
            var authZChecker = new AuthZChecker();
            authZChecker.Load(graphPermissions);

            var resource = authZChecker.FindResource("/users/sdfsdfsdfsdf/authentication/microsoftauthenticatormethods");

            Assert.True(resource.SupportedMethods.ContainsKey("GET"));
            Assert.Contains(resource.SupportedMethods["GET"]["DelegatedWork"], ac => ac.Permission == "UserAuthenticationMethod.ReadWrite");
        }

        [Fact]
        public void GraphFindPerfTest()
        {
            var authZChecker = new AuthZChecker();
            authZChecker.Load(graphPermissions);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < 1000; i++)
            {
                var resource = authZChecker.FindResource("/me/messages");
                Assert.NotNull(resource);
                resource = authZChecker.FindResource("/users/sdfsdfsdfsdf/authentication/microsoftauthenticatormethods");
                Assert.NotNull(resource);
                resource = authZChecker.FindResource("/admin/windows/updates/resourceconnections/microsoft.graph.windowsupdates.operationalinsightsconnection");
                Assert.NotNull(resource);
            }
            _output.WriteLine(stopwatch.ElapsedMilliseconds.ToString());


        }

        private PermissionsDocument CreatePermissionsDocument()
        {
            var permissionsDocument = new PermissionsDocument();

            var fooRead = new Permission
            {
                PathSets = {
                        new PathSet() {
                            Methods = {
                                "GET"
                            },
                            Paths = {
                                { "/foo",  null },
                                { "/bar",  null },
                                { "/bar/{id}",  null },
                                { "/bar/{id}/schmo",  null }
                            }
                        }
                    }
            };
            permissionsDocument.Permissions.Add("Foo.Read", fooRead);
            return permissionsDocument;
        }
    }
}
