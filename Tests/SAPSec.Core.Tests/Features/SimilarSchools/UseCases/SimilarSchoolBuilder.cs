using SAPSec.Core.Features.Geography;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Model;

namespace SAPSec.Core.Tests.Features.SimilarSchools.UseCases;

public class SimilarSchoolBuilder(string urn)
{
    private string? _name = null;
    private ReferenceData? _localAuthority = null;
    private ReferenceData? _region = null;
    private ReferenceData? _urbanRural = null;
    private ReferenceData? _phaseOfEducation = null;
    private ReferenceData? _officialSixthForm = null;
    private ReferenceData? _admissionsPolicy = null;
    private ReferenceData? _gender = null;
    private ReferenceData? _resourcedProvision = null;
    private ReferenceData? _typeOfEstablishment = null;
    private ReferenceData? _establishmentTypeGroup = null;
    private ReferenceData? _trustSchoolFlag = null;
    private BNGCoordinates? _coordinates { get; set; }
    private DataWithAvailability<decimal>? _attainment8 = null;
    private DataWithAvailability<decimal>? _biology = null;
    private DataWithAvailability<decimal>? _chemistry = null;
    private DataWithAvailability<decimal>? _combinedScience = null;
    private DataWithAvailability<decimal>? _englishLanguage = null;
    private DataWithAvailability<decimal>? _englishLiterature = null;
    private DataWithAvailability<decimal>? _englishMaths = null;
    private DataWithAvailability<decimal>? _maths = null;
    private DataWithAvailability<decimal>? _physics = null;

    public SimilarSchoolBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public SimilarSchoolBuilder WithLocalAuthority(string id, string name)
    {
        _localAuthority = new(id, name);
        return this;
    }

    public SimilarSchoolBuilder WithRegion(string id, string name)
    {
        _region = new(id, name);
        return this;
    }

    public SimilarSchoolBuilder WithUrbanRural(string id, string name)
    {
        _urbanRural = new(id, name);
        return this;
    }

    public SimilarSchoolBuilder WithPhaseOfEducation(string id, string name)
    {
        _phaseOfEducation = new(id, name);
        return this;
    }

    public SimilarSchoolBuilder WithOfficialSixthForm(string id, string name)
    {
        _officialSixthForm = new(id, name);
        return this;
    }

    public SimilarSchoolBuilder WithAdmissionsPolicy(string id, string name)
    {
        _admissionsPolicy = new(id, name);
        return this;
    }

    public SimilarSchoolBuilder WithGender(string id, string name)
    {
        _gender = new(id, name);
        return this;
    }

    public SimilarSchoolBuilder WithResourcedProvision(string id, string name)
    {
        _resourcedProvision = new(id, name);
        return this;
    }

    public SimilarSchoolBuilder WithTypeOfEstablishment(string id, string name)
    {
        _typeOfEstablishment = new(id, name);
        return this;
    }

    public SimilarSchoolBuilder WithEstablishmentTypeGroup(string id, string name)
    {
        _establishmentTypeGroup = new(id, name);
        return this;
    }

    public SimilarSchoolBuilder WithTrustSchoolFlag(string id, string name)
    {
        _trustSchoolFlag = new(id, name);
        return this;
    }

    public SimilarSchoolBuilder WithCoordinates(double easting, double northing)
    {
        _coordinates = new(easting, northing);
        return this;
    }

    public SimilarSchoolBuilder WithAttainment8(DataWithAvailability<decimal> value)
    {
        _attainment8 = value;
        return this;
    }

    public SimilarSchoolBuilder WithBiology(DataWithAvailability<decimal> value)
    {
        _biology = value;
        return this;
    }

    public SimilarSchoolBuilder WithChemistry(DataWithAvailability<decimal> value)
    {
        _chemistry = value;
        return this;
    }

    public SimilarSchoolBuilder WithCombinedScience(DataWithAvailability<decimal> value)
    {
        _combinedScience = value;
        return this;
    }

    public SimilarSchoolBuilder WithEnglishLanguage(DataWithAvailability<decimal> value)
    {
        _englishLanguage = value;
        return this;
    }

    public SimilarSchoolBuilder WithEnglishLiterature(DataWithAvailability<decimal> value)
    {
        _englishLiterature = value;
        return this;
    }

    public SimilarSchoolBuilder WithEnglishMaths(DataWithAvailability<decimal> value)
    {
        _englishMaths = value;
        return this;
    }

    public SimilarSchoolBuilder WithMaths(DataWithAvailability<decimal> value)
    {
        _maths = value;
        return this;
    }

    public SimilarSchoolBuilder WithPhysics(DataWithAvailability<decimal> value)
    {
        _physics = value;
        return this;
    }

    public SimilarSchool Build()
    {
        return new SimilarSchool
        {
            URN = urn,
            Name = _name ?? "",
            Address = new Address
            {
                Street = "",
                Locality = "",
                Address3 = "",
                Town = "",
                Postcode = ""
            },
            TotalCapacity = "",
            TotalPupils = "",
            NurseryProvisionName = "",
            Coordinates = _coordinates,
            LocalAuthority = _localAuthority ?? new("", ""),
            UrbanRural = _urbanRural ?? new("", ""),
            Region = _region ?? new("", ""),
            AdmissionsPolicy = _admissionsPolicy ?? new("", ""),
            PhaseOfEducation = _phaseOfEducation ?? new("", ""),
            Gender = _gender ?? new("", ""),
            TypeOfEstablishment = _typeOfEstablishment ?? new("", ""),
            EstablishmentTypeGroup = _establishmentTypeGroup ?? new("", ""),
            TrustSchoolFlag = _trustSchoolFlag ?? new("", ""),
            OfficialSixthForm = _officialSixthForm ?? new("", ""),
            ResourcedProvision = _resourcedProvision ?? new("", ""),
            Attainment8Score = _attainment8 ?? DataWithAvailability.NotAvailable<decimal>(),
            BiologyGcseGrade5AndAbovePercentage = _biology ?? DataWithAvailability.NotAvailable<decimal>(),
            ChemistryGcseGrade5AndAbovePercentage = _chemistry ?? DataWithAvailability.NotAvailable<decimal>(),
            CombinedScienceGcseGrade55AndAbovePercentage = _combinedScience ?? DataWithAvailability.NotAvailable<decimal>(),
            EnglishLanguageGcseGrade5AndAbovePercentage = _englishLanguage ?? DataWithAvailability.NotAvailable<decimal>(),
            EnglishLiteratureGcseGrade5AndAbovePercentage = _englishLiterature ?? DataWithAvailability.NotAvailable<decimal>(),
            EnglishMathsGcseGrade5AndAbovePercentage = _englishMaths ?? DataWithAvailability.NotAvailable<decimal>(),
            MathsGcseGrade5AndAbovePercentage = _maths ?? DataWithAvailability.NotAvailable<decimal>(),
            PhysicsGcseGrade5AndAbovePercentage = _physics ?? DataWithAvailability.NotAvailable<decimal>(),
        };
    }
}
