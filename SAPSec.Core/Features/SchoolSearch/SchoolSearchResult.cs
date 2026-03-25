using SAPSec.Core.Features.Geography;
using SAPSec.Data.Model.Generated;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace SAPSec.Core.Features.SchoolSearch;

[ExcludeFromCodeCoverage]
public record SchoolSearchResult
(
    string Name,
    string URN,
    string EstablishmentName,
    string AddressStreet,
    string AddressLocality,
    string AddressAddress3,
    string AddressTown,
    string AddressPostcode,
    string LANAme,
    string PhaseOfEducationName,
    string? Latitude,
    string? Longitude
)
{
    public static SchoolSearchResult FromNameAndEstablishment(string name, Establishment establishment, GeographicCoordinates? latLong = null) => new(
        name,
        establishment.URN,
        establishment.EstablishmentName,
        establishment.Street,
        establishment.Locality,
        establishment.Address3,
        establishment.Town,
        establishment.Postcode,
        establishment.LAName,
        establishment.PhaseOfEducationName,
        latLong?.Latitude.ToString(CultureInfo.InvariantCulture),
        latLong?.Longitude.ToString(CultureInfo.InvariantCulture));
}
