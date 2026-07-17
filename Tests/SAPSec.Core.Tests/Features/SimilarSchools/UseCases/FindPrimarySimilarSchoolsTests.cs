using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Data.Dto;
using SAPSec.Data.Dto.Absence;
using SAPSec.Data.Dto.SimilarSchools.Primary;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Core.Tests.Features.SimilarSchools.UseCases;

public class FindPrimarySimilarSchoolsTests
{
    private readonly InMemorySimilarSchoolsPrimaryRepository _similarSchoolsRepo = new();
    private readonly InMemoryEstablishmentRepository _establishmentRepo = new();
    private readonly InMemoryAbsenceRepository _absenceRepo = new();
    private readonly InMemoryKs2PerformanceRepository _performanceRepo;
    private readonly FindPrimarySimilarSchools _sut;

    public FindPrimarySimilarSchoolsTests()
    {
        _performanceRepo = new InMemoryKs2PerformanceRepository(_establishmentRepo);
        _sut = new FindPrimarySimilarSchools(_establishmentRepo, _similarSchoolsRepo, _absenceRepo, _performanceRepo);
    }

    [Fact]
    public async Task WhenCurrentSchoolUrnDoesNotExist_ThrowsNotFoundException()
    {
        var act = async () => await _sut.Execute(new("999999"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999999*");
    }

    [Fact]
    public async Task WhenCurrentSchoolValuesDoNotExist_ThrowsNotFoundException()
    {
        _establishmentRepo.SetupEstablishments(new Establishment { URN = "100001", EstablishmentName = "Current School" });

        var act = async () => await _sut.Execute(new("100001"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*100001*");
    }

    [Fact]
    public async Task ReturnsCurrentSchoolAndSimilarSchoolsWithCharacteristics()
    {
        _establishmentRepo.SetupEstablishments(
            new Establishment { URN = "100001", EstablishmentName = "Current School", LAName = "LA One" },
            new Establishment { URN = "100002", EstablishmentName = "Similar School 1", LAName = "LA Two" },
            new Establishment { URN = "100003", EstablishmentName = "Similar School 2", LAName = "LA Three" });

        _similarSchoolsRepo.SetupGroups(
            new SimilarSchoolsPrimaryGroupsEntry { URN = "100001", NeighbourURN = "100002", Dist = "0.1", Rank = "1" },
            new SimilarSchoolsPrimaryGroupsEntry { URN = "100001", NeighbourURN = "100003", Dist = "0.2", Rank = "2" });

        _similarSchoolsRepo.SetupValues(
            new SimilarSchoolsPrimaryValuesEntry
            {
                URN = "100001",
                PPPerc = "20.2",
                Polar4QuintilePupils = "2",
                PStability = "95",
                PercentSchSupport = "10",
                PercentEAL = "5",
                IdaciPupils = "0.123",
                PercentageStatementOrEhp = "1.5",
                NumberOfPupils = "210",
                ReadMatAverage = "102.4",
                Ks1PriorRwmAverage = "11.4"
            },
            new SimilarSchoolsPrimaryValuesEntry
            {
                URN = "100002",
                PPPerc = "25.4",
                Polar4QuintilePupils = "3",
                PStability = "94",
                PercentSchSupport = "12",
                PercentEAL = "6",
                IdaciPupils = "0.223",
                PercentageStatementOrEhp = "2.5",
                NumberOfPupils = "220",
                ReadMatAverage = "101.4",
                Ks1PriorRwmAverage = "10.4"
            },
            new SimilarSchoolsPrimaryValuesEntry
            {
                URN = "100003",
                PPPerc = "30.5",
                Polar4QuintilePupils = "4",
                PStability = "93",
                PercentSchSupport = "14",
                PercentEAL = "7",
                IdaciPupils = "0.323",
                PercentageStatementOrEhp = "3.5",
                NumberOfPupils = "230",
                ReadMatAverage = "100.4",
                Ks1PriorRwmAverage = "9.4"
            });

        var response = await _sut.Execute(new("100001"));

        response.CurrentSchool.Should().BeEquivalentTo(new
        {
            Urn = "100001",
            Name = "Current School",
            LocalAuthorityName = "LA One"
        });
        response.CurrentSchool.Characteristics.ReadMatAverage.Should().Be(102.4m);
        response.CurrentSchool.Characteristics.Ks1PriorRwmAverage.Should().Be(11.4m);

        response.SimilarSchoolsPage.Should().HaveCount(2);
        response.SimilarSchoolsPage.Should().SatisfyRespectively(
            first =>
            {
                first.Urn.Should().Be("100002");
                first.Name.Should().Be("Similar School 1");
                first.LocalAuthorityName.Should().Be("LA Two");
                first.Rank.Should().Be("1");
                first.Distance.Should().Be("0.1");
                first.Characteristics.PupilCount.Should().Be(220m);
            },
            second =>
            {
                second.Urn.Should().Be("100003");
                second.Name.Should().Be("Similar School 2");
                second.LocalAuthorityName.Should().Be("LA Three");
                second.Rank.Should().Be("2");
                second.Distance.Should().Be("0.2");
                second.Characteristics.PupilPremiumEligibilityPercentage.Should().Be(30.5m);
            });
        response.FilterOptions.Should().NotBeEmpty();
        response.SortOptions.Should().SatisfyRespectively(
            o => o.Should().BeEquivalentTo(new { Key = "RwmExpected", Selected = true }),
            o => o.Should().BeEquivalentTo(new { Key = "GpsExpected", Selected = false }));
        response.ValidationErrors.Should().BeEmpty();
    }

    [Fact]
    public async Task ReturnsDynamicFilterOptionsAndAppliesFilters()
    {
        _establishmentRepo.SetupEstablishments(
            new Establishment
            {
                URN = "100001", EstablishmentName = "Current School", LAName = "LA One",
                Easting = 100000, Northing = 100000, RegionId = "R1", RegionName = "North East",
                UrbanRuralId = "U1", UrbanRuralName = "Urban", TypeOfEstablishmentId = "34", TypeOfEstablishmentName = "Academy converter",
                PhaseOfEducationId = "P", PhaseOfEducationName = "Primary", OfficialSixthFormId = "0", OfficialSixthFormName = "Does not have sixth form",
                AdmissionsPolicyId = "1", AdmissionsPolicyName = "Non-selective", GenderId = "3", GenderName = "Mixed",
                ResourcedProvisionId = "1", ResourcedProvisionName = "Not applicable", NurseryProvisionName = "No", TotalCapacity = 300, TotalPupils = 210
            },
            new Establishment
            {
                URN = "100002", EstablishmentName = "Similar School 1", LAName = "LA Two",
                Easting = 108046, Northing = 100000, RegionId = "R1", RegionName = "North East",
                UrbanRuralId = "U1", UrbanRuralName = "Urban", TypeOfEstablishmentId = "34", TypeOfEstablishmentName = "Academy converter",
                PhaseOfEducationId = "P", PhaseOfEducationName = "Primary", OfficialSixthFormId = "0", OfficialSixthFormName = "Does not have sixth form",
                AdmissionsPolicyId = "1", AdmissionsPolicyName = "Non-selective", GenderId = "3", GenderName = "Mixed",
                ResourcedProvisionId = "4", ResourcedProvisionName = "Resourced provision", NurseryProvisionName = "Yes", TotalCapacity = 400, TotalPupils = 220
            },
            new Establishment
            {
                URN = "100003", EstablishmentName = "Similar School 2", LAName = "LA Three",
                Easting = 180467, Northing = 100000, RegionId = "R2", RegionName = "South East",
                UrbanRuralId = "R1", UrbanRuralName = "Rural", TypeOfEstablishmentId = "28", TypeOfEstablishmentName = "Community school",
                PhaseOfEducationId = "P", PhaseOfEducationName = "Primary", OfficialSixthFormId = "0", OfficialSixthFormName = "Does not have sixth form",
                AdmissionsPolicyId = "1", AdmissionsPolicyName = "Non-selective", GenderId = "2", GenderName = "Girls",
                ResourcedProvisionId = "8", ResourcedProvisionName = "SEN unit", NurseryProvisionName = "No", TotalCapacity = 500, TotalPupils = 230
            });

        _similarSchoolsRepo.SetupGroups(
            new SimilarSchoolsPrimaryGroupsEntry { URN = "100001", NeighbourURN = "100002", Dist = "0.1", Rank = "1" },
            new SimilarSchoolsPrimaryGroupsEntry { URN = "100001", NeighbourURN = "100003", Dist = "0.2", Rank = "2" });

        _similarSchoolsRepo.SetupValues(
            new SimilarSchoolsPrimaryValuesEntry { URN = "100001", PPPerc = "20.2", Polar4QuintilePupils = "2", PStability = "95", PercentSchSupport = "10", PercentEAL = "5", IdaciPupils = "0.123", PercentageStatementOrEhp = "1.5", NumberOfPupils = "210", ReadMatAverage = "102.4", Ks1PriorRwmAverage = "11.4" },
            new SimilarSchoolsPrimaryValuesEntry { URN = "100002", PPPerc = "25.4", Polar4QuintilePupils = "3", PStability = "94", PercentSchSupport = "12", PercentEAL = "6", IdaciPupils = "0.223", PercentageStatementOrEhp = "2.5", NumberOfPupils = "220", ReadMatAverage = "101.4", Ks1PriorRwmAverage = "10.4" },
            new SimilarSchoolsPrimaryValuesEntry { URN = "100003", PPPerc = "30.5", Polar4QuintilePupils = "4", PStability = "93", PercentSchSupport = "14", PercentEAL = "7", IdaciPupils = "0.323", PercentageStatementOrEhp = "3.5", NumberOfPupils = "230", ReadMatAverage = "100.4", Ks1PriorRwmAverage = "9.4" });

        _absenceRepo.SetupEstablishmentAbsence(
            new EstablishmentAbsence { Id = "100001", Abs_Tot_Est_Current_Pct = "4.5", Abs_Persistent_Est_Current_Pct = "12.0" },
            new EstablishmentAbsence { Id = "100002", Abs_Tot_Est_Current_Pct = "5.1", Abs_Persistent_Est_Current_Pct = "13.0" },
            new EstablishmentAbsence { Id = "100003", Abs_Tot_Est_Current_Pct = "6.1", Abs_Persistent_Est_Current_Pct = "14.0" });

        var response = await _sut.Execute(new("100001", new Dictionary<string, IEnumerable<string>>
        {
            ["ur"] = ["U1"]
        }));

        response.SimilarSchoolsPage.Select(x => x.Urn).Should().Equal("100002");
        response.FilterOptions.Should().Contain(x => x.Key == "ur");
        response.FilterOptions.Should().Contain(x => x.Key == "reg");
        response.FilterOptions.Should().Contain(x => x.Key == "oar");
    }

    [Fact]
    public async Task SortsByGpsExpectedWhenSelected()
    {
        _establishmentRepo.SetupEstablishments(
            new Establishment { URN = "100001", EstablishmentName = "Current School", LAName = "LA One", RegionId = "R1", RegionName = "North East", UrbanRuralId = "U1", UrbanRuralName = "Urban", TypeOfEstablishmentId = "34", TypeOfEstablishmentName = "Academy converter", PhaseOfEducationId = "P", PhaseOfEducationName = "Primary", OfficialSixthFormId = "0", OfficialSixthFormName = "Does not have sixth form", AdmissionsPolicyId = "1", AdmissionsPolicyName = "Non-selective", GenderId = "3", GenderName = "Mixed", ResourcedProvisionId = "1", ResourcedProvisionName = "Not applicable", NurseryProvisionName = "No", TotalCapacity = 300, TotalPupils = 210 },
            new Establishment { URN = "100002", EstablishmentName = "Alpha School", LAName = "LA Two", RegionId = "R1", RegionName = "North East", UrbanRuralId = "U1", UrbanRuralName = "Urban", TypeOfEstablishmentId = "34", TypeOfEstablishmentName = "Academy converter", PhaseOfEducationId = "P", PhaseOfEducationName = "Primary", OfficialSixthFormId = "0", OfficialSixthFormName = "Does not have sixth form", AdmissionsPolicyId = "1", AdmissionsPolicyName = "Non-selective", GenderId = "3", GenderName = "Mixed", ResourcedProvisionId = "1", ResourcedProvisionName = "Not applicable", NurseryProvisionName = "No", TotalCapacity = 300, TotalPupils = 220 },
            new Establishment { URN = "100003", EstablishmentName = "Beta School", LAName = "LA Three", RegionId = "R1", RegionName = "North East", UrbanRuralId = "U1", UrbanRuralName = "Urban", TypeOfEstablishmentId = "34", TypeOfEstablishmentName = "Academy converter", PhaseOfEducationId = "P", PhaseOfEducationName = "Primary", OfficialSixthFormId = "0", OfficialSixthFormName = "Does not have sixth form", AdmissionsPolicyId = "1", AdmissionsPolicyName = "Non-selective", GenderId = "3", GenderName = "Mixed", ResourcedProvisionId = "1", ResourcedProvisionName = "Not applicable", NurseryProvisionName = "No", TotalCapacity = 300, TotalPupils = 230 });

        _similarSchoolsRepo.SetupGroups(
            new SimilarSchoolsPrimaryGroupsEntry { URN = "100001", NeighbourURN = "100002", Dist = "0.1", Rank = "1" },
            new SimilarSchoolsPrimaryGroupsEntry { URN = "100001", NeighbourURN = "100003", Dist = "0.2", Rank = "2" });

        _similarSchoolsRepo.SetupValues(
            new SimilarSchoolsPrimaryValuesEntry { URN = "100001", PPPerc = "20", Polar4QuintilePupils = "2", PStability = "95", PercentSchSupport = "10", PercentEAL = "5", IdaciPupils = "0.123", PercentageStatementOrEhp = "1.5", NumberOfPupils = "210", ReadMatAverage = "102", Ks1PriorRwmAverage = "11" },
            new SimilarSchoolsPrimaryValuesEntry { URN = "100002", PPPerc = "20", Polar4QuintilePupils = "2", PStability = "95", PercentSchSupport = "10", PercentEAL = "5", IdaciPupils = "0.123", PercentageStatementOrEhp = "1.5", NumberOfPupils = "220", ReadMatAverage = "101", Ks1PriorRwmAverage = "10" },
            new SimilarSchoolsPrimaryValuesEntry { URN = "100003", PPPerc = "20", Polar4QuintilePupils = "2", PStability = "95", PercentSchSupport = "10", PercentEAL = "5", IdaciPupils = "0.123", PercentageStatementOrEhp = "1.5", NumberOfPupils = "230", ReadMatAverage = "100", Ks1PriorRwmAverage = "9" });

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsExpected("60", "", "").WithRwmExpected("80", "", "")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsExpected("70", "", "").WithRwmExpected("70", "", "")));

        var response = await _sut.Execute(new("100001", SortBy: "GpsExpected"));

        response.SimilarSchoolsPage.Select(x => x.Urn).Should().Equal("100003", "100002");
        response.SimilarSchoolsPage.First().SortMetricName.Should().Be("Meeting expected standard in grammar, punctuation and spelling");
        response.SortOptions.Should().Contain(x => x.Key == "GpsExpected" && x.Selected);
    }

    [Fact]
    public async Task ReturnsPagedResultsAndAllResults()
    {
        var establishments = new List<Establishment>
        {
            new()
            {
                URN = "100001", EstablishmentName = "Current School", LAName = "LA One",
                Easting = 100000, Northing = 100000, RegionId = "R1", RegionName = "North East",
                UrbanRuralId = "U1", UrbanRuralName = "Urban", TypeOfEstablishmentId = "34", TypeOfEstablishmentName = "Academy converter",
                PhaseOfEducationId = "P", PhaseOfEducationName = "Primary", OfficialSixthFormId = "0", OfficialSixthFormName = "Does not have sixth form",
                AdmissionsPolicyId = "1", AdmissionsPolicyName = "Non-selective", GenderId = "3", GenderName = "Mixed",
                ResourcedProvisionId = "1", ResourcedProvisionName = "Not applicable", NurseryProvisionName = "No", TotalCapacity = 300, TotalPupils = 210
            }
        };

        var groups = new List<SimilarSchoolsPrimaryGroupsEntry>();
        var values = new List<SimilarSchoolsPrimaryValuesEntry>
        {
            new() { URN = "100001", PPPerc = "20.2", Polar4QuintilePupils = "2", PStability = "95", PercentSchSupport = "10", PercentEAL = "5", IdaciPupils = "0.123", PercentageStatementOrEhp = "1.5", NumberOfPupils = "210", ReadMatAverage = "102.4", Ks1PriorRwmAverage = "11.4" }
        };

        for (var i = 0; i < 12; i++)
        {
            var urn = (100002 + i).ToString();
            establishments.Add(new Establishment
            {
                URN = urn, EstablishmentName = $"Similar School {i + 1}", LAName = $"LA {i + 2}",
                Easting = 108046 + i, Northing = 100000, RegionId = "R1", RegionName = "North East",
                UrbanRuralId = "U1", UrbanRuralName = "Urban", TypeOfEstablishmentId = "34", TypeOfEstablishmentName = "Academy converter",
                PhaseOfEducationId = "P", PhaseOfEducationName = "Primary", OfficialSixthFormId = "0", OfficialSixthFormName = "Does not have sixth form",
                AdmissionsPolicyId = "1", AdmissionsPolicyName = "Non-selective", GenderId = "3", GenderName = "Mixed",
                ResourcedProvisionId = "1", ResourcedProvisionName = "Not applicable", NurseryProvisionName = "No", TotalCapacity = 300, TotalPupils = 200 + i
            });
            groups.Add(new SimilarSchoolsPrimaryGroupsEntry { URN = "100001", NeighbourURN = urn, Dist = $"0.{i + 1}", Rank = (i + 1).ToString() });
            values.Add(new SimilarSchoolsPrimaryValuesEntry { URN = urn, PPPerc = "20", Polar4QuintilePupils = "2", PStability = "95", PercentSchSupport = "10", PercentEAL = "5", IdaciPupils = "0.123", PercentageStatementOrEhp = "1.5", NumberOfPupils = (200 + i).ToString(), ReadMatAverage = "100", Ks1PriorRwmAverage = "10" });
        }

        _establishmentRepo.SetupEstablishments([.. establishments]);
        _similarSchoolsRepo.SetupGroups([.. groups]);
        _similarSchoolsRepo.SetupValues([.. values]);

        var response = await _sut.Execute(new("100001", Page: "2"));

        response.SimilarSchoolsPage.CurrentPage.Should().Be(2);
        response.SimilarSchoolsPage.Should().HaveCount(2);
        response.AllSimilarSchools.Should().HaveCount(12);
        response.SimilarSchoolsPage.Select(x => x.Rank).Should().Equal("11", "12");
    }
}
