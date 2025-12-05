namespace SAPSec.Core.Model;

public class UserInfo
{
    public string Sub { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string Name => $"{GivenName} {FamilyName}".Trim();
    public List<Organisation> Organisations { get; set; } = new();
}