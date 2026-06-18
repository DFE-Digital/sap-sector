using SAPSec.Core.Constants;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Extensions;

public static class SchoolDetailsExtensions
{
    public static bool IsPrimarySchool(this SchoolDetails school)
    {
        ArgumentNullException.ThrowIfNull(school);

        return school.PhaseOfEducation.HasValue
            && PhaseOfEducationValues.IsPrimaryOrAllThrough(school.PhaseOfEducation.Value);
    }

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

        if (HasMissingStatus(establishment))
        {
            return HasSecondaryPhase(establishment);
        }

        return EstablishmentStatusValues.IsIncludedInSearch(
            establishment.EstablishmentStatusId,
            establishment.EstablishmentStatusName);
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
}
