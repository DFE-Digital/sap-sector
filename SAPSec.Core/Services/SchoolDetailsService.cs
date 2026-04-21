using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Mappers;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Generated;
using SAPSec.Core.Rules;

namespace SAPSec.Core.Services;

/// <summary>
/// Service that maps establishment data to SchoolDetails.
/// Business rules are applied internally - no need for DI as they are pure functions.
/// </summary>
public sealed class SchoolDetailsService : ISchoolDetailsService
{
    private readonly IEstablishmentRepository _establishmentRepository;
    private readonly ILogger<SchoolDetailsService> _logger;

    // Rules instantiated directly - they are stateless pure functions
    private readonly GovernanceRule _governanceRule = new();
    private readonly NurseryProvisionRule _nurseryProvisionRule = new();
    private readonly SixthFormRule _sixthFormRule = new();
    private readonly SenUnitRule _senUnitRule = new();
    private readonly ResourcedProvisionRule _resourcedProvisionRule = new();

    public SchoolDetailsService(
        IEstablishmentRepository establishmentRepository,
        ILogger<SchoolDetailsService> logger)
    {
        _establishmentRepository = establishmentRepository;
        _logger = logger;
    }

    public async Task<SchoolDetails> GetByUrnAsync(string urn)
    {
        var establishment = await _establishmentRepository.GetEstablishmentAsync(urn);

        if (establishment is null)
        {
            throw new NotFoundException($"School not found with URN: {urn}");
        }

        var establishmentEmail = await _establishmentRepository.GetEstablishmentEmailAsync(urn);

        return MapToSchoolDetails(establishment, establishmentEmail);
    }

    private SchoolDetails MapToSchoolDetails(Establishment establishment, EstablishmentEmail? establishmentEmail)
    {
        return new SchoolDetails
        {
            // Identifiers
            Urn = establishment.URN,
            Name = establishment.EstablishmentName,
            DfENumber = DataMapper.MapDfENumber(establishment),
            Ukprn = DataMapper.MapRequiredString(establishment.UKPRN),

            // Location
            Address = DataMapper.MapAddress(establishment),
            LocalAuthorityName = DataMapper.MapString(establishment.LAName),
            LocalAuthorityCode = DataMapper.MapRequiredString(establishment.LAId),
            Region = DataMapper.MapString(establishment.DistrictAdministrativeName),
            UrbanRuralDescription = DataMapper.MapString(establishment.UrbanRuralName),

            // School characteristics
            AgeRangeLow = DataWithAvailability.FromNullable(establishment.AgeRangeLow),
            AgeRangeHigh = DataWithAvailability.FromNullable(establishment.AgeRangeHigh),
            GenderOfEntry = DataMapper.MapString(establishment.GenderName),
            PhaseOfEducation = DataMapper.MapString(establishment.PhaseOfEducationName),
            SchoolType = DataMapper.MapString(establishment.TypeOfEstablishmentName),
            AdmissionsPolicy = DataMapper.MapString(establishment.AdmissionsPolicyName),
            ReligiousCharacter = DataMapper.MapString(establishment.ReligiousCharacterName),

            // Governance - business rule
            GovernanceStructure = _governanceRule.Evaluate(establishment),
            AcademyTrustName = DataMapper.MapTrustName(establishment),
            AcademyTrustId = DataMapper.MapTrustId(establishment.TrustId),

            // Provisions - business rules
            HasNurseryProvision = _nurseryProvisionRule.Evaluate(establishment),
            HasSixthForm = _sixthFormRule.Evaluate(establishment),
            HasSenUnit = _senUnitRule.Evaluate(establishment),
            HasResourcedProvision = _resourcedProvisionRule.Evaluate(establishment),

            // Contact
            HeadteacherName = DataMapper.MapHeadteacher(establishment),
            Website = DataMapper.MapWebsite(establishment.Website),
            Telephone = DataMapper.MapString(establishment.TelephoneNum),
            Email = DataMapper.MapString(establishmentEmail?.MainEmail)
        };
    }
}
