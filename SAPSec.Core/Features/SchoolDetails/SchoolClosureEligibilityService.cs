using Microsoft.Extensions.Logging;
using SAPSec.Data.Dto;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SchoolDetails;

/// <summary>
/// Business rule: a closed school may only continue to appear in the service where it meets the
/// agreed data availability criteria (see <see cref="SchoolClosureEligibilityRule"/>). The plain
/// closed-school banner is shown only where the school has no successor relationship at all; where
/// it has one or more resolvable successor relationships (see <see cref="SuccessorRelationshipTypeValues"/>),
/// the predecessor banner is shown instead, signposting to those successor schools.
/// </summary>
public sealed class SchoolClosureEligibilityService(
    IEstablishmentRepository establishmentRepository,
    ILogger<SchoolClosureEligibilityService> logger)
    : ISchoolClosureEligibilityService
{
    public async Task<SchoolClosureEligibility> EvaluateAsync(Establishment establishment)
    {
        ArgumentNullException.ThrowIfNull(establishment);

        if (!EstablishmentStatusValues.IsClosed(establishment.EstablishmentStatusId, establishment.EstablishmentStatusName))
        {
            return SchoolClosureEligibility.NotClosed;
        }

        if (!SchoolClosureEligibilityRule.IsEligibleToAppear(establishment))
        {
            return new SchoolClosureEligibility(IsEligibleForDisplay: false, ShowClosedSchoolBanner: false, Successors: []);
        }

        var links = await establishmentRepository.GetEstablishmentLinksAsync(establishment.URN);
        var hasAnySuccessorLink = links.Any(link => EstablishmentLinkTypeValues.IsSuccessor(link.linktype));

        var successors = await ResolveSuccessorsAsync(establishment.URN, links);

        return new SchoolClosureEligibility(
            IsEligibleForDisplay: true,
            ShowClosedSchoolBanner: !hasAnySuccessorLink,
            Successors: successors);
    }

    private async Task<IReadOnlyList<SuccessorSchool>> ResolveSuccessorsAsync(
        string urn,
        IReadOnlyCollection<EstablishmentLinks> links)
    {
        var candidateUrns = links
            .Where(link => SuccessorRelationshipTypeValues.IsMapped(link.linktype))
            .Select(link => link.linkurn)
            .Distinct()
            .ToList();

        if (candidateUrns.Count == 0)
        {
            return [];
        }

        var candidateEstablishments = (await establishmentRepository.GetEstablishmentsAsync(candidateUrns))
            .ToDictionary(e => e.URN, StringComparer.Ordinal);

        var successors = new List<SuccessorSchool>();

        foreach (var candidateUrn in candidateUrns)
        {
            if (!candidateEstablishments.TryGetValue(candidateUrn, out var candidate))
            {
                logger.LogWarning(
                    "Closed school {Urn} has a successor relationship to {SuccessorUrn}, but no establishment record was found for it.",
                    urn,
                    candidateUrn);
                continue;
            }

            // A mapped relationship (e.g. "Result of Amalgamation") is reciprocal and direction is
            // inferred from status: the current school is the predecessor only when the linked
            // school is not itself closed. Both sides closed is an unexpected combination - log it
            // rather than signpost to a dead end.
            if (EstablishmentStatusValues.IsClosed(candidate.EstablishmentStatusId, candidate.EstablishmentStatusName))
            {
                logger.LogWarning(
                    "Unexpected relationship: closed school {Urn} has a mapped successor relationship to {SuccessorUrn}, which is also closed.",
                    urn,
                    candidateUrn);
                continue;
            }

            successors.Add(new SuccessorSchool(candidate.URN, candidate.EstablishmentName, candidate.PhaseOfEducationName));
        }

        return successors;
    }
}
