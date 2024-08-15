namespace Kibali;

public class StringConstants
{
    internal const string UnexpectedLeastPrivilegeSchemeErrorMessage = "Unexpected Least Privilege Scheme '{0}' for scope '{1}'. Expected Schemes: {2}";

    internal const string DuplicateLeastPrivilegeSchemeErrorMessage = "Duplicate Least Privilege Scopes {0} for Scheme '{1}' for Method '{2}' ";

    internal const string PermissionNotSupported = "Not supported.";

    internal const string PermissionNotAvailable = "Not available.";
    
    internal const string DuplicatePathsetEntryErrorMessage = "Duplicate pathset entry. Permission: '{0}' Path: '{1}' Scheme: '{2}' Method: '{3}' ";
}
