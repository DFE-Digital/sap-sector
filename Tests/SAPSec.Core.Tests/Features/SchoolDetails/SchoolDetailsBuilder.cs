using SAPSec.Core.Features.Availability;
using SAPSec.Core.Model;
using SD = SAPSec.Core.Features.SchoolDetails;

namespace SAPSec.Core.Tests.Features.SchoolDetails;

public class SchoolDetailsBuilder(string urn)
{
    private string? _name = null;
    private string? _telephone = null;
    private string? _website = null;
    private bool _showClosedSchoolBanner = false;
    private IReadOnlyList<SD.SuccessorSchool> _successors = [];
    private IReadOnlyList<SD.SuccessorSchool> _predecessors = [];

    public SchoolDetailsBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public SchoolDetailsBuilder WithTelephone(string telephone)
    {
        _telephone = telephone;
        return this;
    }

    public SchoolDetailsBuilder WithWebsite(string website)
    {
        _website = website;
        return this;
    }

    public SchoolDetailsBuilder WithClosedSchoolBanner(bool showClosedSchoolBanner = true)
    {
        _showClosedSchoolBanner = showClosedSchoolBanner;
        return this;
    }

    public SchoolDetailsBuilder WithSuccessors(params SD.SuccessorSchool[] successors)
    {
        _successors = successors;
        return this;
    }

    public SchoolDetailsBuilder WithPredecessors(params SD.SuccessorSchool[] predecessors)
    {
        _predecessors = predecessors;
        return this;
    }

    public SD.SchoolDetails Build()
    {
        return new SD.SchoolDetails
        {
            Urn = urn,
            Name = _name ?? "",
            ShowClosedSchoolBanner = _showClosedSchoolBanner,
            Successors = _successors,
            Predecessors = _predecessors,
            DfENumber = DataWithAvailability.NotAvailable<string>(),
            Ukprn = DataWithAvailability.NotAvailable<string>(),
            Address = DataWithAvailability.NotAvailable<string>(),
            LocalAuthorityName = DataWithAvailability.NotAvailable<string>(),
            LocalAuthorityCode = DataWithAvailability.NotAvailable<string>(),
            Region = DataWithAvailability.NotAvailable<string>(),
            UrbanRuralDescription = DataWithAvailability.NotAvailable<string>(),
            AgeRangeLow = DataWithAvailability.NotAvailable<int>(),
            AgeRangeHigh = DataWithAvailability.NotAvailable<int>(),
            GenderOfEntry = DataWithAvailability.NotAvailable<string>(),
            PhaseOfEducation = DataWithAvailability.NotAvailable<string>(),
            SchoolType = DataWithAvailability.NotAvailable<string>(),
            TypeOfEstablishmentCode = DataWithAvailability.NotAvailable<string>(),
            AdmissionsPolicy = DataWithAvailability.NotAvailable<string>(),
            ReligiousCharacter = DataWithAvailability.NotAvailable<string>(),
            GovernanceStructure = DataWithAvailability.NotAvailable<GovernanceType>(),
            AcademyTrustName = DataWithAvailability.NotAvailable<string>(),
            AcademyTrustId = DataWithAvailability.NotAvailable<string>(),
            HasNurseryProvision = DataWithAvailability.NotAvailable<bool>(),
            HasSixthForm = DataWithAvailability.NotAvailable<bool>(),
            HasSenUnit = DataWithAvailability.NotAvailable<bool>(),
            HasResourcedProvision = DataWithAvailability.NotAvailable<bool>(),
            HeadteacherName = DataWithAvailability.NotAvailable<string>(),
            Website = _website is null ? DataWithAvailability.NotAvailable<string>() : DataWithAvailability.Available(_website),
            Telephone = _telephone is null ? DataWithAvailability.NotAvailable<string>() : DataWithAvailability.Available(_telephone),
            Email = DataWithAvailability.NotAvailable<string>(),
        };
    }
}
