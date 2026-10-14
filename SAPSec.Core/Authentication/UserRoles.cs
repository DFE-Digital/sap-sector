namespace SAPSec.Core.Authentication;

/// <summary>
/// Role claim values used for authorization.
/// </summary>
public static class UserRoles
{
    /// <summary>
    /// DfE staff, who can search for and view any school. DfE users are expected to sign in
    /// through Entra ID with this as an app role; no DfE Sign-in user currently has it.
    /// </summary>
    public const string DfeUser = "DfEUser";
}
