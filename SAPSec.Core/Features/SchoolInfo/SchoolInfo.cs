using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Features.SchoolInfo;

public record SchoolInfo(string Urn, string Name, LocalAuthority LocalAuthority, Address Address, EducationStage EducationStage)
{
    internal static SchoolInfo FromEstablishment(Establishment establishment) =>
        new SchoolInfo(
            establishment.URN,
            establishment.EstablishmentName,
            LocalAuthority.FromEstablishment(establishment),
            Address.FromEstablishment(establishment),
            EducationStageHelper.FromEstablishment(establishment));

    internal static SchoolInfo FromSimilarSchool(SimilarSchools.SimilarSchool school) =>
        new SchoolInfo(
            school.URN,
            school.Name,
            new LocalAuthority(school.LocalAuthority.Id, school.LocalAuthority.Name),
            new Address(school.Address.Street, school.Address.Locality, school.Address.Address3, school.Address.Town, school.Address.Postcode),
            school.EducationStage);
}

[Flags]
public enum EducationStage
{
    None = 0,
    Primary = 1,
    Secondary = 2
}

public static class EducationStageHelper
{
    public static EducationStage FromEstablishment(Establishment establishment)
    {
        var value = EducationStage.None;

        if (establishment.PhaseOfEducationId is PhaseOfEducationValues.PrimaryId or PhaseOfEducationValues.MiddleDeemedPrimaryId or PhaseOfEducationValues.AllThroughId)
        {
            value |= EducationStage.Primary;
        }

        if (establishment.PhaseOfEducationId is PhaseOfEducationValues.SecondaryId or PhaseOfEducationValues.MiddleDeemedSecondaryId or PhaseOfEducationValues.AllThroughId)
        {
            value |= EducationStage.Secondary;
        }

        return value;
    }
}