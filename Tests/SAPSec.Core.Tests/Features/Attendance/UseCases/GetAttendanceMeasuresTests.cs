using Moq;
using SAPSec.Core;
using SAPSec.Core.Features.Attendance;
using SAPSec.Core.Features.Attendance.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Core.Tests.Features.Attendance.UseCases;

public class GetAttendanceMeasuresTests
{
    [Fact]
    public async Task Execute_WhenSchoolMissing_ThrowsNotFoundException()
    {
        var schoolDetailsServiceMock = new Mock<ISchoolDetailsService>();
        var repositoryMock = new Mock<IAttendanceRepository>();

        schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrnAsync("999999"))
            .ReturnsAsync((SchoolDetails?)null);

        var sut = new GetAttendanceMeasures(repositoryMock.Object, schoolDetailsServiceMock.Object);

        var act = async () => await sut.Execute(new GetAttendanceMeasuresRequest("999999"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999999*");
    }

    [Fact]
    public async Task Execute_WhenDataExists_MapsOverallAndPersistentValues()
    {
        var schoolDetailsServiceMock = new Mock<ISchoolDetailsService>();
        var repositoryMock = new Mock<IAttendanceRepository>();

        schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrnAsync("123456"))
            .ReturnsAsync(CreateSchoolDetails("123456"));

        repositoryMock
            .Setup(x => x.GetByUrnAsync("123456"))
            .ReturnsAsync(new AttendanceMeasuresData(
                new EstablishmentAttendance
                {
                    Abs_Tot_Est_Current_Pct = 5.0m,
                    Abs_Tot_Est_Previous_Pct = 5.2m,
                    Abs_Tot_Est_Previous2_Pct = 5.4m,
                    Abs_Persistent_Est_Current_Pct = 16.0m,
                    Abs_Persistent_Est_Previous_Pct = 16.4m,
                    Abs_Persistent_Est_Previous2_Pct = 16.8m
                },
                new EnglandAttendance
                {
                    Abs_Tot_Eng_Current_Pct = 4.8m,
                    Abs_Tot_Eng_Previous_Pct = 4.9m,
                    Abs_Tot_Eng_Previous2_Pct = 5.0m,
                    Abs_Persistent_Eng_Current_Pct = 15.7m,
                    Abs_Persistent_Eng_Previous_Pct = 15.8m,
                    Abs_Persistent_Eng_Previous2_Pct = 16.0m
                },
                new[] { "2021 to 2022", "2022 to 2023", "2023 to 2024" }));

        var sut = new GetAttendanceMeasures(repositoryMock.Object, schoolDetailsServiceMock.Object);

        var result = await sut.Execute(new GetAttendanceMeasuresRequest("123456"));

        result.OverallAbsenceThreeYearAverage.Should().Be(new AttendanceMeasureAverage(5.2m, 4.9m));
        result.OverallAbsenceYearByYear.School.Should().Be(new AttendanceMeasureSeries(5.0m, 5.2m, 5.4m));
        result.OverallAbsenceYearByYear.England.Should().Be(new AttendanceMeasureSeries(4.8m, 4.9m, 5.0m));

        result.PersistentAbsenceThreeYearAverage.Should().Be(new AttendanceMeasureAverage(16.4m, 15.8m));
        result.PersistentAbsenceYearByYear.School.Should().Be(new AttendanceMeasureSeries(16.0m, 16.4m, 16.8m));
        result.PersistentAbsenceYearByYear.England.Should().Be(new AttendanceMeasureSeries(15.7m, 15.8m, 16.0m));
        result.Years.Should().ContainInOrder("2021 to 2022", "2022 to 2023", "2023 to 2024");
    }

    [Fact]
    public async Task Execute_WhenRepositoryReturnsNull_ReturnsNullMeasures()
    {
        var schoolDetailsServiceMock = new Mock<ISchoolDetailsService>();
        var repositoryMock = new Mock<IAttendanceRepository>();

        schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrnAsync("123456"))
            .ReturnsAsync(CreateSchoolDetails("123456"));

        repositoryMock
            .Setup(x => x.GetByUrnAsync("123456"))
            .ReturnsAsync((AttendanceMeasuresData?)null);

        var sut = new GetAttendanceMeasures(repositoryMock.Object, schoolDetailsServiceMock.Object);

        var result = await sut.Execute(new GetAttendanceMeasuresRequest("123456"));

        result.OverallAbsenceThreeYearAverage.Should().Be(new AttendanceMeasureAverage(null, null));
        result.OverallAbsenceYearByYear.School.Should().Be(new AttendanceMeasureSeries(null, null, null));
        result.OverallAbsenceYearByYear.England.Should().Be(new AttendanceMeasureSeries(null, null, null));
        result.PersistentAbsenceThreeYearAverage.Should().Be(new AttendanceMeasureAverage(null, null));
        result.PersistentAbsenceYearByYear.School.Should().Be(new AttendanceMeasureSeries(null, null, null));
        result.PersistentAbsenceYearByYear.England.Should().Be(new AttendanceMeasureSeries(null, null, null));
        result.Years.Should().BeEmpty();
    }

    private static SchoolDetails CreateSchoolDetails(string urn)
    {
        return new SchoolDetails
        {
            Urn = urn,
            Name = "Test School",
            DfENumber = DataWithAvailability.Available("123/4567"),
            Ukprn = DataWithAvailability.Available("10001234"),
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
            AdmissionsPolicy = DataWithAvailability.Available("Comprehensive"),
            ReligiousCharacter = DataWithAvailability.Available("None"),
            GovernanceStructure = DataWithAvailability.Available(GovernanceType.SingleAcademyTrust),
            AcademyTrustName = DataWithAvailability.Available("Test Trust"),
            AcademyTrustId = DataWithAvailability.Available("TR001"),
            HasNurseryProvision = DataWithAvailability.Available(false),
            HasSixthForm = DataWithAvailability.Available(true),
            HasSenUnit = DataWithAvailability.Available(false),
            HasResourcedProvision = DataWithAvailability.Available(false),
            HeadteacherName = DataWithAvailability.Available("Head Teacher"),
            Website = DataWithAvailability.Available("https://example.org"),
            Telephone = DataWithAvailability.Available("01234 567890"),
            Email = DataWithAvailability.Available("test@example.org")
        };
    }
}
