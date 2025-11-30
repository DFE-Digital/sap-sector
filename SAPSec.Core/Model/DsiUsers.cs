using System.Text.Json.Serialization;

namespace SAPSec.Core.Model;

public class DsiUser
{
    public string Sub { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<DsiOrganisation> Organisations { get; set; } = new();

    public string FullName => $"{GivenName} {FamilyName}";
}
public class DsiRole
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int NumericId { get; set; }
    public int Status { get; set; }
}

public class DsiUserInfo
{
    public string Sub { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string Name => $"{GivenName} {FamilyName}".Trim();
    public List<DsiOrganisation> Organisations { get; set; } = new();
}