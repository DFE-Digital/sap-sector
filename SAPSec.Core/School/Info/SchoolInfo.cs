using SAPSec.Data.Dto;

namespace SAPSec.Core.School.Info;

public record SchoolInfo(string Urn, string Name, LocalAuthority LocalAuthority, Address Address)
{
    public static SchoolInfo FromEstablishment(Establishment establishment) =>
        new SchoolInfo(
            establishment.URN,
            establishment.EstablishmentName,
            LocalAuthority.FromEstablishment(establishment),
            Address.FromEstablishment(establishment));
}

public record LocalAuthority(string Id, string Name)
{
    public static LocalAuthority FromEstablishment(Establishment establishment) =>
         new(establishment.LAId, establishment.LAName);
}
