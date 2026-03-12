using Moq;
using SAPSec.Core;
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
                    Attainment8_Tot_LA_Current_Num = "44.0",
                    Attainment8_Tot_LA_Previous_Num = "45.0",
                    Attainment8_Tot_LA_Previous2_Num = "46.0"
                },
                new EnglandPerformance
                {
                    Attainment8_Tot_Eng_Current_Num = "45.9",
                    Attainment8_Tot_Eng_Previous_Num = "46.1",
                    Attainment8_Tot_Eng_Previous2_Num = "46.4"
                }));

        var sut = new GetKs4HeadlineMeasures(repositoryMock.Object, schoolDetailsServiceMock.Object);

        var result = await sut.Execute(new GetKs4HeadlineMeasuresRequest("123456"));

        result.Should().NotBeNull();
        result!.Attainment8ThreeYearAverage.SchoolValue.Should().Be(46.3m);
        result.Attainment8ThreeYearAverage.LocalAuthorityValue.Should().Be(45.0m);
        result.Attainment8ThreeYearAverage.EnglandValue.Should().Be(46.1m);
    }

    [Fact]
    public async Task Execute_WhenSchoolMissing_ThrowsNotFoundException()
    {
        var schoolDetailsServiceMock = new Mock<ISchoolDetailsService>();
        var repositoryMock = new Mock<IKs4PerformanceRepository>();

        schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrnAsync("999999"))
            .ReturnsAsync((SchoolDetails?)null);

        var sut = new GetKs4HeadlineMeasures(repositoryMock.Object, schoolDetailsServiceMock.Object);

        var act = async () => await sut.Execute(new GetKs4HeadlineMeasuresRequest("999999"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999999*");
    }
    [Fact]
    public async Task Execute_WhenLaAndEnglandContainNonNumericValues_TreatsValuesAsMissing()
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
                    Attainment8_Tot_LA_Current_Num = "c",
                    Attainment8_Tot_LA_Previous_Num = "s",
                    Attainment8_Tot_LA_Previous2_Num = "x"
                },
                new EnglandPerformance
                {
                    Attainment8_Tot_Eng_Current_Num = "c",
                    Attainment8_Tot_Eng_Previous_Num = "s",
                    Attainment8_Tot_Eng_Previous2_Num = "x"
                }));

        var sut = new GetKs4HeadlineMeasures(repositoryMock.Object, schoolDetailsServiceMock.Object);

        var result = await sut.Execute(new GetKs4HeadlineMeasuresRequest("123456"));

        result.Should().NotBeNull();
        result!.Attainment8ThreeYearAverage.SchoolValue.Should().Be(46.3m);
        result.Attainment8ThreeYearAverage.LocalAuthorityValue.Should().BeNull();
        result.Attainment8ThreeYearAverage.EnglandValue.Should().BeNull();
    }

    [Fact]
    public async Task Execute_WhenRepositoryReturnsNullData_ReturnsResponseWithNullMeasures()
    {
        var schoolDetailsServiceMock = new Mock<ISchoolDetailsService>();
        var repositoryMock = new Mock<IKs4PerformanceRepository>();

        schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrnAsync("123456"))
            .ReturnsAsync(CreateSchoolDetails("123456"));

        repositoryMock
            .Setup(x => x.GetByUrnAsync("123456"))
            .ReturnsAsync((Ks4HeadlineMeasuresData?)null);

        var sut = new GetKs4HeadlineMeasures(repositoryMock.Object, schoolDetailsServiceMock.Object);

        var result = await sut.Execute(new GetKs4HeadlineMeasuresRequest("123456"));

        result.Should().NotBeNull();
        result.Attainment8ThreeYearAverage.Should().Be(new Ks4HeadlineMeasureAverage(null, null, null));
        result.Attainment8YearByYear.School.Should().Be(new Ks4HeadlineMeasureSeries(null, null, null));
        result.Attainment8YearByYear.LocalAuthority.Should().Be(new Ks4HeadlineMeasureSeries(null, null, null));
        result.Attainment8YearByYear.England.Should().Be(new Ks4HeadlineMeasureSeries(null, null, null));
        result.EngMaths49ThreeYearAverage.Should().Be(new Ks4HeadlineMeasureAverage(null, null, null));
        result.EngMaths49YearByYear.School.Should().Be(new Ks4HeadlineMeasureSeries(null, null, null));
        result.EngMaths49YearByYear.LocalAuthority.Should().Be(new Ks4HeadlineMeasureSeries(null, null, null));
        result.EngMaths49YearByYear.England.Should().Be(new Ks4HeadlineMeasureSeries(null, null, null));
        result.EngMaths59ThreeYearAverage.Should().Be(new Ks4HeadlineMeasureAverage(null, null, null));
        result.EngMaths59YearByYear.School.Should().Be(new Ks4HeadlineMeasureSeries(null, null, null));
        result.EngMaths59YearByYear.LocalAuthority.Should().Be(new Ks4HeadlineMeasureSeries(null, null, null));
        result.EngMaths59YearByYear.England.Should().Be(new Ks4HeadlineMeasureSeries(null, null, null));
    }

    [Fact]
    public async Task Execute_WhenDataContainsMixedNumericAndStringValues_MapsAllResponseProperties()
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
                    Attainment8_Tot_Est_Previous_Num = "",
                    Attainment8_Tot_Est_Previous2_Num = "47.0",
                    EngMaths49_Tot_Est_Current_Pct = "66.2",
                    EngMaths49_Tot_Est_Previous_Pct = "x",
                    EngMaths49_Tot_Est_Previous2_Pct = "67.0",
                    EngMaths59_Tot_Est_Current_Pct = "44.1",
                    EngMaths59_Tot_Est_Previous_Pct = "45.2",
                    EngMaths59_Tot_Est_Previous2_Pct = "n/a"
                },
                new LAPerformance
                {
                    Attainment8_Tot_LA_Current_Num = "44.0",
                    Attainment8_Tot_LA_Previous_Num = "45.0",
                    Attainment8_Tot_LA_Previous2_Num = "46.0",
                    EngMaths49_Tot_LA_Current_Pct = "64.0",
                    EngMaths49_Tot_LA_Previous_Pct = "65.0",
                    EngMaths49_Tot_LA_Previous2_Pct = "66.0",
                    EngMaths59_Tot_LA_Current_Pct = "42.0",
                    EngMaths59_Tot_LA_Previous_Pct = "43.0",
                    EngMaths59_Tot_LA_Previous2_Pct = "44.0"
                },
                new EnglandPerformance
                {
                    Attainment8_Tot_Eng_Current_Num = "46.0",
                    Attainment8_Tot_Eng_Previous_Num = "46.2",
                    Attainment8_Tot_Eng_Previous2_Num = "c",
                    EngMaths49_Tot_Eng_Current_Pct = "68.0",
                    EngMaths49_Tot_Eng_Previous_Pct = "69.0",
                    EngMaths49_Tot_Eng_Previous2_Pct = "s",
                    EngMaths59_Tot_Eng_Current_Pct = "46.0",
                    EngMaths59_Tot_Eng_Previous_Pct = "47.0",
                    EngMaths59_Tot_Eng_Previous2_Pct = "x"
                }));

        var sut = new GetKs4HeadlineMeasures(repositoryMock.Object, schoolDetailsServiceMock.Object);

        var result = await sut.Execute(new GetKs4HeadlineMeasuresRequest("123456"));

        result.Should().NotBeNull();
        result.Attainment8ThreeYearAverage.Should().Be(new Ks4HeadlineMeasureAverage(46.0m, 45.0m, 46.1m));
        result.Attainment8YearByYear.School.Should().Be(new Ks4HeadlineMeasureSeries(45.0m, null, 47.0m));
        result.Attainment8YearByYear.LocalAuthority.Should().Be(new Ks4HeadlineMeasureSeries(44.0m, 45.0m, 46.0m));
        result.Attainment8YearByYear.England.Should().Be(new Ks4HeadlineMeasureSeries(46.0m, 46.2m, null));

        result.EngMaths49ThreeYearAverage.Should().Be(new Ks4HeadlineMeasureAverage(66.6m, 65.0m, 68.5m));
        result.EngMaths49YearByYear.School.Should().Be(new Ks4HeadlineMeasureSeries(66.2m, null, 67.0m));
        result.EngMaths49YearByYear.LocalAuthority.Should().Be(new Ks4HeadlineMeasureSeries(64.0m, 65.0m, 66.0m));
        result.EngMaths49YearByYear.England.Should().Be(new Ks4HeadlineMeasureSeries(68.0m, 69.0m, null));

        result.EngMaths59ThreeYearAverage.Should().Be(new Ks4HeadlineMeasureAverage(44.7m, 43.0m, 46.5m));
        result.EngMaths59YearByYear.School.Should().Be(new Ks4HeadlineMeasureSeries(44.1m, 45.2m, null));
        result.EngMaths59YearByYear.LocalAuthority.Should().Be(new Ks4HeadlineMeasureSeries(42.0m, 43.0m, 44.0m));
        result.EngMaths59YearByYear.England.Should().Be(new Ks4HeadlineMeasureSeries(46.0m, 47.0m, null));
    }
    private static SchoolDetails CreateSchoolDetails(string urn)
    {
        return new SchoolDetails
        {
            Name = "Test School",
            Urn = urn,
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


