using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Services
{
    public class EstablishmentService : IEstablishmentService
    {
        private readonly IEstablishmentRepository _establishmentRepository;
        private readonly ILookupService _lookUpService;


        public EstablishmentService(
            IEstablishmentRepository establishmentRepository,
            ILookupService lookUpService)
        {
            _establishmentRepository = establishmentRepository;
            _lookUpService = lookUpService;
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
                var allLookups = _lookUpService.GetAllLookups();
                establishment.TypeOfEstablishmentName = GetLookupByCode(allLookups, "TypeOfEstablishment", establishment.TypeOfEstablishmentId);
                establishment.AdmissionPolicy = GetLookupByCode(allLookups, "AdmissionsPolicy", establishment.AdmissionsPolicyId);
                establishment.DistrictAdministrativeName = GetLookupByCode(allLookups, "DistrictAdministrative", establishment.DistrictAdministrativeId);
                establishment.PhaseOfEducationName = GetLookupByCode(allLookups, "PhaseOfEducation", establishment.PhaseOfEducationId);
                establishment.GenderName = GetLookupByCode(allLookups, "Gender", establishment.GenderId);
                establishment.ReligiousCharacterName = GetLookupByCode(allLookups, "ReligiousCharacter", establishment.ReligiousCharacterId);
                establishment.ResourcedProvisionName = GetLookupByCode(allLookups, "ResourcedProvision", establishment.ResourcedProvision);
                establishment.UrbanRuralName = GetLookupByCode(allLookups, "UrbanRural", establishment.UrbanRuralId);
                establishment.TrustName = GetLookupByCode(allLookups, "Trusts", establishment.TrustsId);
                establishment.LANAme = GetLookupByCode(allLookups, "LA", establishment.LAId);
                return establishment;
            }
            //_logger.LogError($"Error looking up establishment with urn {urn}");
            throw new Exception("Error in GetEstablishment");
        }

        public Establishment GetEstablishmentByAnyNumber(string number)
        {
            var establishment = _establishmentRepository.GetEstablishmentByAnyNumber(number);
            if (!string.IsNullOrWhiteSpace(establishment?.URN))
            {
                var allLookups = _lookUpService.GetAllLookups();
                establishment.TypeOfEstablishmentName = GetLookupByCode(allLookups, "TypeOfEstablishment", establishment.TypeOfEstablishmentId);
                establishment.AdmissionPolicy = GetLookupByCode(allLookups, "AdmissionsPolicy", establishment.AdmissionsPolicyId);
                establishment.DistrictAdministrativeName = GetLookupByCode(allLookups, "DistrictAdministrative", establishment.DistrictAdministrativeId);
                establishment.PhaseOfEducationName = GetLookupByCode(allLookups, "PhaseOfEducation", establishment.PhaseOfEducationId);
                establishment.GenderName = GetLookupByCode(allLookups, "Gender", establishment.GenderId);
                establishment.ReligiousCharacterName = GetLookupByCode(allLookups, "ReligiousCharacter", establishment.ReligiousCharacterId);
                establishment.ResourcedProvisionName = GetLookupByCode(allLookups, "ResourcedProvision", establishment.ResourcedProvision);
                establishment.UrbanRuralName = GetLookupByCode(allLookups, "UrbanRural", establishment.UrbanRuralId);
                establishment.TrustName = GetLookupByCode(allLookups, "Trusts", establishment.TrustsId);
                establishment.LANAme = GetLookupByCode(allLookups, "LA", establishment.LAId);
                return establishment;
            }
            //_logger.LogError($"Error looking up establishment with urn {urn}");
            return new Establishment();
        }

        private string GetLookupByCode(IEnumerable<Lookup> lookups, string type, string? id)
        {
            return lookups.FirstOrDefault(x => x.LookupType == type && x.Id == id)?.Name ?? string.Empty;
        }

        private string GetLookupByCode(IEnumerable<Lookup> lookups, string type, int? id)
        {
            return lookups.FirstOrDefault(x => x.LookupType == type && x.Id == id.ToString())?.Name ?? string.Empty;
        }
    }
}
