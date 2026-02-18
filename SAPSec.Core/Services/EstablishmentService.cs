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

    public async Task<IReadOnlyCollection<Establishment>> GetAllEstablishmentsAsync()
    {
        return await _establishmentRepository.GetAllEstablishmentsAsync();
    }

    public async Task<IReadOnlyCollection<Establishment>> GetEstablishmentsAsync(IEnumerable<string> urns)
    {
        return await _establishmentRepository.GetEstablishmentsAsync(urns);
    }

    public async Task<Establishment> GetEstablishmentAsync(string urn)
    {
        var establishment = await _establishmentRepository.GetEstablishmentAsync(urn);

        if (!string.IsNullOrWhiteSpace(establishment?.URN))
        {
            await EnrichEstablishment(establishment);
            return establishment;
        }

        throw new Exception("Error in GetEstablishment");
    }

    public async Task<Establishment> GetEstablishmentByAnyNumberAsync(string number)
    {
        var establishment = await _establishmentRepository.GetEstablishmentByAnyNumberAsync(number);

        if (!string.IsNullOrWhiteSpace(establishment?.URN))
        {
            await EnrichEstablishment(establishment);
            return establishment;
        }

        return new Establishment();
    }

    private async Task EnrichEstablishment(Establishment establishment)
    {
        establishment.TypeOfEstablishmentName = await _lookupService.GetLookupValueAsync(
            LookupTypes.TypeOfEstablishment,
            establishment.TypeOfEstablishmentId);

        establishment.AdmissionsPolicyId = await _lookupService.GetLookupValueAsync(
            LookupTypes.AdmissionsPolicy,
            establishment.AdmissionsPolicyId);

        establishment.DistrictAdministrativeName = await _lookupService.GetLookupValueAsync(
            LookupTypes.DistrictAdministrative,
            establishment.DistrictAdministrativeId);

        establishment.PhaseOfEducationName = await _lookupService.GetLookupValueAsync(
            LookupTypes.PhaseOfEducation,
            establishment.PhaseOfEducationId);

        establishment.GenderName = await _lookupService.GetLookupValueAsync(
            LookupTypes.Gender,
            establishment.GenderId);

        establishment.ReligiousCharacterName = await _lookupService.GetLookupValueAsync(
            LookupTypes.ReligiousCharacter,
            establishment.ReligiousCharacterId);

        establishment.UrbanRuralName = await _lookupService.GetLookupValueAsync(
            LookupTypes.UrbanRural,
            establishment.UrbanRuralId);

        establishment.TrustName = await _lookupService.GetLookupValueAsync(
            LookupTypes.Trusts,
            establishment.TrustsId);

        establishment.LAName = await _lookupService.GetLookupValueAsync(
            LookupTypes.LA,
            establishment.LAId);
    }
}