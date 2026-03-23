using Moq;
using SAPSec.Core.Features.Attendance;
using SAPSec.Core.Features.Attendance.UseCases;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Tests.Features.Attendance.UseCases;

public class GetAttendanceMeasuresTests
{
    [Fact]
    public async Task Execute_WhenSchoolMissing_ThrowsNotFoundException()
    {
        var establishmentRepositoryMock = new Mock<IEstablishmentRepository>();
        var repositoryMock = new Mock<IAttendanceRepository>();

        establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("999999"))
            .ReturnsAsync((Establishment?)null);

        var sut = new GetAttendanceMeasures(repositoryMock.Object, establishmentRepositoryMock.Object);

        var act = async () => await sut.Execute(new GetAttendanceMeasuresRequest("999999"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999999*");
    }

    [Fact]
    public async Task Execute_WhenDataExists_MapsOverallAndPersistentValues()
    {
        var establishmentRepositoryMock = new Mock<IEstablishmentRepository>();
        var repositoryMock = new Mock<IAttendanceRepository>();

        establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(new Establishment { URN = "123456" });

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
                new LocalAuthorityAttendance
                {
                    Abs_Tot_La_Current_Pct = 4.7m,
                    Abs_Tot_La_Previous_Pct = 4.8m,
                    Abs_Tot_La_Previous2_Pct = 5.0m,
                    Abs_Persistent_La_Current_Pct = 15.1m,
                    Abs_Persistent_La_Previous_Pct = 15.3m,
                    Abs_Persistent_La_Previous2_Pct = 15.5m
                },
                new EnglandAttendance
                {
                    Abs_Tot_Eng_Current_Pct = 4.8m,
                    Abs_Tot_Eng_Previous_Pct = 4.9m,
                    Abs_Tot_Eng_Previous2_Pct = 5.0m,
                    Abs_Persistent_Eng_Current_Pct = 15.7m,
                    Abs_Persistent_Eng_Previous_Pct = 15.8m,
                    Abs_Persistent_Eng_Previous2_Pct = 16.0m
                }));

        var sut = new GetAttendanceMeasures(repositoryMock.Object, establishmentRepositoryMock.Object);

        var result = await sut.Execute(new GetAttendanceMeasuresRequest("123456"));

        result.OverallAbsenceThreeYearAverage.Should().Be(new AttendanceMeasureAverage(5.2m, 4.83m, 4.9m));
        result.OverallAbsenceYearByYear.School.Should().Be(new AttendanceMeasureSeries(5.0m, 5.2m, 5.4m));
        result.OverallAbsenceYearByYear.LocalAuthority.Should().Be(new AttendanceMeasureSeries(4.7m, 4.8m, 5.0m));
        result.OverallAbsenceYearByYear.England.Should().Be(new AttendanceMeasureSeries(4.8m, 4.9m, 5.0m));

        result.PersistentAbsenceThreeYearAverage.Should().Be(new AttendanceMeasureAverage(16.4m, 15.3m, 15.83m));
        result.PersistentAbsenceYearByYear.School.Should().Be(new AttendanceMeasureSeries(16.0m, 16.4m, 16.8m));
        result.PersistentAbsenceYearByYear.LocalAuthority.Should().Be(new AttendanceMeasureSeries(15.1m, 15.3m, 15.5m));
        result.PersistentAbsenceYearByYear.England.Should().Be(new AttendanceMeasureSeries(15.7m, 15.8m, 16.0m));
    }

    [Fact]
    public async Task Execute_WhenRepositoryReturnsNull_ReturnsNullMeasures()
    {
        var establishmentRepositoryMock = new Mock<IEstablishmentRepository>();
        var repositoryMock = new Mock<IAttendanceRepository>();

        establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(new Establishment { URN = "123456" });

        repositoryMock
            .Setup(x => x.GetByUrnAsync("123456"))
            .ReturnsAsync((AttendanceMeasuresData?)null);

        var sut = new GetAttendanceMeasures(repositoryMock.Object, establishmentRepositoryMock.Object);

        var result = await sut.Execute(new GetAttendanceMeasuresRequest("123456"));

        result.OverallAbsenceThreeYearAverage.Should().Be(new AttendanceMeasureAverage(null, null, null));
        result.OverallAbsenceYearByYear.School.Should().Be(new AttendanceMeasureSeries(null, null, null));
        result.OverallAbsenceYearByYear.LocalAuthority.Should().Be(new AttendanceMeasureSeries(null, null, null));
        result.OverallAbsenceYearByYear.England.Should().Be(new AttendanceMeasureSeries(null, null, null));
        result.PersistentAbsenceThreeYearAverage.Should().Be(new AttendanceMeasureAverage(null, null, null));
        result.PersistentAbsenceYearByYear.School.Should().Be(new AttendanceMeasureSeries(null, null, null));
        result.PersistentAbsenceYearByYear.LocalAuthority.Should().Be(new AttendanceMeasureSeries(null, null, null));
        result.PersistentAbsenceYearByYear.England.Should().Be(new AttendanceMeasureSeries(null, null, null));
    }

    [Fact]
    public async Task Execute_WhenAnyYearMissing_ThreeYearAverageIsNull()
    {
        var establishmentRepositoryMock = new Mock<IEstablishmentRepository>();
        var repositoryMock = new Mock<IAttendanceRepository>();

        establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(new Establishment { URN = "123456" });

        repositoryMock
            .Setup(x => x.GetByUrnAsync("123456"))
            .ReturnsAsync(new AttendanceMeasuresData(
                new EstablishmentAttendance
                {
                    Abs_Tot_Est_Current_Pct = 5.0m,
                    Abs_Tot_Est_Previous_Pct = 5.2m,
                    Abs_Tot_Est_Previous2_Pct = null,
                    Abs_Persistent_Est_Current_Pct = 16.0m,
                    Abs_Persistent_Est_Previous_Pct = null,
                    Abs_Persistent_Est_Previous2_Pct = 16.8m
                },
                new LocalAuthorityAttendance
                {
                    Abs_Tot_La_Current_Pct = 4.7m,
                    Abs_Tot_La_Previous_Pct = 4.8m,
                    Abs_Tot_La_Previous2_Pct = null,
                    Abs_Persistent_La_Current_Pct = 15.1m,
                    Abs_Persistent_La_Previous_Pct = null,
                    Abs_Persistent_La_Previous2_Pct = 15.5m
                },
                new EnglandAttendance
                {
                    Abs_Tot_Eng_Current_Pct = 4.8m,
                    Abs_Tot_Eng_Previous_Pct = 4.9m,
                    Abs_Tot_Eng_Previous2_Pct = null,
                    Abs_Persistent_Eng_Current_Pct = 15.7m,
                    Abs_Persistent_Eng_Previous_Pct = null,
                    Abs_Persistent_Eng_Previous2_Pct = 16.0m
                }));

        var sut = new GetAttendanceMeasures(repositoryMock.Object, establishmentRepositoryMock.Object);

        var result = await sut.Execute(new GetAttendanceMeasuresRequest("123456"));

        result.OverallAbsenceThreeYearAverage.Should().Be(new AttendanceMeasureAverage(null, null, null));
        result.PersistentAbsenceThreeYearAverage.Should().Be(new AttendanceMeasureAverage(null, null, null));
    }

}
