using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Model
{
    [ExcludeFromCodeCoverage]
    public class EstablishmentMetadata
    {
        public string Id { get; set; } = string.Empty;
        public int EstablishmentNumber { get; set; }
        public string EstablishmentName { get; set; } = string.Empty;

        public int? TrustsId { get; set; }
        public string TrustName { get; set; } = string.Empty;

        public int? AdmissionsPolicyId { get; set; }
        public string AdmissionPolicy { get; set; } = string.Empty;
        public string DistrictAdministrativeId { get; set; } = string.Empty;
        public string DistrictAdministrativeName { get; set; } = string.Empty;
        public int? PhaseOfEducationId { get; set; }
        public string PhaseOfEducationName { get; set; } = string.Empty;
        public int? GenderId { get; set; }
        public string GenderName { get; set; } = string.Empty;
        public int? OfficialSixthFormId { get; set; }
        public int? LAId { get; set; }
        public string LANAme { get; set; } = string.Empty;
        public int? ReligiousCharacterId { get; set; }
        public string ReligiousCharacterName { get; set; } = string.Empty;
        public string TelephoneNum { get; set; } = string.Empty;
        public int TotalPupils { get; set; }
        public int? TypeOfEstablishmentId { get; set; }
        public string TypeOfEstablishmentName { get; set; } = string.Empty;
        public int? ResourcedProvision { get; set; }
        public string ResourcedProvisionName { get; set; } = string.Empty;
        public int UKPRN { get; set; }
        public string UrbanRuralId { get; set; } = string.Empty;
        public string UrbanRuralName { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public int? Easting { get; set; }
        public int? Northing { get; set; }

    }
}
