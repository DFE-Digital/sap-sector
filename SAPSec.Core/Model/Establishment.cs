using SAPSec.Core.Model.KS4.Absence;
using SAPSec.Core.Model.KS4.Destinations;
using SAPSec.Core.Model.KS4.Performance;
using SAPSec.Core.Model.KS4.SubjectEntries;
using SAPSec.Core.Model.KS4.Suspensions;
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
        public string EstablishmentNumber { get; set; } = string.Empty;
        public string EstablishmentName { get; set; } = string.Empty;


        public string HeadteacherTitle { get; set; } = string.Empty;
        public string HeadteacherFirstName { get; set; } = string.Empty;
        public string HeadteacherLastName { get; set; } = string.Empty;
        // Age range
        public string AgeRangeLow { get; set; } = string.Empty;
        public string AgeRangeRange { get; set; } = string.Empty;
        public string HeadteacherPreferredJobTitle { get; set; } = string.Empty;
        public string AddressStreet { get; set; } = string.Empty;
        public string AddressLocality { get; set; } = string.Empty;
        public string AddressAddress3 { get; set; } = string.Empty;
        public string AddressTown { get; set; } = string.Empty;
        public string AddressPostcode { get; set; } = string.Empty;


        public string? TrustsId { get; set; }
        public string TrustName { get; set; } = string.Empty;

        public string? AdmissionsPolicyId { get; set; }
        public string AdmissionPolicy { get; set; } = string.Empty;
        public string DistrictAdministrativeId { get; set; } = string.Empty;
        public string DistrictAdministrativeName { get; set; } = string.Empty;
        public string? PhaseOfEducationId { get; set; }
        public string PhaseOfEducationName { get; set; } = string.Empty;
        public string? GenderId { get; set; }
        public string GenderName { get; set; } = string.Empty;
        public string? OfficialSixthFormId { get; set; }
        public string? LAId { get; set; }
        public string LANAme { get; set; } = string.Empty;
        public string? ReligiousCharacterId { get; set; }
        public string ReligiousCharacterName { get; set; } = string.Empty;
        public string TelephoneNum { get; set; } = string.Empty;
        public string TotalPupils { get; set; } = string.Empty;
        public string? TypeOfEstablishmentId { get; set; }
        public string TypeOfEstablishmentName { get; set; } = string.Empty;
        public string ResourcedProvision { get; set; } = string.Empty;
        public string UKPRN { get; set; } = string.Empty;
        public string UrbanRuralId { get; set; } = string.Empty;
        public string UrbanRuralName { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string? Easting { get; set; }
        public string? Northing { get; set; }

        // Also known as LA/Estab, for obvious reasons
        public string DfENumber => $"{LAId}/{EstablishmentNumber}";
        public string DfENumberSearchable => $"{LAId}{EstablishmentNumber}";

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

        public EstablishmentSuspensions EstablishmentSuspensions { get; set; } = new();
        public LASuspensions LASuspensions { get; set; } = new();
        public EnglandSuspensions EnglandSuspensions { get; set; } = new();


        public EstablishmentAbsence EstablishmentAbsence { get; set; } = new();
        public LAAbsence LAAbsence { get; set; } = new();
        public EnglandAbsence EnglandAbsence { get; set; } = new(); 

        public EstablishmentWorkforce Workforce { get; set; } = new(); 



    }
}
