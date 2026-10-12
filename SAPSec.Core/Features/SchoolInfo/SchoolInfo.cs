using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Data.Dto;
using System.Globalization;

namespace SAPSec.Core.Features.SchoolInfo;

public class SchoolInfo(string urn, string name, LocalAuthority localAuthority, Address address, string establishmentStatusId, string phaseOfEducationId, DateOnly? closeDate)
{
    private const string CloseDateFormat = "dd-MM-yyyy";

    public string Urn => urn;
    public string Name => name;
    public LocalAuthority LocalAuthority => localAuthority;
    public Address Address => address;
    public DateOnly? CloseDate => closeDate;

    public static SchoolInfo Create(Establishment establishment) =>
        new SchoolInfo(
            establishment.URN,
            establishment.EstablishmentName,
            LocalAuthority.FromEstablishment(establishment),
            Address.FromEstablishment(establishment),
            establishment.EstablishmentStatusId,
            establishment.PhaseOfEducationId,
            DateOnly.TryParseExact(establishment.CloseDate, CloseDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedCloseDate)
                ? parsedCloseDate
                : null);

    public bool IsClosed => establishmentStatusId is EstablishmentStatusId.ClosedId;

    public bool IsSearchable(
        bool primarySchoolsEnabled,
        bool allThroughSchoolsEnabled)
    {
        if (phaseOfEducationId is PhaseOfEducationId.PrimaryId or PhaseOfEducationId.MiddleDeemedPrimaryId or PhaseOfEducationId.AllThroughId
            && !primarySchoolsEnabled)
        {
            return false;
        }

        if (phaseOfEducationId is PhaseOfEducationId.AllThroughId
            && !allThroughSchoolsEnabled)
        {
            return false;
        }

        //if (!HasSearchablePhase(establishment, primarySchoolsEnabled, allThroughSchoolsEnabled))
        //{
        //    return false;
        //}

        //if (IsSecondaryExcluded(establishment))
        //{
        //    return false;
        //}

        if (establishmentStatusId is EstablishmentStatusId.OpenId or EstablishmentStatusId.OpenButProposedToCloseId)
        {
            return true;
        }

        if (establishmentStatusId is EstablishmentStatusId.ClosedId or EstablishmentStatusId.ProposedToOpenId)
        {
            return SchoolClosureEligibilityRule.IsEligibleToAppear(this);
        }

        return false;

        //if (HasMissingStatus(establishment))
        //{
        //    return HasSecondaryPhase(establishment);
        //}

        //return EstablishmentStatusValues.IsIncludedInSearch(
        //        establishment.EstablishmentStatusId,
        //        establishment.EstablishmentStatusName)
        //    || IsEligibleClosedSchool(establishment);
    }

    public bool HasPrimaryEducationPhase => phaseOfEducationId is
        PhaseOfEducationId.PrimaryId or
        PhaseOfEducationId.MiddleDeemedPrimaryId or
        PhaseOfEducationId.AllThroughId;

    public bool HasSecondaryEducationPhase => phaseOfEducationId is
        PhaseOfEducationId.SecondaryId or
        PhaseOfEducationId.MiddleDeemedSecondaryId or
        PhaseOfEducationId.AllThroughId;

    public bool IsAllThroughSchool => phaseOfEducationId is PhaseOfEducationId.AllThroughId;
    public bool IsPrimarySchool => phaseOfEducationId is PhaseOfEducationId.PrimaryId;
    public bool IsSecondarySchool => phaseOfEducationId is PhaseOfEducationId.SecondaryId;
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

        if (establishment.PhaseOfEducationId is PhaseOfEducationId.PrimaryId or PhaseOfEducationId.MiddleDeemedPrimaryId or PhaseOfEducationId.AllThroughId)
        {
            value |= EducationStage.Primary;
        }

        if (establishment.PhaseOfEducationId is PhaseOfEducationId.SecondaryId or PhaseOfEducationId.MiddleDeemedSecondaryId or PhaseOfEducationId.AllThroughId)
        {
            value |= EducationStage.Secondary;
        }

        return value;
    }
}

public class SchoolInfoWithLinks
{
    public required SchoolInfo Info { get; init; }

    // Identifiers
    public required bool ShowClosedSchoolBanner { get; init; }
    public required IReadOnlyList<SchoolInfo> Successors { get; init; }
    public required IReadOnlyList<SchoolInfo> Predecessors { get; init; }
}