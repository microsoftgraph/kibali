﻿using Kibali;

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
|Application|Foo.ReadWrite|Not supported.|".Replace("\r\n", string.Empty).Replace("\n", string.Empty);

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
                        }
                    }
        };
        permissionsDocument.Permissions.Add("Foo.Read", fooRead);
        permissionsDocument.Permissions.Add("Foo.ReadWrite", fooReadWrite);
        return permissionsDocument;
    }
}
