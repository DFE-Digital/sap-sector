using Microsoft.Extensions.Logging;
using SAPSec.Core.Exceptions;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SchoolDetails;

/// <summary>
/// Service that maps establishment data to SchoolDetails.
/// Business rules are applied internally - no need for DI as they are pure functions.
/// </summary>
public sealed class LinkedSchoolService : ILinkedSchoolService
{
    private readonly IEstablishmentRepository _establishmentRepository;
    private readonly ISchoolClosureEligibilityService _closureEligibilityService;
    private readonly ISchoolPredecessorRelationshipService _predecessorRelationshipService;
    private readonly ILogger<LinkedSchoolService> _logger;

    public LinkedSchoolService(
        IEstablishmentRepository establishmentRepository,
        ISchoolClosureEligibilityService closureEligibilityService,
        ISchoolPredecessorRelationshipService predecessorRelationshipService,
        ILogger<LinkedSchoolService> logger)
    {
        _establishmentRepository = establishmentRepository;
        _closureEligibilityService = closureEligibilityService;
        _predecessorRelationshipService = predecessorRelationshipService;
        _logger = logger;
    }

    public async Task<LinkedSchool> GetByUrnAsync(string urn)
    {
        var establishment = await _establishmentRepository.GetEstablishmentAsync(urn);

        if (establishment is null)
        {
            throw new NotFoundException($"School not found with URN: {urn}");
        }

        var closureEligibility = await _closureEligibilityService.EvaluateAsync(establishment);

        if (!closureEligibility.IsEligibleForDisplay)
        {
            throw new NotFoundException($"School with URN {urn} is closed and no longer available");
        }

        var predecessorRelationship = await _predecessorRelationshipService.EvaluateAsync(establishment);

        return LinkedSchool.Create(establishment, closureEligibility, predecessorRelationship);
    }
}
