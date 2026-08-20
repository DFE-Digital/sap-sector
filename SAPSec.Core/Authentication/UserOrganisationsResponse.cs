using System.Text.Json.Serialization;

namespace SAPSec.Core.Authentication;

public class UserOrganisationsResponse
{
    [JsonPropertyName("organisations")]
    public List<Organisation>? Organisations { get; set; }
}
