namespace SAPSec.Web.ViewModels;

public record DemoSimilarSchoolResultViewModel
{
    public required string URN { get; set; }
    public required string Name { get; set; }
    public required string Street { get; set; }
    public required string Locality { get; set; }
    public required string Address3 { get; set; }
    public required string Town { get; set; }
    public required string Postcode { get; set; }
    public required string LAName { get; set; }
    public required string? Latitude { get; set; }
    public required string? Longitude { get; set; }
    public required string? UrbanRuralId { get; set; }
    public required string? UrbanRuralName { get; set; }
    public required string SortValue { get; set; }
}