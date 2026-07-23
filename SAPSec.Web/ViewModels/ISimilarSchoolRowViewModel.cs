namespace SAPSec.Web.ViewModels;

/// <summary>
/// Shape needed by the shared "similar schools" list/map partials, implemented by
/// both the primary and secondary row ViewModels.
/// </summary>
public interface ISimilarSchoolRowViewModel
{
    string Urn { get; }
    string Name { get; }
    string LocalAuthorityName { get; }
    string ComparisonUrl { get; }
    string FullAddress { get; }
    string? Latitude { get; }
    string? Longitude { get; }
    string SortMetricName { get; }
    string SortMetricDisplayValue { get; }
}
