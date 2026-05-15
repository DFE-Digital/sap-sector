using Moq;
using SAPSec.Core.Features.Attendance;
using SAPSec.Core.Features.Attendance.UseCases;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Tests.Features.Attendance.UseCases;

public class GetAttendanceMeasuresTests
{
    [Fact]
    public async Task Execute_WhenSchoolMissing_ThrowsNotFoundException()
    {
        var establishmentRepositoryMock = new Mock<IEstablishmentRepository>();
        var repositoryMock = new Mock<IAbsenceRepository>();
        var similarSchoolsRepositoryMock = new Mock<ISimilarSchoolsSecondaryRepository>();

        establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("999999"))
            .ReturnsAsync((Establishment?)null);
        similarSchoolsRepositoryMock
            .Setup(x => x.GetSimilarSchoolsGroupAsync(It.IsAny<string>()))
            .ReturnsAsync(Array.Empty<SimilarSchoolsSecondaryGroupsEntry>());

        var sut = new GetAttendanceMeasures(repositoryMock.Object, establishmentRepositoryMock.Object, similarSchoolsRepositoryMock.Object);

        var act = async () => await sut.Execute(new GetAttendanceMeasuresRequest("999999"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999999*");
    }

    [Fact]
    public async Task Execute_WhenDataExists_MapsOverallAndPersistentValues()
    {
        var establishmentRepositoryMock = new Mock<IEstablishmentRepository>();
        var repositoryMock = new Mock<IAbsenceRepository>();
        var similarSchoolsRepositoryMock = new Mock<ISimilarSchoolsSecondaryRepository>();

        establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(new Establishment { URN = "123456", LAId = "373" });

        repositoryMock
            .Setup(x => x.GetByUrnAsync("123456"))
            .ReturnsAsync(new AbsenceData(
                "123456",
                new EstablishmentAbsence
                {
                    Abs_Tot_Est_Current_Pct = "5.0",
                    Abs_Tot_Est_Previous_Pct = "5.2",
                    Abs_Tot_Est_Previous2_Pct = "5.4",
                    Abs_Persistent_Est_Current_Pct = "16.0",
                    Abs_Persistent_Est_Previous_Pct = "16.4",
                    Abs_Persistent_Est_Previous2_Pct = "16.8"
                },
                new LAAbsence
                {
                    Abs_Tot_LA_Current_Pct = "4.7",
                    Abs_Tot_LA_Previous_Pct = "4.8",
                    Abs_Tot_LA_Previous2_Pct = "5.0",
                    Abs_Persistent_LA_Current_Pct = "15.1",
                    Abs_Persistent_LA_Previous_Pct = "15.3",
                    Abs_Persistent_LA_Previous2_Pct = "15.5"
                },
                new EnglandAbsence
                {
                    Abs_Tot_Eng_Current_Pct = "4.8",
                    Abs_Tot_Eng_Previous_Pct = "4.9",
                    Abs_Tot_Eng_Previous2_Pct = "5.0",
                    Abs_Persistent_Eng_Current_Pct = "15.7",
                    Abs_Persistent_Eng_Previous_Pct = "15.8",
                    Abs_Persistent_Eng_Previous2_Pct = "16.0"
                }));
        similarSchoolsRepositoryMock
            .Setup(x => x.GetSimilarSchoolsGroupAsync("123456"))
            .ReturnsAsync(
            [
                new SimilarSchoolsSecondaryGroupsEntry { URN = "123456", NeighbourURN = "200001" },
                new SimilarSchoolsSecondaryGroupsEntry { URN = "123456", NeighbourURN = "200002" }
            ]);
        repositoryMock
            .Setup(x => x.GetByUrnsAsync(It.Is<IEnumerable<string>>(urns => urns.SequenceEqual(new[] { "200001", "200002" }))))
            .ReturnsAsync(
            [
                new AbsenceData(
                    "200001",
                    new EstablishmentAbsence
                    {
                        Abs_Tot_Est_Current_Pct = "6.0",
                        Abs_Tot_Est_Previous_Pct = "6.2",
                        Abs_Tot_Est_Previous2_Pct = "6.4",
                        Abs_Persistent_Est_Current_Pct = "18.0",
                        Abs_Persistent_Est_Previous_Pct = "18.4",
                        Abs_Persistent_Est_Previous2_Pct = "18.8"
                    },
                    null,
                    null,
                    null),
                new AbsenceData(
                    "200002",
                    new EstablishmentAbsence
                    {
                        Abs_Tot_Est_Current_Pct = "5.0",
                        Abs_Tot_Est_Previous_Pct = "5.2",
                        Abs_Tot_Est_Previous2_Pct = "5.4",
                        Abs_Persistent_Est_Current_Pct = "17.0",
                        Abs_Persistent_Est_Previous_Pct = "17.4",
                        Abs_Persistent_Est_Previous2_Pct = "17.8"
                    },
                    null,
                    null,
                    null)
            ]);

        var sut = new GetAttendanceMeasures(repositoryMock.Object, establishmentRepositoryMock.Object, similarSchoolsRepositoryMock.Object);

        var result = await sut.Execute(new GetAttendanceMeasuresRequest("123456"));

        result.OverallAbsenceThreeYearAverage.Should().Be(new AttendanceMeasureAverage(5.2m, 5.7m, 4.83m, 4.9m));
        result.OverallAbsenceYearByYear.School.Should().Be(new AttendanceMeasureSeries(5.0m, 5.2m, 5.4m));
        result.OverallAbsenceYearByYear.SimilarSchools.Should().Be(new AttendanceMeasureSeries(5.5m, 5.7m, 5.9m));
        result.OverallAbsenceYearByYear.LocalAuthority.Should().Be(new AttendanceMeasureSeries(4.7m, 4.8m, 5.0m));
        result.OverallAbsenceYearByYear.England.Should().Be(new AttendanceMeasureSeries(4.8m, 4.9m, 5.0m));

        result.PersistentAbsenceThreeYearAverage.Should().Be(new AttendanceMeasureAverage(16.4m, 17.9m, 15.3m, 15.83m));
        result.PersistentAbsenceYearByYear.School.Should().Be(new AttendanceMeasureSeries(16.0m, 16.4m, 16.8m));
        result.PersistentAbsenceYearByYear.SimilarSchools.Should().Be(new AttendanceMeasureSeries(17.5m, 17.9m, 18.3m));
        result.PersistentAbsenceYearByYear.LocalAuthority.Should().Be(new AttendanceMeasureSeries(15.1m, 15.3m, 15.5m));
        result.PersistentAbsenceYearByYear.England.Should().Be(new AttendanceMeasureSeries(15.7m, 15.8m, 16.0m));
    }

    [Fact]
    public async Task Execute_WhenRepositoryReturnsNull_ReturnsNullMeasures()
    {
        var establishmentRepositoryMock = new Mock<IEstablishmentRepository>();
        var repositoryMock = new Mock<IAbsenceRepository>();
        var similarSchoolsRepositoryMock = new Mock<ISimilarSchoolsSecondaryRepository>();

        establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(new Establishment { URN = "123456" });
        similarSchoolsRepositoryMock
            .Setup(x => x.GetSimilarSchoolsGroupAsync(It.IsAny<string>()))
            .ReturnsAsync(Array.Empty<SimilarSchoolsSecondaryGroupsEntry>());

        repositoryMock
            .Setup(x => x.GetByUrnAsync("123456"))
            .ReturnsAsync((AbsenceData?)null);

        var sut = new GetAttendanceMeasures(repositoryMock.Object, establishmentRepositoryMock.Object, similarSchoolsRepositoryMock.Object);

        var result = await sut.Execute(new GetAttendanceMeasuresRequest("123456"));

        result.OverallAbsenceThreeYearAverage.Should().Be(new AttendanceMeasureAverage(null, null, null, null));
        result.OverallAbsenceYearByYear.School.Should().Be(new AttendanceMeasureSeries(null, null, null));
        result.OverallAbsenceYearByYear.SimilarSchools.Should().Be(new AttendanceMeasureSeries(null, null, null));
        result.OverallAbsenceYearByYear.LocalAuthority.Should().Be(new AttendanceMeasureSeries(null, null, null));
        result.OverallAbsenceYearByYear.England.Should().Be(new AttendanceMeasureSeries(null, null, null));
        result.PersistentAbsenceThreeYearAverage.Should().Be(new AttendanceMeasureAverage(null, null, null, null));
        result.PersistentAbsenceYearByYear.School.Should().Be(new AttendanceMeasureSeries(null, null, null));
        result.PersistentAbsenceYearByYear.SimilarSchools.Should().Be(new AttendanceMeasureSeries(null, null, null));
        result.PersistentAbsenceYearByYear.LocalAuthority.Should().Be(new AttendanceMeasureSeries(null, null, null));
        result.PersistentAbsenceYearByYear.England.Should().Be(new AttendanceMeasureSeries(null, null, null));
    }

    [Fact]
    public async Task Execute_WhenAnyYearMissing_ThreeYearAverageIsNull()
    {
        var establishmentRepositoryMock = new Mock<IEstablishmentRepository>();
        var repositoryMock = new Mock<IAbsenceRepository>();
        var similarSchoolsRepositoryMock = new Mock<ISimilarSchoolsSecondaryRepository>();

        establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(new Establishment { URN = "123456", LAId = "373" });
        similarSchoolsRepositoryMock
            .Setup(x => x.GetSimilarSchoolsGroupAsync(It.IsAny<string>()))
            .ReturnsAsync(Array.Empty<SimilarSchoolsSecondaryGroupsEntry>());

        repositoryMock
            .Setup(x => x.GetByUrnAsync("123456"))
            .ReturnsAsync(new AbsenceData(
                "123456",
                new EstablishmentAbsence
                {
                    Abs_Tot_Est_Current_Pct = "5.0",
                    Abs_Tot_Est_Previous_Pct = "5.2",
                    Abs_Tot_Est_Previous2_Pct = null,
                    Abs_Persistent_Est_Current_Pct = "16.0",
                    Abs_Persistent_Est_Previous_Pct = null,
                    Abs_Persistent_Est_Previous2_Pct = "16.8"
                },
                new LAAbsence
                {
                    Abs_Tot_LA_Current_Pct = "4.7",
                    Abs_Tot_LA_Previous_Pct = "4.8",
                    Abs_Tot_LA_Previous2_Pct = null,
                    Abs_Persistent_LA_Current_Pct = "15.1",
                    Abs_Persistent_LA_Previous_Pct = null,
                    Abs_Persistent_LA_Previous2_Pct = "15.5"
                },
                new EnglandAbsence
                {
                    Abs_Tot_Eng_Current_Pct = "4.8",
                    Abs_Tot_Eng_Previous_Pct = "4.9",
                    Abs_Tot_Eng_Previous2_Pct = null,
                    Abs_Persistent_Eng_Current_Pct = "15.7",
                    Abs_Persistent_Eng_Previous_Pct = null,
                    Abs_Persistent_Eng_Previous2_Pct = "16.0"
                }));

        var sut = new GetAttendanceMeasures(repositoryMock.Object, establishmentRepositoryMock.Object, similarSchoolsRepositoryMock.Object);

        var result = await sut.Execute(new GetAttendanceMeasuresRequest("123456"));

        result.OverallAbsenceThreeYearAverage.Should().Be(new AttendanceMeasureAverage(null, null, null, null));
        result.PersistentAbsenceThreeYearAverage.Should().Be(new AttendanceMeasureAverage(null, null, null, null));
    }

}
