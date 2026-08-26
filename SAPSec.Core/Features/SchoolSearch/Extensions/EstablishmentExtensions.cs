using SAPSec.Core.Constants;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Features.SchoolSearch.Extensions;

public static class EstablishmentExtensions
{
    public static string SearchExclusionReason(this Establishment? establishment, bool primarySchoolsEnabled)
    {
        if (establishment == null)
        {
            return "null_establishment";
        }

        if (!HasSearchablePhase(establishment, primarySchoolsEnabled))
        {
            return "phase_not_searchable";
        }

        if (IsSecondaryExcluded(establishment))
        {
            return "secondary_status_excluded";
        }

        if (HasMissingStatus(establishment))
        {
            return HasSecondaryPhase(establishment)
                ? "included_missing_status_secondary"
                : "missing_status_non_secondary";
        }

        return EstablishmentStatusValues.IsIncludedInSearch(
            establishment.EstablishmentStatusId,
            establishment.EstablishmentStatusName)
            ? "included"
            : "status_not_included";
    }

    public static bool CanIndexForSearch(this Establishment? establishment)
    {
        return establishment.SearchExclusionReason(primarySchoolsEnabled: true) is "included" or "included_missing_status_secondary";
    }

    public static bool CanSearch(this Establishment? establishment, bool primarySchoolsEnabled)
    {
        if (establishment == null)
        {
            return false;
        }

        if (!HasSearchablePhase(establishment, primarySchoolsEnabled))
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
            establishment.EstablishmentStatusName);
    }

    public static bool IsSearchable(this Establishment? establishment)
    {
        return establishment.CanIndexForSearch();
    }

    public static bool IsSearchable(this Establishment? establishment, bool primarySchoolsEnabled)
    {
        return establishment.CanSearch(primarySchoolsEnabled);
    }

    private static bool HasSearchablePhase(Establishment establishment, bool primarySchoolsEnabled)
    {
        if (PhaseOfEducationValues.IsSearchableSearchPhaseId(establishment.PhaseOfEducationId, primarySchoolsEnabled))
        {
            return true;
        }

        var phase = establishment.PhaseOfEducationName;

        return PhaseOfEducationValues.IsSecondary(phase)
            || (primarySchoolsEnabled && PhaseOfEducationValues.IsPrimaryOrAllThrough(phase));
    }

    private static bool HasLegacySearchablePhaseName(string? phaseOfEducationName)
    {
        return PhaseOfEducationValues.IsSecondary(phaseOfEducationName)
            || PhaseOfEducationValues.IsPrimaryOrAllThrough(phaseOfEducationName);
    }

    private static bool HasSecondaryPhase(Establishment establishment)
    {
        return PhaseOfEducationValues.IsSearchableSearchPhaseId(establishment.PhaseOfEducationId, primarySchoolsEnabled: false)
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

        return establishmentStatusId is EstablishmentStatusValues.ClosedId
            or EstablishmentStatusValues.ProposedToOpenId;
    }
}
