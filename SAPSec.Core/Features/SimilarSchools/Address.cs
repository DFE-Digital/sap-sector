namespace SAPSec.Core.Features.SimilarSchools;

public record Address
{
    public required string Street { get; set; }
    public required string? Locality { get; set; }
    public required string? Address3 { get; set; }
    public required string Town { get; set; }
    public required string Postcode { get; set; }
}
