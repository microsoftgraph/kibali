using Kibali;
using Xunit.Abstractions;

namespace KibaliTests;

public class ValidationTests
{
    private readonly ITestOutputHelper _output;
    public ValidationTests(ITestOutputHelper output)
    {
        _output = output;
    }
    [Fact]
    public void ValidateSinglePermissionsFileIsValid()
    {
        // Arrange
        var doc = PermissionsDocument.Load(new FileStream("ValidUser.json", FileMode.Open));

        // Act
        var authZChecker = new AuthZChecker();
        authZChecker.Validate(doc);
        
        // Assert
        Assert.False(authZChecker.ContainsErrors);
    }
    [Fact]
    public void ValidateSinglePermissionFileIsNotvalid()
    {
        // Arrange
        var doc = PermissionsDocument.Load(new FileStream("InValidUser.json", FileMode.Open));
        
        // Act
        var authZChecker = new AuthZChecker();
        authZChecker.Validate(doc);

        // Assert
        Assert.True(authZChecker.ContainsErrors);
        var actualErrors = authZChecker.Errors;
        Assert.True(actualErrors.Count == 2);
        Assert.Contains(PermissionsErrorCode.DuplicateLeastPrivilegeScopes, actualErrors.Select(e => e.ErrorCode));
        Assert.Contains(PermissionsErrorCode.InvalidLeastPrivilegeScheme, actualErrors.Select(e => e.ErrorCode));
        Assert.Contains("/me", actualErrors.Select(e => e.Path));
        Assert.Contains("/me/createdobjects", actualErrors.Select(e => e.Path)); 
    }
    [Fact]
    public void ValidateFolderIsValid()
    {
        // Arrange
        var doc = PermissionsDocument.LoadAndMerge("ValidPermissions");

        // Act
        var authZChecker = new AuthZChecker();
        authZChecker.Validate(doc);

        // Assert
        Assert.False(authZChecker.ContainsErrors);
    }
    [Fact]
    public void ValidateFolderIsNotValid()
    {
        // Arrange
        var doc = PermissionsDocument.LoadAndMerge("InvalidPermissions");

        // Act
        var authZChecker = new AuthZChecker();
        authZChecker.Validate(doc);

        // Assert
        Assert.True(authZChecker.ContainsErrors);
        var actualErrors = authZChecker.Errors;
        Assert.True(actualErrors.Count == 3);
        Assert.Contains(PermissionsErrorCode.DuplicateLeastPrivilegeScopes, actualErrors.Select(e => e.ErrorCode));
        Assert.Contains(PermissionsErrorCode.InvalidLeastPrivilegeScheme, actualErrors.Select(e => e.ErrorCode));
        Assert.Contains("/me", actualErrors.Select(e => e.Path));
        Assert.Contains("/users/{id}/licensedetails", actualErrors.Select(e => e.Path));
    }
   
}
