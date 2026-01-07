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
    public string AdmissionsPolicy => GetValueOrNoData(_school.AdmissionPolicy);
    public string SendIntegratedResource => FormatSendResource();
    public string ReligiousCharacter => GetValueOrNoData(_school.ReligiousCharacterName);

    // Contact Details Section
    public string HeadteacherPrincipal => FormatHeadteacher();
    public string Website => GetValueOrNoData(_school.Website);
    public string WebsiteUrl => FormatWebsiteUrl();
    public bool HasWebsite => !string.IsNullOrWhiteSpace(_school.Website);
    public string Telephone => GetValueOrNoData(_school.TelephoneNum);
    public string Email => FormatEmail();
    public bool HasEmail => !string.IsNullOrWhiteSpace(_school.Website); 

    public string OfstedReportUrl => $"https://reports.ofsted.gov.uk/provider/23/{_school.URN}";
    public bool HasOfstedReport => !string.IsNullOrWhiteSpace(_school.URN);

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
        return NoDataAvailable; // e.g., "11 to 16"
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

    private string FormatEmail()
    {
        return NoDataAvailable;
    }

    private string FormatSendResource()
    {
        if (string.IsNullOrWhiteSpace(_school.ResourcedProvision))
            return NoDataAvailable;

        return _school.ResourcedProvision;
    }

    private string DetermineGovernanceStructure()
    {
        if (!string.IsNullOrWhiteSpace(_school.TrustsId))
            return "Multi-academy trust (MAT)";

        return NoDataAvailable;
    }
}