using SAPSec.Core.Constants;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Core.Services;

public class EstablishmentService : IEstablishmentService
{
    private readonly IEstablishmentRepository _establishmentRepository;
    private readonly ILookupService _lookupService;

    public EstablishmentService(
        IEstablishmentRepository establishmentRepository,
        ILookupService lookupService)
    {
        _establishmentRepository = establishmentRepository;
        _lookupService = lookupService;
    }

    public IEnumerable<Establishment> GetAllEstablishments()
    {
        return _establishmentRepository.GetAllEstablishments();
    }

    public Establishment GetEstablishment(string urn)
    {
        var establishment = _establishmentRepository.GetEstablishment(urn);

        if (!string.IsNullOrWhiteSpace(establishment?.URN))
        {
            EnrichEstablishment(establishment);
            return establishment;
        }

        throw new Exception("Error in GetEstablishment");
    }

    public Establishment GetEstablishmentByAnyNumber(string number)
    {
        var establishment = _establishmentRepository.GetEstablishmentByAnyNumber(number);

        if (!string.IsNullOrWhiteSpace(establishment?.URN))
        {
            EnrichEstablishment(establishment);
            return establishment;
        }

        return new Establishment();
    }

    private void EnrichEstablishment(Establishment establishment)
    {
        establishment.TypeOfEstablishmentName = _lookupService.GetLookupValue(
            LookupTypes.TypeOfEstablishment,
            establishment.TypeOfEstablishmentId);

        establishment.AdmissionPolicy = _lookupService.GetLookupValue(
            LookupTypes.AdmissionsPolicy,
            establishment.AdmissionsPolicyId);

        establishment.DistrictAdministrativeName = _lookupService.GetLookupValue(
            LookupTypes.DistrictAdministrative,
            establishment.DistrictAdministrativeId);

        establishment.PhaseOfEducationName = _lookupService.GetLookupValue(
            LookupTypes.PhaseOfEducation,
            establishment.PhaseOfEducationId);

        establishment.GenderName = _lookupService.GetLookupValue(
            LookupTypes.Gender,
            establishment.GenderId);

        establishment.ReligiousCharacterName = _lookupService.GetLookupValue(
            LookupTypes.ReligiousCharacter,
            establishment.ReligiousCharacterId);

        establishment.UrbanRuralName = _lookupService.GetLookupValue(
            LookupTypes.UrbanRural,
            establishment.UrbanRuralId);

        establishment.TrustName = _lookupService.GetLookupValue(
            LookupTypes.Trusts,
            establishment.TrustsId);

        establishment.LANAme = _lookupService.GetLookupValue(
            LookupTypes.LA,
            establishment.LAId);
    }
}