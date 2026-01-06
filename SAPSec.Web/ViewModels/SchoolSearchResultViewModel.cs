using Lucene.Net.Search;

namespace SAPSec.Web.ViewModels;

public record SchoolSearchResultViewModel
{
    public string? URN { get; init; }
    public string? SchoolName { get; init; }
    public string? Address { get; init; }
    public string? LocalAuthority { get; init; }
    public string? Latitude { get; init; }
    public string? Longitude { get; init; }
}