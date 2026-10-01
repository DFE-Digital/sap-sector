using Microsoft.Extensions.Logging;
using SAPSec.Data.Dto;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SchoolDetails;

/// <summary>
/// Business rule: a school that is not itself closed shows the successor banner where it has one or
/// more predecessor relationships (see <see cref="PredecessorRelationshipTypeValues"/>). A closed
/// school always shows the closed-school banner instead - see <see cref="SchoolClosureEligibilityService"/>.
/// </summary>
public sealed class SchoolPredecessorRelationshipService(
    IEstablishmentRepository establishmentRepository,
    ILogger<SchoolPredecessorRelationshipService> logger)
    : ISchoolPredecessorRelationshipService
{
    public async Task<SchoolPredecessorRelationship> EvaluateAsync(Establishment establishment)
    {
        ArgumentNullException.ThrowIfNull(establishment);

        if (EstablishmentStatusValues.IsClosed(establishment.EstablishmentStatusId, establishment.EstablishmentStatusName))
        {
            return SchoolPredecessorRelationship.None;
        }

        var links = await establishmentRepository.GetEstablishmentLinksAsync(establishment.URN);

        var candidateUrns = links
            .Where(link => PredecessorRelationshipTypeValues.IsMapped(link.linktype))
            .Select(link => link.linkurn)
            .Distinct()
            .ToList();

        if (candidateUrns.Count == 0)
        {
            return SchoolPredecessorRelationship.None;
        }

        var candidateEstablishments = (await establishmentRepository.GetEstablishmentsAsync(candidateUrns))
            .ToDictionary(e => e.URN, StringComparer.Ordinal);

        var predecessors = new List<SuccessorSchool>();

        foreach (var candidateUrn in candidateUrns)
        {
            if (!candidateEstablishments.TryGetValue(candidateUrn, out var candidate))
            {
                logger.LogWarning(
                    "School {Urn} has a predecessor relationship to {PredecessorUrn}, but no establishment record was found for it.",
                    establishment.URN,
                    candidateUrn);
                continue;
            }

            // A predecessor beyond its own data eligibility window would link to a 404 - don't
            // signpost to it.
            if (!SchoolClosureEligibilityRule.IsEligibleToAppear(candidate))
            {
                logger.LogWarning(
                    "School {Urn} has a predecessor relationship to {PredecessorUrn}, which is beyond the data eligibility window and cannot be linked to.",
                    establishment.URN,
                    candidateUrn);
                continue;
            }

            predecessors.Add(new SuccessorSchool(candidate.URN, candidate.EstablishmentName, candidate.PhaseOfEducationName));
        }

        return new SchoolPredecessorRelationship(predecessors);
    }
}
