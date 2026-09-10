namespace SAPSec.Web.Areas.Shared.ViewModels.SimilarSchools;

public class SimilarSchoolViewModel
{
    public required string Urn { get; init; }
    public required string Name { get; init; }
    public required string LocalAuthorityName { get; init; }
    public required string FullAddress { get; init; }
    public required string? Latitude { get; init; }
    public required string? Longitude { get; init; }
    public required string SortMetricName { get; init; }
    public required string SortMetricDisplayValue { get; init; }
    public required string ComparisonUrl { get; init; }
}
