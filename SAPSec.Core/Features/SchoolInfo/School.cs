using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Data.Dto;
using System.Globalization;

namespace SAPSec.Core.Features.SchoolInfo;

public class School
{
    private const string CloseDateFormat = "dd-MM-yyyy";

    private string _establishmentStatusId;
    private string _phaseOfEducationId;

    public string Urn { get; private init; }
    public string Name { get; private init; }
    public LocalAuthority LocalAuthority { get; private init; }
    public Address Address { get; private init; }
    public DateOnly? CloseDate { get; private init; }

    private School(
        string urn,
        string name,
        LocalAuthority localAuthority,
        Address address,
        string establishmentStatusId,
        string phaseOfEducationId,
        DateOnly? closeDate)
    {
        Urn = urn;
        Name = name;
        LocalAuthority = localAuthority;
        Address = address;
        CloseDate = closeDate;

        _establishmentStatusId = establishmentStatusId;
        _phaseOfEducationId = phaseOfEducationId;
    }

    protected School(School school)
    {
        Urn = school.Urn;
        Name = school.Name;
        LocalAuthority = school.LocalAuthority;
        Address = school.Address;
        CloseDate = school.CloseDate;

        _establishmentStatusId = school._establishmentStatusId;
        _phaseOfEducationId = school._phaseOfEducationId;
    }

    public static School Create(Establishment establishment) =>
        new School(
            establishment.URN,
            establishment.EstablishmentName,
            LocalAuthority.FromEstablishment(establishment),
            Address.FromEstablishment(establishment),
            establishment.EstablishmentStatusId,
            establishment.PhaseOfEducationId,
            DateOnly.TryParseExact(establishment.CloseDate, CloseDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedCloseDate)
                ? parsedCloseDate
                : null);

    public bool IsClosed => _establishmentStatusId is EstablishmentStatusId.ClosedId;

    public bool IsSearchable(
        bool primarySchoolsEnabled,
        bool allThroughSchoolsEnabled)
    {
        if (_phaseOfEducationId is PhaseOfEducationId.PrimaryId or PhaseOfEducationId.MiddleDeemedPrimaryId or PhaseOfEducationId.AllThroughId
            && !primarySchoolsEnabled)
        {
            return false;
        }

        if (_phaseOfEducationId is PhaseOfEducationId.AllThroughId
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

        if (_establishmentStatusId is EstablishmentStatusId.OpenId or EstablishmentStatusId.OpenButProposedToCloseId)
        {
            return true;
        }

        if (_establishmentStatusId is EstablishmentStatusId.ClosedId or EstablishmentStatusId.ProposedToOpenId)
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

    public bool HasPrimaryEducationPhase => _phaseOfEducationId is
        PhaseOfEducationId.PrimaryId or
        PhaseOfEducationId.MiddleDeemedPrimaryId or
        PhaseOfEducationId.AllThroughId;

    public bool HasSecondaryEducationPhase => _phaseOfEducationId is
        PhaseOfEducationId.SecondaryId or
        PhaseOfEducationId.MiddleDeemedSecondaryId or
        PhaseOfEducationId.AllThroughId;

    public bool IsAllThroughSchool => _phaseOfEducationId is PhaseOfEducationId.AllThroughId;
    public bool IsPrimarySchool => _phaseOfEducationId is PhaseOfEducationId.PrimaryId;
    public bool IsSecondarySchool => _phaseOfEducationId is PhaseOfEducationId.SecondaryId;
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
