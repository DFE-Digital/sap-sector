using System.Text.Json.Serialization;

namespace SAPSec.Core.Model;

public class UserOrganisationsResponse
{
    [JsonPropertyName("organisations")]
    public List<Organisation>? Organisations { get; set; }
}
