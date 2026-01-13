using SAPSec.Core.Model;

namespace SAPSec.Web.ViewModels;

public class SchoolViewModel
{
    private const string NoDataAvailable = "No available data";
    private readonly Establishment _school;

    public SchoolViewModel(Establishment school)
    {
        _school = school;
    }

    // Header
    public string Name => GetValueOrNoData(_school.EstablishmentName);

    // Location Section
    public string Address => FormatAddress();
    public string LocalAuthority => FormatLocalAuthority();
    public string Region => GetValueOrNoData(_school.DistrictAdministrativeName);
    public string UrbanRuralDescription => GetValueOrNoData(_school.UrbanRuralName);

    // School Details Section
    public string Id => FormatId();
    public string Urn => GetValueOrNoData(_school.URN);
    public string DfENumber => GetValueOrNoData(_school.DfENumber);
    public string Ukprn => GetValueOrNoData(_school.UKPRN);
    public string AgeRange => FormatAgeRange();
    public string GenderOfEntry => GetValueOrNoData(_school.GenderName);
    public string PhaseOfEducation => GetValueOrNoData(_school.PhaseOfEducationName);
    public string SchoolType => GetValueOrNoData(_school.TypeOfEstablishmentName);
    public string GovernanceStructure => DetermineGovernanceStructure();
    public string AcademyTrust => GetValueOrNoData(_school.TrustName);
    public string? AcademyTrustId => _school.TrustsId;
    public bool IsPartOfTrust => !string.IsNullOrWhiteSpace(_school.TrustsId)
                                  && !string.IsNullOrWhiteSpace(_school.TrustName);
    public bool IsPartOfLocalAuthority => _school.TypeOfEstablishmentId == "01";
    public string AdmissionsPolicy => GetValueOrNoData(_school.AdmissionPolicy);
    public string ReligiousCharacter => GetValueOrNoData(_school.ReligiousCharacterName);

    // Provision fields
    public string NurseryProvision => FormatNurseryProvision();
    public string SixthForm => FormatSixthForm();
    public string SenUnit => FormatSenUnit();
    public string ResourcedProvision => FormatResourcedProvision();

    // Contact Details Section
    public string HeadteacherPrincipal => FormatHeadteacher();
    public string Website => GetValueOrNoData(_school.Website);
    public string WebsiteUrl => FormatWebsiteUrl();
    public bool HasWebsite => !string.IsNullOrWhiteSpace(_school.Website);
    public string Telephone => GetValueOrNoData(_school.TelephoneNum);
    public string Email => FormatEmail();
    public bool HasEmail => !string.IsNullOrWhiteSpace(FormatEmailRaw());

    // Further Information Section
    public string OfstedReportUrl => $"https://reports.ofsted.gov.uk/provider/23/{_school.URN}";
    public bool HasOfstedReport => !string.IsNullOrWhiteSpace(_school.URN);

    // Information from other services
    public string FinancialBenchmarkingUrl => $"https://financial-benchmarking-and-insights-tool.education.gov.uk/school/{_school.URN}";
    public string GetInformationAboutSchoolsUrl => $"https://get-information-schools.service.gov.uk/Establishments/Establishment/Details/{_school.URN}";

    /// <summary>
    /// Returns the value if not null/empty, otherwise returns "No available data"
    /// </summary>
    private static string GetValueOrNoData(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? NoDataAvailable : value;
    }

    private string FormatAddress()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(_school.AddressStreet))
            parts.Add(_school.AddressStreet);
        if (!string.IsNullOrWhiteSpace(_school.AddressLocality))
            parts.Add(_school.AddressLocality);
        if (!string.IsNullOrWhiteSpace(_school.AddressAddress3))
            parts.Add(_school.AddressAddress3);
        if (!string.IsNullOrWhiteSpace(_school.AddressTown))
            parts.Add(_school.AddressTown);
        if (!string.IsNullOrWhiteSpace(_school.AddressPostcode))
            parts.Add(_school.AddressPostcode);

        return parts.Count > 0 ? string.Join(", ", parts) : NoDataAvailable;
    }

    private string FormatLocalAuthority()
    {
        if (string.IsNullOrWhiteSpace(_school.LANAme))
            return NoDataAvailable;

        if (!string.IsNullOrWhiteSpace(_school.LAId))
            return $"{_school.LANAme} ({_school.LAId})";

        return _school.LANAme;
    }

    private string FormatId()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(_school.URN))
            parts.Add($"URN: {_school.URN}");
        if (!string.IsNullOrWhiteSpace(_school.DfENumber) && _school.DfENumber != "/")
            parts.Add($"DfE number: {_school.DfENumber}");
        if (!string.IsNullOrWhiteSpace(_school.UKPRN))
            parts.Add($"UKPRN: {_school.UKPRN}");

        return parts.Count > 0 ? string.Join(", ", parts) : NoDataAvailable;
    }

    private string FormatAgeRange()
    {
        if (string.IsNullOrWhiteSpace(_school.AgeRangeLow))
            return NoDataAvailable;

        if (!string.IsNullOrWhiteSpace(_school.AgeRangeRange))
            return $"{_school.AgeRangeLow} to {_school.AgeRangeRange}";

        return _school.AgeRangeLow;
    }

    private string FormatHeadteacher()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(_school.HeadteacherTitle))
            parts.Add(_school.HeadteacherTitle);
        if (!string.IsNullOrWhiteSpace(_school.HeadteacherFirstName))
            parts.Add(_school.HeadteacherFirstName);
        if (!string.IsNullOrWhiteSpace(_school.HeadteacherLastName))
            parts.Add(_school.HeadteacherLastName);

        return parts.Count > 0 ? string.Join(" ", parts) : NoDataAvailable;
    }

    private string FormatWebsiteUrl()
    {
        if (string.IsNullOrWhiteSpace(_school.Website))
            return string.Empty;

        var website = _school.Website.Trim();

        if (!website.StartsWith("http://") && !website.StartsWith("https://"))
            return $"https://{website}";

        return website;
    }

    private string FormatEmailRaw()
    {
        // Return raw email if available in the model
        return string.Empty;
    }

    private string FormatEmail()
    {
        var email = FormatEmailRaw();
        return string.IsNullOrWhiteSpace(email) ? NoDataAvailable : email;
    }

    private string FormatNurseryProvision()
    {
        // Check PhaseOfEducation for nursery indication
        var phase = _school.PhaseOfEducationName?.ToLower() ?? "";

        if (phase.Contains("nursery"))
            return "Has nursery classes";

        if (phase.Contains("secondary") || phase.Contains("16 plus") || phase.Contains("post-16"))
            return "Does not have nursery classes";

        if (phase.Contains("primary"))
            return "Does not have nursery classes";

        return NoDataAvailable;
    }

    private string FormatSixthForm()
    {
        // OfficialSixthFormId: 1 = Has sixth form, 2 = Does not have sixth form, 0 = Not applicable
        return _school.OfficialSixthFormId switch
        {
            "1" => "Has a sixth form",
            "2" => "Does not have a sixth form",
            "0" => "Does not have a sixth form",
            _ => NoDataAvailable
        };
    }

    private string FormatSenUnit()
    {
        // Check ResourcedProvision field for SEN unit info
        var provision = _school.ResourcedProvision?.ToLower() ?? "";

        if (provision.Contains("sen unit"))
            return "Has a SEN unit";

        if (string.IsNullOrWhiteSpace(_school.ResourcedProvision) ||
            provision == "not applicable" ||
            provision == "none" ||
            provision == "")
            return "Does not have a SEN unit";

        // If there's only resourced provision but no SEN unit mentioned
        if (provision.Contains("resourced provision") && !provision.Contains("sen unit"))
            return "Does not have a SEN unit";

        return "Does not have a SEN unit";
    }

    private string FormatResourcedProvision()
    {
        var provision = _school.ResourcedProvision?.ToLower() ?? "";

        if (provision.Contains("resourced provision"))
            return "Has a resourced provision";

        if (string.IsNullOrWhiteSpace(_school.ResourcedProvision) ||
            provision == "not applicable" ||
            provision == "none" ||
            provision == "")
            return "Does not have a resourced provision";

        return "Does not have a resourced provision";
    }

    private string DetermineGovernanceStructure()
    {
        // Determine based on TrustsId or TypeOfEstablishment
        if (!string.IsNullOrWhiteSpace(_school.TrustsId))
            return "Multi-academy trust (MAT)";

        // Single academy trust
        var schoolType = _school.TypeOfEstablishmentName?.ToLower() ?? "";
        if (schoolType.Contains("academy") && string.IsNullOrWhiteSpace(_school.TrustsId))
            return "Single-academy trust (SAT)";

        // Maintained school
        if (_school.TypeOfEstablishmentId == "01")
            return "Local Authority (LA)";

        return NoDataAvailable;
    }

}