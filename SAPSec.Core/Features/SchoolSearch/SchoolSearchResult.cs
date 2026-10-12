using SAPSec.Core.Geography;
using SAPSec.Data.Dto;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace SAPSec.Core.Features.SchoolSearch;

[ExcludeFromCodeCoverage]
public record SchoolSearchResult
(
    SchoolInfo.SchoolInfo Info,
    string? Latitude,
    string? Longitude
)
{
    public static SchoolSearchResult FromNameAndEstablishment(string name, Establishment establishment, GeographicCoordinates? latLong = null) => new(
        SchoolInfo.SchoolInfo.Create(establishment),
        latLong?.Latitude.ToString(CultureInfo.InvariantCulture),
        latLong?.Longitude.ToString(CultureInfo.InvariantCulture));
}
