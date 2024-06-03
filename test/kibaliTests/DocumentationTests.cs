using Kibali;

namespace KibaliTests;

public class DocumentationTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DocumentationTableGenerated(bool mergeMultiplePaths)
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, "/foo", "GET", true) { MergeMultiplePaths = mergeMultiplePaths};
        var table = generator.GenerateTable().Replace("\r\n", string.Empty).Replace("\n", string.Empty);

        var expectedTable = @"
|Permission type|Least privileged permissions|Higher privileged permissions|
|:---|:---|:---|
|Delegated (work or school account)|Foo.Read|Foo.ReadWrite|
|Delegated (personal Microsoft account)|Not supported.|Not supported.|
|Application|Foo.ReadWrite|Not available.|".Replace("\r\n", string.Empty).Replace("\n", string.Empty);

        Assert.Equal(expectedTable, table);
        
    }


    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DocumentationTableGeneratedLenient(bool mergeMultiplePaths)
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, "/foo/(value={value})", "GET", true, true) { MergeMultiplePaths = mergeMultiplePaths };
        var table = generator.GenerateTable().Replace("\r\n", string.Empty).Replace("\n", string.Empty);

        var expectedTable = @"
|Permission type|Least privileged permissions|Higher privileged permissions|
|:---|:---|:---|
|Delegated (work or school account)|Foo.Read|Foo.ReadWrite|
|Delegated (personal Microsoft account)|Not supported.|Not supported.|
|Application|Foo.ReadWrite|Not available.|".Replace("\r\n", string.Empty).Replace("\n", string.Empty);

        Assert.Equal(expectedTable, table);

    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DocumentationTableNotGeneratedNotLenient(bool mergeMultiplePaths)
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, "/foo/(value={value})", "GET", true) { MergeMultiplePaths = mergeMultiplePaths };
        var table = generator.GenerateTable().Replace("\r\n", string.Empty).Replace("\n", string.Empty);

        var expectedTable = @"
|Permission type|Least privileged permissions|Higher privileged permissions|
|:---|:---|:---|
|Delegated (work or school account)|Not supported.|Not supported.|
|Delegated (personal Microsoft account)|Not supported.|Not supported.|
|Application|Not supported.|Not supported.|".Replace("\r\n", string.Empty).Replace("\n", string.Empty);

        Assert.Equal(expectedTable, table);

    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DocumentationTableNotGeneratedMissingPath(bool mergeMultiplePaths)
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, "/foo/bar", "GET", true) { MergeMultiplePaths = mergeMultiplePaths };
        var table = generator.GenerateTable().Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        var expectedTable = @"
|Permission type|Least privileged permissions|Higher privileged permissions|
|:---|:---|:---|
|Delegated (work or school account)|Not supported.|Not supported.|
|Delegated (personal Microsoft account)|Not supported.|Not supported.|
|Application|Not supported.|Not supported.|".Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        Assert.Equal(expectedTable, table);

    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DocumentationTableNotGeneratedNoMethod(bool mergeMultiplePaths)
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, "/foo", null, true) { MergeMultiplePaths = mergeMultiplePaths };
        var table = generator.GenerateTable().Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        var expectedTable = @"
|Permission type|Least privileged permissions|Higher privileged permissions|
|:---|:---|:---|
|Delegated (work or school account)|Not supported.|Not supported.|
|Delegated (personal Microsoft account)|Not supported.|Not supported.|
|Application|Not supported.|Not supported.|".Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        Assert.Equal(expectedTable, table);

    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DocumentationTableNotGeneratedUnsupportedMethod(bool mergeMultiplePaths)
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, "/foo", "PATCH", true) { MergeMultiplePaths = mergeMultiplePaths };
        var table = generator.GenerateTable().Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        var expectedTable = @"
|Permission type|Least privileged permissions|Higher privileged permissions|
|:---|:---|:---|
|Delegated (work or school account)|Not supported.|Not supported.|
|Delegated (personal Microsoft account)|Not supported.|Not supported.|
|Application|Not supported.|Not supported.|".Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        Assert.Equal(expectedTable, table, true);

    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DocumentationTableNotGeneratedNoPrivilege(bool mergeMultiplePaths)
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, "/fooNoPrivilege", null, true) { MergeMultiplePaths = mergeMultiplePaths };
        var table = generator.GenerateTable().Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        var expectedTable = @"
|Permission type|Least privileged permissions|Higher privileged permissions|
|:---|:---|:---|
|Delegated (work or school account)|Not supported.|Not supported.|
|Delegated (personal Microsoft account)|Not supported.|Not supported.|
|Application|Not supported.|Not supported.|".Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        Assert.Equal(expectedTable, table);

    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DocumentationTableNotGeneratedNoDefault(bool mergeMultiplePaths)
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, "/foo", "PATCH") { MergeMultiplePaths = mergeMultiplePaths };
        var table = generator.GenerateTable().Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        var expectedTable = string.Empty;
        Assert.Equal(expectedTable, table);
        
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DocumentationTableNotGeneratedInvalidRow(bool mergeMultiplePaths)
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, "/fooNoPrivilege", "GET", true) { MergeMultiplePaths = mergeMultiplePaths };
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


    [Fact]
    public void DocumentationTableGeneratedMultiplePaths()
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, " /foo/(value={value});/fooOther", "GET", true, true) { MergeMultiplePaths = true };
        var table = generator.GenerateTable();
        var actualTable = table.Replace("\r\n", string.Empty).Replace("\n", string.Empty);
        var expectedTable = @"
|Permission type|Least privileged permissions|Higher privileged permissions|
|:---|:---|:---|
|Delegated (work or school account)|Foo.Read|Foo.ReadWrite|
|Delegated (personal Microsoft account)|Not supported.|Not supported.|
|Application|Foo.ReadWrite|Not available.|".Replace("\r\n", string.Empty).Replace("\n", string.Empty);

        Assert.Equal(expectedTable, actualTable);

    }

    [Fact]
    public void DocumentationTableThrowsExceptionMultiplePathsMultipleLPP()
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, "/fooOther;/fooBar", "GET", true, true) { MergeMultiplePaths = true };
        var generateTable = () => generator.GenerateTable();
        var exception = Assert.Throws<ArgumentException>(generateTable);
        Assert.Equal("Differing least privilege permissions Bar.ReadWrite,Foo.ReadWrite for the scheme Application.", exception.Message);

    }

    [Fact]
    public void DocumentationTableGeneratedMultiplePathsMultipleHPP()
    {
        var permissionsDocument = CreatePermissionsDocument();

        var generator = new PermissionsStubGenerator(permissionsDocument, "/bar;/fooBar", "GET", true, true) { MergeMultiplePaths = true };
        var table = generator.GenerateTable();
        var actualTable = table.Replace("\r\n", string.Empty).Replace("\n", string.Empty);

        var expectedTable = @"
|Permission type|Least privileged permissions|Higher privileged permissions|
|:---|:---|:---|
|Delegated (work or school account)|Bar.ReadWrite|Foo.Read, Foo.ReadWrite|
|Delegated (personal Microsoft account)|Foo.Read|Not available.|
|Application|Bar.ReadWrite|Foo.ReadWrite|".Replace("\r\n", string.Empty).Replace("\n", string.Empty);

        Assert.Equal(expectedTable, actualTable);
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
                                { "/foo",  "least=DelegatedWork" },
                                { "/bar",  "" }
                            }
                        },
                        new PathSet() {
                            Methods = {
                                "GET"
                            },
                            SchemeKeys = {
                                "DelegatedPersonal"
                            },
                            Paths = {
                                { "/bar",  "least=DelegatedPersonal" }
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
                                { "/fooNoPrivilege",  "" },
                                { "/fooOther",  "least=Application" },
                                { "/bar",  "" }
                            }
                        }
                    }
        };
        var barReadWrite = new Permission
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
                                { "/fooBar",  "least=Application,DelegatedWork" },
                                { "/bar",  "" }
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
