using Moq;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Model.KS4.Performance;

namespace SAPSec.Core.Tests.Features.Ks4HeadlineMeasures;

public class GetKs4HeadlineMeasuresTests
{
    [Fact]
    public async Task Execute_WhenDataExists_ReturnsThreeYearAverages()
    {
        var schoolDetailsServiceMock = new Mock<ISchoolDetailsService>();
        var repositoryMock = new Mock<IKs4PerformanceRepository>();

        schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrnAsync("123456"))
            .ReturnsAsync(CreateSchoolDetails("123456"));

        repositoryMock
            .Setup(x => x.GetByUrnAsync("123456"))
            .ReturnsAsync(new Ks4HeadlineMeasuresData(
                new EstablishmentPerformance
                {
                    Attainment8_Tot_Est_Current_Num = "45.0",
                    Attainment8_Tot_Est_Previous_Num = "46.0",
                    Attainment8_Tot_Est_Previous2_Num = "48.0"
                },
                new LAPerformance
                {
                    Attainment8_Tot_LA_Current_Num = 44.0,
                    Attainment8_Tot_LA_Previous_Num = 45.0,
                    Attainment8_Tot_LA_Previous2_Num = 46.0
                },
                new EnglandPerformance
                {
                    Attainment8_Tot_Eng_Current_Num = 45.9,
                    Attainment8_Tot_Eng_Previous_Num = 46.1,
                    Attainment8_Tot_Eng_Previous2_Num = 46.4
                },
                1200,
                10844860));

        var sut = new GetKs4HeadlineMeasures(repositoryMock.Object, schoolDetailsServiceMock.Object);

        var result = await sut.Execute(new GetKs4HeadlineMeasuresRequest("123456"));

        result.Should().NotBeNull();
        result!.Attainment8ThreeYearAverage.SchoolValue.Should().Be(46.3m);
        result.Attainment8ThreeYearAverage.LocalAuthorityValue.Should().Be(45.0m);
        result.Attainment8ThreeYearAverage.EnglandValue.Should().Be(46.1m);
    }

    [Fact]
    public async Task Execute_WhenSchoolMissing_ReturnsNull()
    {
        var schoolDetailsServiceMock = new Mock<ISchoolDetailsService>();
        var repositoryMock = new Mock<IKs4PerformanceRepository>();

        schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrnAsync("999999"))
            .ReturnsAsync((SchoolDetails?)null);

        var sut = new GetKs4HeadlineMeasures(repositoryMock.Object, schoolDetailsServiceMock.Object);

        var result = await sut.Execute(new GetKs4HeadlineMeasuresRequest("999999"));

        result.Should().BeNull();
    }

    private static SchoolDetails CreateSchoolDetails(string urn)
    {
        return new SchoolDetails
        {
            Name = DataWithAvailability.Available("Test School"),
            Urn = DataWithAvailability.Available(urn),
            DfENumber = DataWithAvailability.Available("001/1234"),
            Ukprn = DataWithAvailability.Available("10000000"),
            Address = DataWithAvailability.Available("Test Address"),
            LocalAuthorityName = DataWithAvailability.Available("Test LA"),
            LocalAuthorityCode = DataWithAvailability.Available("001"),
            Region = DataWithAvailability.Available("Test Region"),
            UrbanRuralDescription = DataWithAvailability.Available("Urban"),
            AgeRangeLow = DataWithAvailability.Available(11),
            AgeRangeHigh = DataWithAvailability.Available(16),
            GenderOfEntry = DataWithAvailability.Available("Mixed"),
            PhaseOfEducation = DataWithAvailability.Available("Secondary"),
            SchoolType = DataWithAvailability.Available("Academy"),
            AdmissionsPolicy = DataWithAvailability.Available("Not selective"),
            ReligiousCharacter = DataWithAvailability.Available("None"),
            GovernanceStructure = DataWithAvailability.Available(GovernanceType.MultiAcademyTrust),
            AcademyTrustName = DataWithAvailability.Available("Test Trust"),
            AcademyTrustId = DataWithAvailability.Available("5000"),
            HasNurseryProvision = DataWithAvailability.Available(false),
            HasSixthForm = DataWithAvailability.Available(false),
            HasSenUnit = DataWithAvailability.Available(false),
            HasResourcedProvision = DataWithAvailability.Available(false),
            HeadteacherName = DataWithAvailability.Available("Head Teacher"),
            Website = DataWithAvailability.Available("https://example.test"),
            Telephone = DataWithAvailability.Available("0123456789"),
            Email = DataWithAvailability.NotAvailable<string>()
        };
    }
}
