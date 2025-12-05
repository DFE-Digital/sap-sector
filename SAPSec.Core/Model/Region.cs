using System.Text.Json.Serialization;

namespace SAPSec.Core.Model;

public class Region
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
