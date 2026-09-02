using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Data.Dto;

namespace SAPSec.Test.Common.Builders;

public class EstablishmentBuilder(string urn, string name)
{
    string UkPrn = string.Empty;
    string PhaseOfEducationId = string.Empty;
    string PhaseOfEducationName = string.Empty;
    string EstablishmentStatusId = string.Empty;
    string EstablishmentStatusName = string.Empty;
    string LAId = string.Empty;
    string LAName = string.Empty;
    string Street = string.Empty;
    string Locality = string.Empty;
    string Address3 = string.Empty;
    string Town = string.Empty;
    string Postcode = string.Empty;
    string AdmissionsPolicyId = string.Empty;
    string AdmissionsPolicyName = string.Empty;
    string TrustId = string.Empty;
    string TrustName = string.Empty;
    string TrustSchoolFlagId = string.Empty;
    string TrustSchoolFlagName = string.Empty;
    string EstablishmentTypeGroupId = string.Empty;
    string EstablishmentTypeGroupName = string.Empty;
    string NurseryProvisionName = string.Empty;
    string OfficialSixthFormId = string.Empty;
    string OfficialSixthFormName = string.Empty;
    string ResourcedProvisionId = string.Empty;
    string ResourcedProvisionName = string.Empty;
    string GenderId = string.Empty;
    string GenderName = string.Empty;
    string ReligiousCharacterId = string.Empty;
    string ReligiousCharacterName = string.Empty;
    string HeadTitle = string.Empty;
    string HeadFirstName = string.Empty;
    string HeadLastName = string.Empty;
    string Website = string.Empty;
    string TelephoneNum = string.Empty;
    int? Easting = null;
    int? Northing = null;

    public EstablishmentBuilder Primary()
    {
        PhaseOfEducationId = PhaseOfEducationValues.PrimaryId;
        PhaseOfEducationName = PhaseOfEducationValues.Primary;

        return this;
    }

    public EstablishmentBuilder Secondary()
    {
        PhaseOfEducationId = PhaseOfEducationValues.SecondaryId;
        PhaseOfEducationName = PhaseOfEducationValues.Secondary;

        return this;
    }

    public EstablishmentBuilder Open()
    {
        EstablishmentStatusId = EstablishmentStatusValues.OpenId;
        EstablishmentStatusName = EstablishmentStatusValues.Open;

        return this;
    }

    public EstablishmentBuilder WithUkPrn(string ukPrn)
    {
        UkPrn = ukPrn;

        return this;
    }

    public EstablishmentBuilder WithAddress(
        string street,
        string locality,
        string address3,
        string town,
        string postcode)
    {
        Street = street;
        Locality = locality;
        Address3 = address3;
        Town = town;
        Postcode = postcode;

        return this;
    }

    public EstablishmentBuilder WithEastingNorthing(int easting, int northing)
    {
        Easting = easting;
        Northing = northing;

        return this;
    }

    public EstablishmentBuilder InLA(string id, string? name = null)
    {
        LAId = id;
        LAName = name ?? string.Empty;

        return this;
    }

    public EstablishmentBuilder WithAdmissionsPolicy(string id, string? name = null)
    {
        AdmissionsPolicyId = id;
        AdmissionsPolicyName = name ?? string.Empty;

        return this;
    }

    public EstablishmentBuilder WithTrust(string id, string? name = null)
    {
        TrustId = id;
        TrustName = name ?? string.Empty;

        return this;
    }

    public EstablishmentBuilder WithTrustSchoolFlag(string id, string? name = null)
    {
        TrustSchoolFlagId = id;
        TrustSchoolFlagName = name ?? string.Empty;

        return this;
    }

    public EstablishmentBuilder WithEstablishmentTypeGroup(string id, string? name = null)
    {
        EstablishmentTypeGroupId = id;
        EstablishmentTypeGroupName = name ?? string.Empty;

        return this;
    }

    public EstablishmentBuilder WithNurseryProvisionName(string name)
    {
        NurseryProvisionName = name;

        return this;
    }

    public EstablishmentBuilder WithOfficialSixthForm(string id, string? name = null)
    {
        OfficialSixthFormId = id;
        OfficialSixthFormName = name ?? string.Empty;

        return this;
    }

    public EstablishmentBuilder WithResourcedProvision(string id, string? name = null)
    {
        ResourcedProvisionId = id;
        ResourcedProvisionName = name ?? string.Empty;

        return this;
    }

    public EstablishmentBuilder WithGender(string id, string? name = null)
    {
        GenderId = id;
        GenderName = name ?? string.Empty;

        return this;
    }

    public EstablishmentBuilder WithReligiousCharacter(string id, string? name = null)
    {
        ReligiousCharacterId = id;
        ReligiousCharacterName = name ?? string.Empty;

        return this;
    }

    public EstablishmentBuilder WithHeadTeacher(string title, string firstName, string lastName)
    {
        HeadTitle = title;
        HeadFirstName = firstName;
        HeadLastName = lastName;

        return this;
    }

    public EstablishmentBuilder WithWebsite(string website)
    {
        Website = website;

        return this;
    }

    public EstablishmentBuilder WithTelephone(string telephone)
    {
        TelephoneNum = telephone;

        return this;
    }

    public Establishment Build() =>
        new Establishment()
        {
            URN = urn,
            UKPRN = UkPrn,
            EstablishmentName = name,
            PhaseOfEducationId = PhaseOfEducationId,
            PhaseOfEducationName = PhaseOfEducationName,
            EstablishmentStatusId = EstablishmentStatusId,
            EstablishmentStatusName = EstablishmentStatusName,
            LAId = LAId,
            LAName = LAName,
            Street = Street,
            Locality = Locality,
            Address3 = Address3,
            Town = Town,
            Postcode = Postcode,
            Easting = Easting,
            Northing = Northing,
            AdmissionsPolicyId = AdmissionsPolicyId,
            AdmissionsPolicyName = AdmissionsPolicyName,
            TrustId = TrustId,
            TrustName = TrustName,
            TrustSchoolFlagId = TrustSchoolFlagId,
            TrustSchoolFlagName = TrustSchoolFlagName,
            EstablishmentTypeGroupId = EstablishmentTypeGroupId,
            EstablishmentTypeGroupName = EstablishmentTypeGroupName,
            NurseryProvisionName = NurseryProvisionName,
            OfficialSixthFormId = OfficialSixthFormId,
            OfficialSixthFormName = OfficialSixthFormName,
            ResourcedProvisionId = ResourcedProvisionId,
            ResourcedProvisionName = ResourcedProvisionName,
            GenderId = GenderId,
            GenderName = GenderName,
            ReligiousCharacterId = ReligiousCharacterId,
            ReligiousCharacterName = ReligiousCharacterName,
            HeadTitle = HeadTitle,
            HeadFirstName = HeadFirstName,
            HeadLastName = HeadLastName,
            Website = Website,
            TelephoneNum = TelephoneNum,
        };
}
