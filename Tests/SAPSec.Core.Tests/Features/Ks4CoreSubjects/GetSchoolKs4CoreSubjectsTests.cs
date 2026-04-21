using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Features.Geography;
using SAPSec.Core.Features.Ks4CoreSubjects.UseCases;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model.Generated;
using SAPSec.Core.Services;

namespace SAPSec.Core.Tests.Features.Ks4CoreSubjects;

public class GetSchoolKs4CoreSubjectsTests
{
    [Fact]
    public async Task Execute_EnglishLanguageGrade4_BuildsThreeYearAverages()
    {
        var context = new TestContext();
        context.SetupCurrentSchoolData(
            establishment: data =>
            {
                data.EngLang49_Sum_Est_Current_Pct = "52";
                data.EngLang49_Sum_Est_Previous_Pct = "51";
                data.EngLang49_Sum_Est_Previous2_Pct = "50";
            },
            localAuthority: data =>
            {
                data.EngLang49_Tot_LA_Current_Pct = "61";
                data.EngLang49_Tot_LA_Previous_Pct = "60";
                data.EngLang49_Tot_LA_Previous2_Pct = "59";
            },
            england: data =>
            {
                data.EngLang49_Tot_Eng_Current_Pct = "62";
                data.EngLang49_Tot_Eng_Previous_Pct = "61";
                data.EngLang49_Tot_Eng_Previous2_Pct = "60";
            });

        context.SetupSimilarSchools(
            CreateSimilarSchoolMeasures("200001", "Alpha school", establishment: data =>
            {
                data.EngLang49_Sum_Est_Current_Pct = "62";
                data.EngLang49_Sum_Est_Previous_Pct = "61";
                data.EngLang49_Sum_Est_Previous2_Pct = "60";
            }),
            CreateSimilarSchoolMeasures("200002", "Beta school", establishment: data =>
            {
                data.EngLang49_Sum_Est_Current_Pct = "58";
                data.EngLang49_Sum_Est_Previous_Pct = "57";
                data.EngLang49_Sum_Est_Previous2_Pct = "56";
            }));

        var result = await context.Sut.Execute(new GetSchoolKs4CoreSubjectsRequest("100001"));
        var subject = SchoolKs4CoreSubjectSelection.From(result, SchoolKs4CoreSubject.EnglishLanguage, SchoolKs4CoreSubjectGradeFilter.Grade4);

        result.SimilarSchoolsCount.Should().Be(2);
        subject.ThreeYearAverage.Should().BeEquivalentTo(new SchoolKs4ComparisonAverage(51m, 59m, 60m, 61m));
    }

    [Fact]
    public async Task Execute_EnglishLanguageGrade4_BuildsTopPerformersInRankOrder()
    {
        var context = new TestContext();
        context.SetupCurrentSchoolData(establishment: data =>
        {
            data.EngLang49_Sum_Est_Current_Pct = "52";
            data.EngLang49_Sum_Est_Previous_Pct = "51";
            data.EngLang49_Sum_Est_Previous2_Pct = "50";
        });

        context.SetupSimilarSchools(
            CreateSimilarSchoolMeasures("200001", "Alpha school", establishment: data =>
            {
                data.EngLang49_Sum_Est_Current_Pct = "62";
                data.EngLang49_Sum_Est_Previous_Pct = "61";
                data.EngLang49_Sum_Est_Previous2_Pct = "60";
            }),
            CreateSimilarSchoolMeasures("200002", "Beta school", establishment: data =>
            {
                data.EngLang49_Sum_Est_Current_Pct = "58";
                data.EngLang49_Sum_Est_Previous_Pct = "57";
                data.EngLang49_Sum_Est_Previous2_Pct = "56";
            }),
            CreateSimilarSchoolMeasures("200003", "Gamma school", establishment: data =>
            {
                data.EngLang49_Sum_Est_Current_Pct = "48";
                data.EngLang49_Sum_Est_Previous_Pct = "47";
                data.EngLang49_Sum_Est_Previous2_Pct = "46";
            }));

        var result = await context.Sut.Execute(new GetSchoolKs4CoreSubjectsRequest("100001"));
        var subject = SchoolKs4CoreSubjectSelection.From(result, SchoolKs4CoreSubject.EnglishLanguage, SchoolKs4CoreSubjectGradeFilter.Grade4);

        subject.TopPerformers.Select(x => (x.Rank, x.Name, x.Value)).Should().ContainInOrder(
            (1, "Alpha school", 61m as decimal?),
            (2, "Beta school", 57m as decimal?),
            (3, "Current school", 51m as decimal?));
        subject.TopPerformers[2].IsCurrentSchool.Should().BeTrue();
    }

    [Fact]
    public async Task Execute_WhenSimilarSchoolUrnsContainDuplicates_BuildsUniqueTopPerformers()
    {
        var context = new TestContext();
        context.SetupCurrentSchoolData(establishment: data =>
        {
            data.EngLang49_Sum_Est_Current_Pct = "52";
            data.EngLang49_Sum_Est_Previous_Pct = "51";
            data.EngLang49_Sum_Est_Previous2_Pct = "50";
        });

        context.SetupSimilarSchools(
            [
                CreateSimilarSchoolMeasures("200001", "Alpha school", establishment: data =>
                {
                    data.EngLang49_Sum_Est_Current_Pct = "62";
                    data.EngLang49_Sum_Est_Previous_Pct = "61";
                    data.EngLang49_Sum_Est_Previous2_Pct = "60";
                }),
                CreateSimilarSchoolMeasures("200001", "Alpha school", establishment: data =>
                {
                    data.EngLang49_Sum_Est_Current_Pct = "62";
                    data.EngLang49_Sum_Est_Previous_Pct = "61";
                    data.EngLang49_Sum_Est_Previous2_Pct = "60";
                }),
                CreateSimilarSchoolMeasures("200002", "Beta school", establishment: data =>
                {
                    data.EngLang49_Sum_Est_Current_Pct = "58";
                    data.EngLang49_Sum_Est_Previous_Pct = "57";
                    data.EngLang49_Sum_Est_Previous2_Pct = "56";
                }),
                CreateSimilarSchoolMeasures("200003", "Gamma school", establishment: data =>
                {
                    data.EngLang49_Sum_Est_Current_Pct = "48";
                    data.EngLang49_Sum_Est_Previous_Pct = "47";
                    data.EngLang49_Sum_Est_Previous2_Pct = "46";
                })
            ]);

        var result = await context.Sut.Execute(new GetSchoolKs4CoreSubjectsRequest("100001"));
        var subject = SchoolKs4CoreSubjectSelection.From(result, SchoolKs4CoreSubject.EnglishLanguage, SchoolKs4CoreSubjectGradeFilter.Grade4);

        result.SimilarSchoolsCount.Should().Be(3);
        subject.TopPerformers.Select(x => x.Urn).Should().Equal("200001", "200002", "100001");
    }

    [Fact]
    public async Task Execute_EnglishLiteratureGrade4_BuildsYearByYearSeries()
    {
        var context = new TestContext();
        context.SetupCurrentSchoolData(
            establishment: data =>
            {
                data.EngLit49_Sum_Est_Current_Pct = "54";
                data.EngLit49_Sum_Est_Previous_Pct = "53";
                data.EngLit49_Sum_Est_Previous2_Pct = "52";
            },
            localAuthority: data =>
            {
                data.EngLit49_Tot_LA_Current_Pct = "63";
                data.EngLit49_Tot_LA_Previous_Pct = "62";
                data.EngLit49_Tot_LA_Previous2_Pct = "61";
            },
            england: data =>
            {
                data.EngLit49_Tot_Eng_Current_Pct = "64";
                data.EngLit49_Tot_Eng_Previous_Pct = "63";
                data.EngLit49_Tot_Eng_Previous2_Pct = "62";
            });

        context.SetupSimilarSchools(
            CreateSimilarSchoolMeasures("200001", "Alpha school", establishment: data =>
            {
                data.EngLit49_Sum_Est_Current_Pct = "64";
                data.EngLit49_Sum_Est_Previous_Pct = "63";
                data.EngLit49_Sum_Est_Previous2_Pct = "62";
            }),
            CreateSimilarSchoolMeasures("200002", "Beta school", establishment: data =>
            {
                data.EngLit49_Sum_Est_Current_Pct = "60";
                data.EngLit49_Sum_Est_Previous_Pct = "59";
                data.EngLit49_Sum_Est_Previous2_Pct = "58";
            }));

        var result = await context.Sut.Execute(new GetSchoolKs4CoreSubjectsRequest("100001"));
        var subject = SchoolKs4CoreSubjectSelection.From(result, SchoolKs4CoreSubject.EnglishLiterature, SchoolKs4CoreSubjectGradeFilter.Grade4);

        subject.YearByYear.Should().BeEquivalentTo(new SchoolKs4ComparisonYearByYear(
            new Ks4HeadlineMeasureSeries(54m, 53m, 52m),
            new Ks4HeadlineMeasureSeries(62m, 61m, 60m),
            new Ks4HeadlineMeasureSeries(63m, 62m, 61m),
            new Ks4HeadlineMeasureSeries(64m, 63m, 62m)));
    }

    [Fact]
    public async Task Execute_CombinedScienceGrade4_BuildsThreeYearAverages()
    {
        var context = new TestContext();
        context.SetupCurrentSchoolData(
            establishment: data =>
            {
                data.CombSci49_Sum_Est_Current_Pct = "48";
                data.CombSci49_Sum_Est_Previous_Pct = "47";
                data.CombSci49_Sum_Est_Previous2_Pct = "46";
            },
            localAuthority: data =>
            {
                data.CombSci49_Tot_LA_Current_Pct = "57";
                data.CombSci49_Tot_LA_Previous_Pct = "56";
                data.CombSci49_Tot_LA_Previous2_Pct = "55";
            },
            england: data =>
            {
                data.CombSci49_Tot_Eng_Current_Pct = "58";
                data.CombSci49_Tot_Eng_Previous_Pct = "57";
                data.CombSci49_Tot_Eng_Previous2_Pct = "56";
            });

        context.SetupSimilarSchools(
            CreateSimilarSchoolMeasures("200001", "Alpha school", establishment: data =>
            {
                data.CombSci49_Sum_Est_Current_Pct = "66";
                data.CombSci49_Sum_Est_Previous_Pct = "65";
                data.CombSci49_Sum_Est_Previous2_Pct = "64";
            }),
            CreateSimilarSchoolMeasures("200002", "Beta school", establishment: data =>
            {
                data.CombSci49_Sum_Est_Current_Pct = "62";
                data.CombSci49_Sum_Est_Previous_Pct = "61";
                data.CombSci49_Sum_Est_Previous2_Pct = "60";
            }));

        var result = await context.Sut.Execute(new GetSchoolKs4CoreSubjectsRequest("100001"));
        var subject = SchoolKs4CoreSubjectSelection.From(result, SchoolKs4CoreSubject.CombinedScienceDoubleAward, SchoolKs4CoreSubjectGradeFilter.Grade4);

        subject.ThreeYearAverage.Should().BeEquivalentTo(new SchoolKs4ComparisonAverage(47m, 63m, 56m, 57m));
    }

    [Fact]
    public async Task Execute_BiologyGrade5_BuildsThreeYearAverages()
    {
        var context = new TestContext();
        context.SetupCurrentSchoolData(
            establishment: data =>
            {
                data.Bio59_Sum_Est_Current_Pct = "45";
                data.Bio59_Sum_Est_Previous_Pct = "44";
                data.Bio59_Sum_Est_Previous2_Pct = "43";
            },
            localAuthority: data =>
            {
                data.Bio59_Tot_LA_Current_Pct = "55";
                data.Bio59_Tot_LA_Previous_Pct = "54";
                data.Bio59_Tot_LA_Previous2_Pct = "53";
            },
            england: data =>
            {
                data.Bio59_Tot_Eng_Current_Pct = "65";
                data.Bio59_Tot_Eng_Previous_Pct = "64";
                data.Bio59_Tot_Eng_Previous2_Pct = "63";
            });

        context.SetupSimilarSchools(
            CreateSimilarSchoolMeasures("200001", "Alpha school", establishment: data =>
            {
                data.Bio59_Sum_Est_Current_Pct = "75";
                data.Bio59_Sum_Est_Previous_Pct = "74";
                data.Bio59_Sum_Est_Previous2_Pct = "73";
            }),
            CreateSimilarSchoolMeasures("200002", "Beta school", establishment: data =>
            {
                data.Bio59_Sum_Est_Current_Pct = "69";
                data.Bio59_Sum_Est_Previous_Pct = "68";
                data.Bio59_Sum_Est_Previous2_Pct = "67";
            }));

        var result = await context.Sut.Execute(new GetSchoolKs4CoreSubjectsRequest("100001"));
        var subject = SchoolKs4CoreSubjectSelection.From(result, SchoolKs4CoreSubject.Biology, SchoolKs4CoreSubjectGradeFilter.Grade5);

        subject.ThreeYearAverage.Should().BeEquivalentTo(new SchoolKs4ComparisonAverage(44m, 71m, 54m, 64m));
    }

    [Fact]
    public async Task Execute_ChemistryGrade5_BuildsYearByYearSeries()
    {
        var context = new TestContext();
        context.SetupCurrentSchoolData(
            establishment: data =>
            {
                data.Chem59_Sum_Est_Current_Pct = "46";
                data.Chem59_Sum_Est_Previous_Pct = "45";
                data.Chem59_Sum_Est_Previous2_Pct = "44";
            },
            localAuthority: data =>
            {
                data.Chem59_Tot_LA_Current_Pct = "56";
                data.Chem59_Tot_LA_Previous_Pct = "55";
                data.Chem59_Tot_LA_Previous2_Pct = "54";
            },
            england: data =>
            {
                data.Chem59_Tot_Eng_Current_Pct = "66";
                data.Chem59_Tot_Eng_Previous_Pct = "65";
                data.Chem59_Tot_Eng_Previous2_Pct = "64";
            });

        context.SetupSimilarSchools(
            CreateSimilarSchoolMeasures("200001", "Alpha school", establishment: data =>
            {
                data.Chem59_Sum_Est_Current_Pct = "66";
                data.Chem59_Sum_Est_Previous_Pct = "65";
                data.Chem59_Sum_Est_Previous2_Pct = "64";
            }),
            CreateSimilarSchoolMeasures("200002", "Beta school", establishment: data =>
            {
                data.Chem59_Sum_Est_Current_Pct = "62";
                data.Chem59_Sum_Est_Previous_Pct = "61";
                data.Chem59_Sum_Est_Previous2_Pct = "60";
            }));

        var result = await context.Sut.Execute(new GetSchoolKs4CoreSubjectsRequest("100001"));
        var subject = SchoolKs4CoreSubjectSelection.From(result, SchoolKs4CoreSubject.Chemistry, SchoolKs4CoreSubjectGradeFilter.Grade5);

        subject.YearByYear.Should().BeEquivalentTo(new SchoolKs4ComparisonYearByYear(
            new Ks4HeadlineMeasureSeries(46m, 45m, 44m),
            new Ks4HeadlineMeasureSeries(64m, 63m, 62m),
            new Ks4HeadlineMeasureSeries(56m, 55m, 54m),
            new Ks4HeadlineMeasureSeries(66m, 65m, 64m)));
    }

    [Fact]
    public async Task Execute_PhysicsGrade7_BuildsTopPerformersInRankOrder()
    {
        var context = new TestContext();
        context.SetupCurrentSchoolData(establishment: data =>
        {
            data.Physics79_Sum_Est_Current_Pct = "72";
            data.Physics79_Sum_Est_Previous_Pct = "71";
            data.Physics79_Sum_Est_Previous2_Pct = "70";
        });

        context.SetupSimilarSchools(
            CreateSimilarSchoolMeasures("200001", "Alpha school", establishment: data =>
            {
                data.Physics79_Sum_Est_Current_Pct = "86";
                data.Physics79_Sum_Est_Previous_Pct = "85";
                data.Physics79_Sum_Est_Previous2_Pct = "84";
            }),
            CreateSimilarSchoolMeasures("200002", "Beta school", establishment: data =>
            {
                data.Physics79_Sum_Est_Current_Pct = "80";
                data.Physics79_Sum_Est_Previous_Pct = "79";
                data.Physics79_Sum_Est_Previous2_Pct = "78";
            }),
            CreateSimilarSchoolMeasures("200003", "Gamma school", establishment: data =>
            {
                data.Physics79_Sum_Est_Current_Pct = "74";
                data.Physics79_Sum_Est_Previous_Pct = "73";
                data.Physics79_Sum_Est_Previous2_Pct = "72";
            }));

        var result = await context.Sut.Execute(new GetSchoolKs4CoreSubjectsRequest("100001"));
        var subject = SchoolKs4CoreSubjectSelection.From(result, SchoolKs4CoreSubject.Physics, SchoolKs4CoreSubjectGradeFilter.Grade7);

        subject.TopPerformers.Select(x => (x.Rank, x.Name, x.Value)).Should().ContainInOrder(
            (1, "Alpha school", 85m as decimal?),
            (2, "Beta school", 79m as decimal?),
            (3, "Gamma school", 73m as decimal?));
    }

    [Fact]
    public async Task Execute_MathsGrade7_BuildsThreeYearAverages()
    {
        var context = new TestContext();
        context.SetupCurrentSchoolData(
            establishment: data =>
            {
                data.Maths79_Sum_Est_Current_Pct = "77";
                data.Maths79_Sum_Est_Previous_Pct = "76";
                data.Maths79_Sum_Est_Previous2_Pct = "75";
            },
            localAuthority: data =>
            {
                data.Maths79_Tot_LA_Current_Pct = "87";
                data.Maths79_Tot_LA_Previous_Pct = "86";
                data.Maths79_Tot_LA_Previous2_Pct = "85";
            },
            england: data =>
            {
                data.Maths79_Tot_Eng_Current_Pct = "97";
                data.Maths79_Tot_Eng_Previous_Pct = "96";
                data.Maths79_Tot_Eng_Previous2_Pct = "95";
            });

        context.SetupSimilarSchools(
            CreateSimilarSchoolMeasures("200001", "Alpha school", establishment: data =>
            {
                data.Maths79_Sum_Est_Current_Pct = "89";
                data.Maths79_Sum_Est_Previous_Pct = "88";
                data.Maths79_Sum_Est_Previous2_Pct = "87";
            }),
            CreateSimilarSchoolMeasures("200002", "Beta school", establishment: data =>
            {
                data.Maths79_Sum_Est_Current_Pct = "83";
                data.Maths79_Sum_Est_Previous_Pct = "82";
                data.Maths79_Sum_Est_Previous2_Pct = "81";
            }));

        var result = await context.Sut.Execute(new GetSchoolKs4CoreSubjectsRequest("100001"));
        var subject = SchoolKs4CoreSubjectSelection.From(result, SchoolKs4CoreSubject.Maths, SchoolKs4CoreSubjectGradeFilter.Grade7);

        subject.ThreeYearAverage.Should().BeEquivalentTo(new SchoolKs4ComparisonAverage(76m, 85m, 86m, 96m));
    }

    [Fact]
    public async Task Execute_CombinedScienceGrade7_BuildsYearByYearSeries()
    {
        var context = new TestContext();
        context.SetupCurrentSchoolData(
            establishment: data =>
            {
                data.CombSci79_Sum_Est_Current_Pct = "78";
                data.CombSci79_Sum_Est_Previous_Pct = "77";
                data.CombSci79_Sum_Est_Previous2_Pct = "76";
            },
            localAuthority: data =>
            {
                data.CombSci79_Tot_LA_Current_Pct = "90";
                data.CombSci79_Tot_LA_Previous_Pct = "89";
                data.CombSci79_Tot_LA_Previous2_Pct = "88";
            },
            england: data =>
            {
                data.CombSci79_Tot_Eng_Current_Pct = "91";
                data.CombSci79_Tot_Eng_Previous_Pct = "90";
                data.CombSci79_Tot_Eng_Previous2_Pct = "89";
            });

        context.SetupSimilarSchools(
            CreateSimilarSchoolMeasures("200001", "Alpha school", establishment: data =>
            {
                data.CombSci79_Sum_Est_Current_Pct = "88";
                data.CombSci79_Sum_Est_Previous_Pct = "87";
                data.CombSci79_Sum_Est_Previous2_Pct = "86";
            }),
            CreateSimilarSchoolMeasures("200002", "Beta school", establishment: data =>
            {
                data.CombSci79_Sum_Est_Current_Pct = "84";
                data.CombSci79_Sum_Est_Previous_Pct = "83";
                data.CombSci79_Sum_Est_Previous2_Pct = "82";
            }));

        var result = await context.Sut.Execute(new GetSchoolKs4CoreSubjectsRequest("100001"));
        var subject = SchoolKs4CoreSubjectSelection.From(result, SchoolKs4CoreSubject.CombinedScienceDoubleAward, SchoolKs4CoreSubjectGradeFilter.Grade7);

        subject.YearByYear.Should().BeEquivalentTo(new SchoolKs4ComparisonYearByYear(
            new Ks4HeadlineMeasureSeries(78m, 77m, 76m),
            new Ks4HeadlineMeasureSeries(86m, 85m, 84m),
            new Ks4HeadlineMeasureSeries(90m, 89m, 88m),
            new Ks4HeadlineMeasureSeries(91m, 90m, 89m)));
    }

    private static SimilarSchoolMeasuresInput CreateSimilarSchoolMeasures(
        string urn,
        string name,
        Action<EstablishmentPerformance>? establishment = null,
        Action<LAPerformance>? localAuthority = null,
        Action<EnglandPerformance>? england = null) =>
        new(
            urn,
            name,
            CreateMeasures(urn, establishment, localAuthority, england));

    private static Ks4PerformanceData CreateMeasures(
        string urn,
        Action<EstablishmentPerformance>? establishment = null,
        Action<LAPerformance>? localAuthority = null,
        Action<EnglandPerformance>? england = null)
    {
        var establishmentPerformance = new EstablishmentPerformance();
        var localAuthorityPerformance = new LAPerformance();
        var englandPerformance = new EnglandPerformance();

        establishment?.Invoke(establishmentPerformance);
        localAuthority?.Invoke(localAuthorityPerformance);
        england?.Invoke(englandPerformance);

        return new(urn, establishmentPerformance, localAuthorityPerformance, englandPerformance);
    }

    private sealed record SimilarSchoolMeasuresInput(string Urn, string Name, Ks4PerformanceData Data);

    private sealed class TestContext
    {
        private readonly Mock<IKs4PerformanceRepository> _repositoryMock = new();
        private readonly Mock<IEstablishmentRepository> _establishmentRepositoryMock = new();
        private readonly Mock<ISimilarSchoolsSecondaryRepository> _similarSchoolsRepositoryMock = new();
        private Ks4PerformanceData _currentSchoolData = CreateMeasures("100001");

        public TestContext()
        {
            _establishmentRepositoryMock
                .Setup(x => x.GetEstablishmentAsync("100001"))
                .ReturnsAsync(CreateSchool("100001", "Current school"));
        }

        public GetSchoolKs4CoreSubjects Sut => new(
            _repositoryMock.Object,
            new SchoolDetailsService(_establishmentRepositoryMock.Object, new Mock<ILogger<SchoolDetailsService>>().Object),
            _establishmentRepositoryMock.Object,
            _similarSchoolsRepositoryMock.Object);

        public void SetupCurrentSchoolData(
            Action<EstablishmentPerformance>? establishment = null,
            Action<LAPerformance>? localAuthority = null,
            Action<EnglandPerformance>? england = null)
        {
            _currentSchoolData = CreateMeasures("100001", establishment, localAuthority, england);

            _repositoryMock
                .Setup(x => x.GetByUrnAsync("100001"))
                .ReturnsAsync(_currentSchoolData);
        }

        public void SetupSimilarSchools(params SimilarSchoolMeasuresInput[] similarSchools) =>
            SetupSimilarSchools((IEnumerable<SimilarSchoolMeasuresInput>)similarSchools);

        public void SetupSimilarSchools(IEnumerable<SimilarSchoolMeasuresInput> similarSchools)
        {
            var similarSchoolsArray = similarSchools.ToArray();

            _similarSchoolsRepositoryMock
                .Setup(x => x.GetSimilarSchoolsGroupAsync("100001"))
                .ReturnsAsync(similarSchoolsArray.Select(x => new SimilarSchoolsSecondaryGroupsEntry { URN = "100001", NeighbourURN = x.Urn }).ToArray());

            _repositoryMock
                .Setup(x => x.GetByUrnsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(similarSchoolsArray
                    .GroupBy(x => x.Urn, StringComparer.Ordinal)
                    .Select(x => x.First().Data)
                    .ToArray());

            _establishmentRepositoryMock
                .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(similarSchoolsArray
                    .GroupBy(x => x.Urn, StringComparer.Ordinal)
                    .Select(x => x.First())
                    .Select(x => new Establishment { URN = x.Urn, EstablishmentName = x.Name })
                    .ToArray());
        }
    }

    private static Establishment CreateSchool(string urn, string name, BNGCoordinates? coordinates = null)
    {
        return new Establishment
        {
            URN = urn,
            EstablishmentName = name,
            Street = "123 Test Street",
            Town = "Sheffield",
            Postcode = "S1 1AA",
            Locality = "",
            Address3 = "",
            TotalCapacity = 1200,
            TotalPupils = 1000,
            NurseryProvisionName = "No",
            LAId = "373",
            LAName = "Sheffield",
            RegionId = "R",
            RegionName = "Yorkshire and the Humber",
            UrbanRuralId = "A1",
            UrbanRuralName = "Urban",
            PhaseOfEducationId = "P",
            PhaseOfEducationName = "Secondary",
            OfficialSixthFormId = "1",
            OfficialSixthFormName = "Has sixth form",
            AdmissionsPolicyId = "1",
            AdmissionsPolicyName = "Comprehensive",
            GenderId = "M",
            GenderName = "Mixed",
            ResourcedProvisionId = "0",
            ResourcedProvisionName = "No",
            TypeOfEstablishmentId = "27",
            TypeOfEstablishmentName = "Academy",
            EstablishmentTypeGroupId = "10",
            EstablishmentTypeGroupName = "Academies",
            TrustSchoolFlagId = "0",
            TrustSchoolFlagName = "No",
            Easting = coordinates?.Easting,
            Northing = coordinates?.Northing,
        };
    }
}
