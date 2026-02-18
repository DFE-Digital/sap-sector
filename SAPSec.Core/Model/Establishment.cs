using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Core.Model
{
    [ExcludeFromCodeCoverage]
    public class Establishment
    {
        public string URN { get; set; } = string.Empty;
        public string LAId { get; set; } = string.Empty;
        public string LAName { get; set; } = string.Empty;
        public string RegionId { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public string EstablishmentName { get; set; } = string.Empty;
        public string EstablishmentNumber { get; set; } = string.Empty;
        public string TrustsId { get; set; } = string.Empty;
        public string TrustName { get; set; } = string.Empty;
        public string AdmissionsPolicyId { get; set; } = string.Empty;
        public string AdmissionsPolicyName { get; set; } = string.Empty;
        public string DistrictAdministrativeId { get; set; } = string.Empty;
        public string DistrictAdministrativeName { get; set; } = string.Empty;
        public string PhaseOfEducationId { get; set; } = string.Empty;
        public string PhaseOfEducationName { get; set; } = string.Empty;
        public string GenderId { get; set; } = string.Empty;
        public string GenderName { get; set; } = string.Empty;
        public string ReligiousCharacterId { get; set; } = string.Empty;
        public string ReligiousCharacterName { get; set; } = string.Empty;
        public string TelephoneNum { get; set; } = string.Empty;
        public string TotalCapacity { get; set; } = string.Empty;
        public string TotalPupils { get; set; } = string.Empty;
        public string TypeOfEstablishmentId { get; set; } = string.Empty;
        public string TypeOfEstablishmentName { get; set; } = string.Empty;
        public string EstablishmentTypeGroupId { get; set; } = string.Empty;
        public string EstablishmentTypeGroupName { get; set; } = string.Empty;
        public string TrustSchoolFlagId { get; set; } = string.Empty;
        public string TrustSchoolFlagName { get; set; } = string.Empty;
        public string OfficialSixthFormId { get; set; } = string.Empty;
        public string OfficialSixthFormName { get; set; } = string.Empty;
        public string NurseryProvisionName { get; set; } = string.Empty;
        public string ResourcedProvisionId { get; set; } = string.Empty;
        public string ResourcedProvisionName { get; set; } = string.Empty;
        public string UKPRN { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Locality { get; set; } = string.Empty;
        public string Address3 { get; set; } = string.Empty;
        public string Town { get; set; } = string.Empty;
        public string County { get; set; } = string.Empty;
        public string Postcode { get; set; } = string.Empty;
        public string HeadTitle { get; set; } = string.Empty;
        public string HeadFirstName { get; set; } = string.Empty;
        public string HeadLastName { get; set; } = string.Empty;
        public string HeadPreferredJobTitle { get; set; } = string.Empty;
        public string UrbanRuralId { get; set; } = string.Empty;
        public string UrbanRuralName { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Easting { get; set; } = string.Empty;
        public string Northing { get; set; } = string.Empty;
        public string AgeRangeLow { get; set; } = string.Empty;
        public string AgeRangeRange { get; set; } = string.Empty;


        public string? Latitude { get; set; }
        public string? Longitude { get; set; }

        // Also known as LA/Estab, for obvious reasons
        public string DfENumber => $"{LAId}/{EstablishmentNumber}";
        public string DfENumberSearchable => $"{LAId}{EstablishmentNumber}";
    }
}
