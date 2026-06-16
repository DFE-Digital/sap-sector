using SAPSec.Core.Constants;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Features.SchoolSearch.UseCases;

public class DetermineSchoolSearchEligibility
{
    public bool CanIndex(Establishment? establishment)
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

    public bool CanSearch(
        Establishment? establishment,
        EstablishmentEmail? establishmentEmail,
        bool primarySchoolsEnabled)
    {
        if (establishment == null)
        {
            return false;
        }

        if (!HasSearchablePhase(establishment, primarySchoolsEnabled))
        {
            return false;
        }

        if (HasMissingStatus(establishment, establishmentEmail))
        {
            return HasSecondaryPhase(establishment);
        }

        var resolvedStatus = ResolveStatus(establishment, establishmentEmail);
        return EstablishmentStatusValues.IsIncludedInSearch(resolvedStatus.StatusId, resolvedStatus.StatusName);
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

    private static bool HasMissingStatus(
        Establishment establishment,
        EstablishmentEmail? establishmentEmail)
    {
        return string.IsNullOrWhiteSpace(establishment.EstablishmentStatusId)
            && string.IsNullOrWhiteSpace(establishment.EstablishmentStatusName)
            && (establishmentEmail == null
                || (string.IsNullOrWhiteSpace(establishmentEmail.EstablishmentStatusId)
                    && string.IsNullOrWhiteSpace(establishmentEmail.EstablishmentStatusName)));
    }

    private static (string? StatusId, string? StatusName) ResolveStatus(
        Establishment establishment,
        EstablishmentEmail? establishmentEmail)
    {
        var statusId = string.IsNullOrWhiteSpace(establishment.EstablishmentStatusId)
            ? establishmentEmail?.EstablishmentStatusId
            : establishment.EstablishmentStatusId;

        var statusName = string.IsNullOrWhiteSpace(establishment.EstablishmentStatusName)
            ? establishmentEmail?.EstablishmentStatusName
            : establishment.EstablishmentStatusName;

        return (statusId, statusName);
    }
}
