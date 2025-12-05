using System.Text.Json.Serialization;

namespace SAPSec.Core.Model;

public class Status
{
    [JsonPropertyName("id")]
    public int Id { get; set; }  

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("tagColor")]
    public string? TagColor { get; set; } 
}

