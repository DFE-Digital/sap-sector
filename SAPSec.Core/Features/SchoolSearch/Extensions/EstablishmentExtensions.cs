using SAPSec.Core.Constants;
using SAPSec.Core.Model.Generated;

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

        if (IsSecondaryWithExcludedStatus(establishment))
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

    private static bool IsSecondaryWithExcludedStatus(Establishment establishment)
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
