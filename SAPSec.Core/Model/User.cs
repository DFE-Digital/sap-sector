using System.Text.Json.Serialization;

namespace SAPSec.Core.Model;

public class User
{
    public string Sub { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<Organisation> Organisations { get; set; } = new();

    public string FullName => $"{GivenName} {FamilyName}";
}