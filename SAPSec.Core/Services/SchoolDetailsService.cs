using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Mappers;
using SAPSec.Core.Model;
using SAPSec.Core.Rules;

namespace SAPSec.Core.Services;

/// <summary>
/// Service that maps establishment data to SchoolDetails.
/// Business rules are applied internally - no need for DI as they are pure functions.
/// </summary>
public sealed class SchoolDetailsService : ISchoolDetailsService
{
    private readonly IEstablishmentService _establishmentService;
    private readonly ILogger<SchoolDetailsService> _logger;

    // Rules instantiated directly - they are stateless pure functions
    private readonly GovernanceRule _governanceRule = new();
    private readonly NurseryProvisionRule _nurseryProvisionRule = new();
    private readonly SixthFormRule _sixthFormRule = new();
    private readonly SenUnitRule _senUnitRule = new();
    private readonly ResourcedProvisionRule _resourcedProvisionRule = new();

    public SchoolDetailsService(
        IEstablishmentService establishmentService,
        ILogger<SchoolDetailsService> logger)
    {
        _establishmentService = establishmentService;
        _logger = logger;
    }

    public async Task<SchoolDetails> GetByUrnAsync(string urn)
    {
        var establishment = await _establishmentService.GetEstablishmentAsync(urn);

        return MapToSchoolDetails(establishment);
    }

    public async Task<SchoolDetails?> TryGetByUrnAsync(string urn)
    {
        try
        {
            return await GetByUrnAsync(urn);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "School not found for URN: {Urn}", urn);
            return null;
        }
    }

    public async Task<SchoolDetails?> GetByIdentifierAsync(string identifier)
    {
        var establishment = await _establishmentService.GetEstablishmentByAnyNumberAsync(identifier);

        if (string.IsNullOrWhiteSpace(establishment?.URN))
        {
            return null;
        }

        return MapToSchoolDetails(establishment);
    }

    private SchoolDetails MapToSchoolDetails(Establishment establishment)
    {
        return new SchoolDetails
        {
            // Identifiers
            Urn = establishment.URN,
            Name = establishment.EstablishmentName,
            DfENumber = DataMapper.MapDfENumber(establishment.DfENumber),
            Ukprn = DataMapper.MapRequiredString(establishment.UKPRN),

            // Location
            Address = DataMapper.MapAddress(establishment),
            LocalAuthorityName = DataMapper.MapString(establishment.LAName),
            LocalAuthorityCode = DataMapper.MapRequiredString(establishment.LAId),
            Region = DataMapper.MapString(establishment.DistrictAdministrativeName),
            UrbanRuralDescription = DataMapper.MapString(establishment.UrbanRuralName),

            // School characteristics
            AgeRangeLow = DataMapper.MapAge(establishment.AgeRangeLow),
            AgeRangeHigh = DataMapper.MapAge(establishment.AgeRangeRange),
            GenderOfEntry = DataMapper.MapString(establishment.GenderName),
            PhaseOfEducation = DataMapper.MapString(establishment.PhaseOfEducationName),
            SchoolType = DataMapper.MapString(establishment.TypeOfEstablishmentName),
            AdmissionsPolicy = DataMapper.MapString(establishment.AdmissionsPolicyId),
            ReligiousCharacter = DataMapper.MapString(establishment.ReligiousCharacterName),

            // Governance - business rule
            GovernanceStructure = _governanceRule.Evaluate(establishment),
            AcademyTrustName = DataMapper.MapTrustName(establishment),
            AcademyTrustId = DataMapper.MapTrustId(establishment.TrustsId),

            // Provisions - business rules
            HasNurseryProvision = _nurseryProvisionRule.Evaluate(establishment),
            HasSixthForm = _sixthFormRule.Evaluate(establishment),
            HasSenUnit = _senUnitRule.Evaluate(establishment),
            HasResourcedProvision = _resourcedProvisionRule.Evaluate(establishment),

            // Contact
            HeadteacherName = DataMapper.MapHeadteacher(establishment),
            Website = DataMapper.MapWebsite(establishment.Website),
            Telephone = DataMapper.MapString(establishment.TelephoneNum),
            Email = DataWithAvailability.NotAvailable<string>()
        };
    }
}