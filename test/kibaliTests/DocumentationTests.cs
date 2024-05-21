using Kibali;

namespace KibaliTests;

public class DocumentationTests
{
    [Fact]
    public void DocumentationTableGenerated()
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, "/foo", "GET", true);
        var table = generator.GenerateTable().Replace("\r\n", string.Empty).Replace("\n", string.Empty);

        var expectedTable = @"
|Permission type|Least privileged permissions|Higher privileged permissions|
|:---|:---|:---|
|Delegated (work or school account)|Foo.Read|Foo.ReadWrite|
|Delegated (personal Microsoft account)|Not supported.|Not supported.|
|Application|Foo.ReadWrite|Not available.|".Replace("\r\n", string.Empty).Replace("\n", string.Empty);

        Assert.Equal(expectedTable, table);
        
    }


    [Fact]
    public void DocumentationTableGeneratedLenient()
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, "/foo/(value={value})", "GET", true, true);
        var table = generator.GenerateTable().Replace("\r\n", string.Empty).Replace("\n", string.Empty);

        var expectedTable = @"
|Permission type|Least privileged permissions|Higher privileged permissions|
|:---|:---|:---|
|Delegated (work or school account)|Foo.Read|Foo.ReadWrite|
|Delegated (personal Microsoft account)|Not supported.|Not supported.|
|Application|Foo.ReadWrite|Not available.|".Replace("\r\n", string.Empty).Replace("\n", string.Empty);

        Assert.Equal(expectedTable, table);

    }

    [Fact]
    public void DocumentationTableNotGeneratedNotLenient()
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, "/foo/(value={value})", "GET", true);
        var table = generator.GenerateTable().Replace("\r\n", string.Empty).Replace("\n", string.Empty);

        var expectedTable = @"
|Permission type|Least privileged permissions|Higher privileged permissions|
|:---|:---|:---|
|Delegated (work or school account)|Not supported.|Not supported.|
|Delegated (personal Microsoft account)|Not supported.|Not supported.|
|Application|Not supported.|Not supported.|".Replace("\r\n", string.Empty).Replace("\n", string.Empty);

        Assert.Equal(expectedTable, table);

    }

    [Fact]
    public void DocumentationTableNotGeneratedMissingPath()
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, "/foo/bar", "GET", true);
        var table = generator.GenerateTable().Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        var expectedTable = @"
|Permission type|Least privileged permissions|Higher privileged permissions|
|:---|:---|:---|
|Delegated (work or school account)|Not supported.|Not supported.|
|Delegated (personal Microsoft account)|Not supported.|Not supported.|
|Application|Not supported.|Not supported.|".Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        Assert.Equal(expectedTable, table);

    }

    [Fact]
    public void DocumentationTableNotGeneratedNoMethod()
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, "/foo", null, true);
        var table = generator.GenerateTable().Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        var expectedTable = @"
|Permission type|Least privileged permissions|Higher privileged permissions|
|:---|:---|:---|
|Delegated (work or school account)|Not supported.|Not supported.|
|Delegated (personal Microsoft account)|Not supported.|Not supported.|
|Application|Not supported.|Not supported.|".Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        Assert.Equal(expectedTable, table);

    }

    [Fact]
    public void DocumentationTableNotGeneratedUnsupportedMethod()
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, "/foo", "PATCH", true);
        var table = generator.GenerateTable().Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        var expectedTable = @"
|Permission type|Least privileged permissions|Higher privileged permissions|
|:---|:---|:---|
|Delegated (work or school account)|Not supported.|Not supported.|
|Delegated (personal Microsoft account)|Not supported.|Not supported.|
|Application|Not supported.|Not supported.|".Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        Assert.Equal(expectedTable, table, true);

    }

    [Fact]
    public void DocumentationTableNotGeneratedNoPrivilege()
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, "/fooNoPrivilege", null, true);
        var table = generator.GenerateTable().Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        var expectedTable = @"
|Permission type|Least privileged permissions|Higher privileged permissions|
|:---|:---|:---|
|Delegated (work or school account)|Not supported.|Not supported.|
|Delegated (personal Microsoft account)|Not supported.|Not supported.|
|Application|Not supported.|Not supported.|".Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        Assert.Equal(expectedTable, table);

    }

    [Fact]
    public void DocumentationTableNotGeneratedNoDefault()
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, "/foo", "PATCH");
        var table = generator.GenerateTable().Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        var expectedTable = string.Empty;
        Assert.Equal(expectedTable, table);
        
    }

    [Fact]
    public void DocumentationTableNotGeneratedInvalidRow()
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, "/fooNoPrivilege", "GET", true);
        var table = generator.GenerateTable().Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        var expectedTable = string.Empty;
        Assert.Equal(expectedTable, table);
    }

    [Fact]
    public void DocumentationTableGeneratedMultiplePermissions()
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, "/foo", "DELETE", true, true);
        var generatedTable = generator.GenerateTable();
        var table = generatedTable.Replace("\r\n", string.Empty).Replace("\n", string.Empty);

        var expectedTable = @"
|Permission type|Least privileged permissions|Higher privileged permissions|
|:---|:---|:---|
|Delegated (work or school account)|Bar.ReadWrite.OwnedBy and Foo.ReadWrite|Not available.|
|Delegated (personal Microsoft account)|Not supported.|Not supported.|
|Application|Bar.ReadWrite.OwnedBy and Foo.ReadWrite|Bar.ReadWrite.OwnedBy and Baz.ReadWrite, Bar.ReadWrite|".Replace("\r\n", string.Empty).Replace("\n", string.Empty);

        Assert.Equal(expectedTable, table);

    }

    private static PermissionsDocument CreatePermissionsDocument()
    {
        var permissionsDocument = new PermissionsDocument();

        var fooRead = new Permission
        {
            PathSets = {
                        new PathSet() {
                            Methods = {
                                "GET"
                            },
                            SchemeKeys = {
                                "DelegatedWork"
                            },
                            Paths = {
                                { "/foo",  "least=DelegatedWork" }
                            }
                        }
                    }
        };
        var fooReadWrite = new Permission
        {
            PathSets = {
                        new PathSet() {
                            Methods = {
                                "GET",
                                "POST"
                            },
                            SchemeKeys = {
                                "Application",
                                "DelegatedWork"
                            },
                            Paths = {
                                { "/foo",  "least=Application" },
                                { "/fooNoPrivilege",  "" }
                            }
                        },
                        new PathSet() {
                            Methods = {
                                "DELETE"
                            },
                            SchemeKeys = {
                                "Application",
                                "DelegatedWork"
                            },
                            Paths = {
                                { "/foo",  "least=DelegatedWork,Application;AlsoRequires=Bar.ReadWrite.OwnedBy" },
                            }
                        }
                    }
        };

        var barReadWriteOwnedBy = new Permission
        {
            PathSets = {
                        new PathSet() {
                            Methods = {
                                "DELETE"
                            },
                            SchemeKeys = {
                                "Application",
                                "DelegatedWork"
                            },
                            Paths = {
                                { "/foo",  "least=DelegatedWork,Application;AlsoRequires=Foo.ReadWrite" },
                            }
                        }
                    }
        };
        var barReadWrite = new Permission
        {
            PathSets = {
                        new PathSet() {
                            Methods = {
                                "DELETE"
                            },
                            SchemeKeys = {
                                "Application",
                            },
                            Paths = {
                                { "/foo",  "" },
                            }
                        }
                    }
        };

        var bazReadWrite = new Permission
        {
            PathSets = {
                        new PathSet() {
                            Methods = {
                                "DELETE"
                            },
                            SchemeKeys = {
                                "Application",
                            },
                            Paths = {
                                { "/foo",  "AlsoRequires=Bar.ReadWrite.OwnedBy" },
                            }
                        }
                    }
        };
        permissionsDocument.Permissions.Add("Foo.Read", fooRead);
        permissionsDocument.Permissions.Add("Foo.ReadWrite", fooReadWrite);
        permissionsDocument.Permissions.Add("Bar.ReadWrite.OwnedBy", barReadWriteOwnedBy);
        permissionsDocument.Permissions.Add("Bar.ReadWrite", barReadWrite);
        permissionsDocument.Permissions.Add("Baz.ReadWrite", bazReadWrite);
        return permissionsDocument;
    }
}
