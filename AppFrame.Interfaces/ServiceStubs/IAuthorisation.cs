namespace AppFrame.Interfaces;

public interface IAuthorisation
{   
    ITable<PrincipalData> table {get;} // Dependency on ITable to retrieve user account data
    Task<OAuthToken> AuthenticatePrincipalAsync(string userPrincipalName, string AuthKey);
    Task UpsertPrincipalAsync(PrincipalData principalData);
    Task<PrincipalData> GetPrincipalDataAsync(string userPrincipalName);
    Task<bool> ValidateTokenAsync(OAuthToken token);
    Task<bool> ValidatePermissionAsync(OAuthToken token, string scopeToCheck, string permissionValue);
    
}

public record OAuthToken(string IssuingTenant, string Upn, PrincipalAccountType PrincipalType, string PrincipalTenant, string ResourceTenant, string FullName, string Location, int ExpiresIn, IEnumerable<string> Scopes, IEnumerable<string> Groups, IEnumerable<string> Roles, DateTime IssuedAt, string Signature, Dictionary<string, string> Headers, Dictionary<string, string> Internals);

public record PrincipalData(string UserPrincipalName, PrincipalAccountType PrincipalType, string Tenant, IEnumerable<string> AuthKeys, IEnumerable<string> Scopes, IEnumerable<string> Groups, IEnumerable<string> Roles){}

public enum PrincipalAccountType 
{
    InteractiveUser,
    BackgroundService
}
