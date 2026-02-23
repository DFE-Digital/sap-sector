using SAPSec.Core.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Core.Features.SchoolSearch
{
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
        string? Latitude,
        string? Longitude
    )
    {
        public static SchoolSearchResult FromNameAndEstablishment(string name, Establishment establishment) => new(
            name,
            establishment.URN,
            establishment.EstablishmentName,
            establishment.Street,
            establishment.Locality,
            establishment.Address3,
            establishment.Town,
            establishment.Postcode,
            establishment.LAName,
            establishment.Latitude,
            establishment.Longitude);
    }
}