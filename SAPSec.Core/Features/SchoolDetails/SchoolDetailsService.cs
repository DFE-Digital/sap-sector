using Microsoft.Extensions.Logging;
using SAPSec.Core.Exceptions;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SchoolDetails;

/// <summary>
/// Service that maps establishment data to SchoolDetails.
/// Business rules are applied internally - no need for DI as they are pure functions.
/// </summary>
public sealed class SchoolDetailsService : ISchoolDetailsService
{
    private readonly IEstablishmentRepository _establishmentRepository;
    private readonly ISchoolClosureEligibilityService _closureEligibilityService;
    private readonly ISchoolPredecessorRelationshipService _predecessorRelationshipService;
    private readonly ILogger<SchoolDetailsService> _logger;

    // Rules instantiated directly - they are stateless pure functions
    private readonly GovernanceRule _governanceRule = new();
    private readonly NurseryProvisionRule _nurseryProvisionRule = new();
    private readonly SixthFormRule _sixthFormRule = new();
    private readonly SenUnitRule _senUnitRule = new();
    private readonly ResourcedProvisionRule _resourcedProvisionRule = new();

    public SchoolDetailsService(
        IEstablishmentRepository establishmentRepository,
        ISchoolClosureEligibilityService closureEligibilityService,
        ISchoolPredecessorRelationshipService predecessorRelationshipService,
        ILogger<SchoolDetailsService> logger)
    {
        _establishmentRepository = establishmentRepository;
        _closureEligibilityService = closureEligibilityService;
        _predecessorRelationshipService = predecessorRelationshipService;
        _logger = logger;
    }

    public async Task<SchoolDetails> GetByUrnAsync(string urn)
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

        var establishmentEmail = await _establishmentRepository.GetEstablishmentEmailAsync(urn);

        return SchoolDetails.Create(
            establishment,
            closureEligibility,
            predecessorRelationship,
            establishmentEmail,
            _governanceRule.Evaluate(establishment),
            _nurseryProvisionRule.Evaluate(establishment),
            _sixthFormRule.Evaluate(establishment),
            _senUnitRule.Evaluate(establishment),
            _resourcedProvisionRule.Evaluate(establishment));
    }
}
