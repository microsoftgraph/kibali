using Kibali;
using Xunit.Abstractions;

namespace KibaliTests;

public class LeastPrivilegeTests
{
    private readonly ITestOutputHelper _output;
    public LeastPrivilegeTests(ITestOutputHelper output)
    {
        _output = output;
    }
    [Fact]
    public void FindLeastPrivilege()
    {
        // Arrange
        var authZChecker = new AuthZChecker();
        authZChecker.Load(CreatePermissionsDocument());

        // Act
        var resource = authZChecker.FindResource("/bar");
        var leastPrivilege = resource.FetchLeastPrivilege().Replace("\r\n", string.Empty).Replace("\n", string.Empty);

        // Assert
        var expected = @"
GET
|DelegatedPersonal |Foo.Read|
|Application |Foo.Read|
POST
|DelegatedPersonal |Foo.Read|
|Application |Foo.Read|
".Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        Assert.Equal(expected, leastPrivilege);
    }

    [Fact]
    public void FindLeastPrivilegeMethodProvided()
    {
        // Arrange
        var authZChecker = new AuthZChecker();
        authZChecker.Load(CreatePermissionsDocument());

        // Act
        var resource = authZChecker.FindResource("/bar");
        var leastPrivilege = resource.FetchLeastPrivilege("GET").Replace("\r\n", string.Empty).Replace("\n", string.Empty);

        // Assert
        var expected = @"
GET
|DelegatedPersonal |Foo.Read|
|Application |Foo.Read|
".Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        Assert.Equal(expected, leastPrivilege);
    }

    [Fact]
    public void FindLeastPrivilegeSchemeProvided()
    {
        // Arrange
        var authZChecker = new AuthZChecker();
        authZChecker.Load(CreatePermissionsDocument());

        // Act
        var resource = authZChecker.FindResource("/bar");
        var leastPrivilege = resource.FetchLeastPrivilege(null, "Application").Replace("\r\n", string.Empty).Replace("\n", string.Empty);

        // Assert
        var expected = @"
GET
|Application |Foo.Read|
POST
|Application |Foo.Read|".Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        Assert.Equal(expected, leastPrivilege);
    }

    private PermissionsDocument CreatePermissionsDocument()
    {
        var permissionsDocument = new PermissionsDocument();

        var fooRead = new Permission
        {
            PathSets = {
                        new PathSet() {
                            SchemeKeys = {
                                "DelegatedWork","Application"
                            },
                            Methods = {
                                "GET"
                            },
                            Paths = {
                                { "/foo",  "least=DelegatedWork,Application" },
                                { "/bar",  null },
                                { "/bar/{id}",  null },
                                { "/bar/{id}/schmo",  null }
                            }
                        },
                        new PathSet() {
                            SchemeKeys = {
                                "DelegatedPersonal","Application"
                            },
                            Methods = {
                                "GET","POST"
                            },
                            Paths = {
                                { "/bar",  "least=DelegatedPersonal,Application" },
                                { "/bar/{id}",  null },
                                { "/bar/{id}/schmo",  null }
                            }

                        }
                    }
        };
        permissionsDocument.Permissions.Add("Foo.Read", fooRead);

        var barRead = new Permission
        {
            PathSets = {
                        new PathSet() {
                            SchemeKeys = {
                                "DelegatedPersonal"
                            },
                            Methods = {
                                "GET"
                            },
                            Paths = {
                                { "/bar/{id}",  null },
                            }
                        }
                    }
        };
        permissionsDocument.Permissions.Add("Bar.Read", barRead);
        return permissionsDocument;
    }
}
