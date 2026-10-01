using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Features.SchoolSearch.Extensions;

public static class EstablishmentExtensions
{
    public static bool CanIndexForSearch(this Establishment? establishment)
    {
        if (establishment == null)
        {
            return false;
        }

        if (PhaseOfEducationValues.IsSearchableIndexPhaseId(establishment.PhaseOfEducationId))
        {
            return true;
        }

        return HasLegacySearchablePhaseName(establishment.PhaseOfEducationName);
    }

    public static bool CanSearch(
        this Establishment? establishment,
        bool primarySchoolsEnabled,
        bool allThroughSchoolsEnabled)
    {
        if (establishment == null)
        {
            return false;
        }

        if (!HasSearchablePhase(establishment, primarySchoolsEnabled, allThroughSchoolsEnabled))
        {
            return false;
        }

        if (IsSecondaryExcluded(establishment))
        {
            return false;
        }

        if (HasMissingStatus(establishment))
        {
            return HasSecondaryPhase(establishment);
        }

        return EstablishmentStatusValues.IsIncludedInSearch(
                establishment.EstablishmentStatusId,
                establishment.EstablishmentStatusName)
            || IsEligibleClosedSchool(establishment);
    }

    public static bool IsSearchable(this Establishment? establishment)
    {
        return establishment.CanIndexForSearch();
    }

    public static bool IsSearchable(
        this Establishment? establishment,
        bool primarySchoolsEnabled,
        bool allThroughSchoolsEnabled)
    {
        return establishment.CanSearch(primarySchoolsEnabled, allThroughSchoolsEnabled);
    }

    private static bool HasSearchablePhase(
        Establishment establishment,
        bool primarySchoolsEnabled,
        bool allThroughSchoolsEnabled)
    {
        if (PhaseOfEducationValues.IsSearchableSearchPhaseId(
                establishment.PhaseOfEducationId,
                primarySchoolsEnabled,
                allThroughSchoolsEnabled))
        {
            return true;
        }

        var phase = establishment.PhaseOfEducationName;

        return PhaseOfEducationValues.IsSecondary(phase)
            || (primarySchoolsEnabled && PhaseOfEducationValues.IsPrimary(phase))
            || (allThroughSchoolsEnabled && PhaseOfEducationValues.IsAllThrough(phase));
    }

    private static bool HasLegacySearchablePhaseName(string? phaseOfEducationName)
    {
        return PhaseOfEducationValues.IsSecondary(phaseOfEducationName)
            || PhaseOfEducationValues.IsPrimaryOrAllThrough(phaseOfEducationName);
    }

    private static bool HasSecondaryPhase(Establishment establishment)
    {
        return PhaseOfEducationValues.IsSearchableSearchPhaseId(
                establishment.PhaseOfEducationId,
                primarySchoolsEnabled: false,
                allThroughSchoolsEnabled: false)
            || PhaseOfEducationValues.IsSecondary(establishment.PhaseOfEducationName);
    }

    private static bool HasMissingStatus(Establishment establishment)
    {
        return string.IsNullOrWhiteSpace(establishment.EstablishmentStatusId)
            && string.IsNullOrWhiteSpace(establishment.EstablishmentStatusName);
    }

    private static bool IsSecondaryExcluded(Establishment establishment)
    {
        if (!PhaseOfEducationValues.IsSecondary(establishment.PhaseOfEducationName))
        {
            return false;
        }

        var establishmentStatusId = establishment.EstablishmentStatusId?.Trim();

        if (establishmentStatusId is EstablishmentStatusValues.ProposedToOpenId)
        {
            return true;
        }

        if (establishmentStatusId is EstablishmentStatusValues.ClosedId)
        {
            return !SchoolClosureEligibilityRule.IsEligibleToAppear(establishment);
        }

        return false;
    }

    /// <summary>
    /// A closed school still appears in search results where it meets the agreed data
    /// eligibility criteria - see <see cref="SchoolClosureEligibilityRule"/>. Schools closed
    /// beyond the eligibility window are not searchable (and 404 if navigated to directly).
    /// </summary>
    private static bool IsEligibleClosedSchool(Establishment establishment) =>
        EstablishmentStatusValues.IsClosed(establishment.EstablishmentStatusId, establishment.EstablishmentStatusName)
        && SchoolClosureEligibilityRule.IsEligibleToAppear(establishment);
}
