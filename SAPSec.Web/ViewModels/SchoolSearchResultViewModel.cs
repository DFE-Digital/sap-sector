using SAPSec.Core.Features.SchoolSearch;
using SAPSec.Web.Constants;

namespace SAPSec.Web.ViewModels;

public record SchoolSearchResultViewModel
{
    public string? URN { get; init; }
    public string? SchoolName { get; init; }
    public string? Address { get; init; }
    public string? LocalAuthority { get; init; }
    public string? Latitude { get; init; }
    public string? Longitude { get; init; }
    public string? SchoolUrl { get; init; }

    public static SchoolSearchResultViewModel From(SchoolSearchResult result) => new()
    {
        SchoolName = result.EstablishmentName,
        URN = result.URN,
        LocalAuthority = result.LANAme,
        Latitude = result.Latitude,
        Longitude = result.Longitude,
        SchoolUrl = Routes.School(result.URN, result.PhaseOfEducationName),
        Address = string.Join(", ", new[]
        {
            result.AddressStreet,
            result.AddressLocality,
            result.LANAme,
            result.AddressPostcode
        }.Where(x => !string.IsNullOrWhiteSpace(x)))
    };
}
