using SAPSec.Core.Constants;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Features.SchoolInfo;

public record SchoolInfo(string Urn, string Name, LocalAuthority LocalAuthority, Address Address, bool IsPrimary, bool IsSecondary)
{
    public static SchoolInfo FromEstablishment(Establishment establishment) =>
        new SchoolInfo(
            establishment.URN,
            establishment.EstablishmentName,
            LocalAuthority.FromEstablishment(establishment),
            Address.FromEstablishment(establishment),
            PhaseOfEducationValues.IsPrimaryOrAllThrough(establishment.PhaseOfEducationName),
            PhaseOfEducationValues.IsSecondary(establishment.PhaseOfEducationName));
}

public record LocalAuthority(string Id, string Name)
{
    public static LocalAuthority FromEstablishment(Establishment establishment) =>
         new(establishment.LAId, establishment.LAName);
}
