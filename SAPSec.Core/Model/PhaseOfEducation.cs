using System.Text.Json.Serialization;

namespace SAPSec.Core.Model;

public class PhaseOfEducation
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}