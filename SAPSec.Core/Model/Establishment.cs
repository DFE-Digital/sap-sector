using SAPSec.Core.Model.KS4.Absence;
using SAPSec.Core.Model.KS4.Destinations;
using SAPSec.Core.Model.KS4.Performance;
using SAPSec.Core.Model.KS4.SubjectEntries;
using SAPSec.Core.Model.KS4.Workforce;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Model
{
    [ExcludeFromCodeCoverage]
    public class Establishment
    {
        public string URN { get; set; } = string.Empty;

        #region MetaData
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

        // Also known as LA/Estab, for obvious reasons
        public string DfENumber => $"{LAId}/{EstablishmentNumber}";

        #endregion

        public EstablishmentPerformance KS4Performance { get; set; } = new();
        public LAPerformance LAPerformance { get; set; } = new();
        public EnglandPerformance EnglandPerformance { get; set; } = new();

        public EstablishmentSubjectEntries KS4SubjectEntries { get; set; } = new();
        public LASubjectEntries LASubjectEntries { get; set; } = new();
        public EnglandSubjectEntries EnglandSubjectEntries { get; set; } = new();

        public EstablishmentDestinations EstablishmentDestinations { get; set; } = new();
        public LADestinations LADestinations { get; set; } = new();
        public EnglandDestinations EnglandDestinations { get; set; } = new();


        public EstablishmentAbsence EstablishmentAbsence { get; set; } = new();
        public LAAbsence LAAbsence { get; set; } = new();
        public EnglandAbsence Absence { get; set; } = new(); // Will eventually need one per phase

        public EstablishmentWorkforce Workforce { get; set; } = new(); // Will eventually need one per phase



    }
}
