using System.Text.Json.Serialization;

namespace SAPSec.Core.Authentication;

public class Region
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
