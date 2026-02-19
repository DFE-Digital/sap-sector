using SAPSec.Core.Model;

namespace SAPSec.Core.Tests.Features.SimilarSchools.UseCases;

public class SchoolDetailsBuilder(string urn)
{
    private string? _name = null;

    public SchoolDetailsBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public SchoolDetails Build()
    {
        return new SchoolDetails
        {
            Urn = urn,
            Name = _name ?? "",
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
            Website = DataWithAvailability.NotAvailable<string>(),
            Telephone = DataWithAvailability.NotAvailable<string>(),
            Email = DataWithAvailability.NotAvailable<string>(),
        };
    }
}