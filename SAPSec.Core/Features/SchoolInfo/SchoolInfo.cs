using SAPSec.Data.Dto;

namespace SAPSec.Core.Features.SchoolInfo;

public record SchoolInfo(string Urn, string Name, LocalAuthority LocalAuthority, Address Address)
{
    internal static SchoolInfo FromEstablishment(Establishment establishment) =>
        new SchoolInfo(
            establishment.URN,
            establishment.EstablishmentName,
            LocalAuthority.FromEstablishment(establishment),
            Address.FromEstablishment(establishment));

    internal static SchoolInfo FromSimilarSchool(SimilarSchools.SimilarSchool school) =>
        new SchoolInfo(
            school.URN,
            school.Name,
            new LocalAuthority(school.LocalAuthority.Id, school.LocalAuthority.Name),
            new Address(school.Address.Street, school.Address.Locality, school.Address.Address3, school.Address.Town, school.Address.Postcode));
}
