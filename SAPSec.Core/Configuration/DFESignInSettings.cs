namespace SAPSec.Core.Configuration;

public class DFESignInSettings
{
    public const string SectionName = "DFESignInSettings";

    public string? APIUri { get; set; }

    public string? Audience { get; set; }

    public string CallbackPath { get; set; } = "/signin-oidc";

    public string ClientID { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string? MetadataAddress { get; set; }

    public string SignedOutCallbackPath { get; set; } = "/signout-callback-oidc";

    public string? SignOutUri { get; set; }

    public string? SignInUri { get; set; }

    public bool IsConfigured()
    {
        return !string.IsNullOrEmpty(ClientID) &&
               !string.IsNullOrEmpty(ClientSecret) &&
               !string.IsNullOrEmpty(Issuer);
    }
}
