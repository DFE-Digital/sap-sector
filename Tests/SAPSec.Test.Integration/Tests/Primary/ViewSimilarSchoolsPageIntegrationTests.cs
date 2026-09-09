using FluentAssertions;
using SAPSec.Data.Dto;
using SAPSec.Data.Dto.Absence;
using SAPSec.Data.Dto.SimilarSchools.Primary;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Primary;

public class ViewSimilarSchoolsPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    [Fact]
    public async Task ViewSimilarSchools_ShowsExistingSimilarSchoolFiltersAndResults()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            new Establishment
            {
                URN = "100001",
                EstablishmentName = "Test School 1",
                LAId = "001",
                LAName = "Test LA 1",
                PhaseOfEducationId = "P",
                PhaseOfEducationName = "Primary",
                RegionId = "R1",
                RegionName = "North East",
                UrbanRuralId = "U1",
                UrbanRuralName = "Urban",
                TypeOfEstablishmentId = "34",
                TypeOfEstablishmentName = "Academy converter",
                AdmissionsPolicyId = "1",
                AdmissionsPolicyName = "Non-selective",
                GenderId = "3",
                GenderName = "Mixed",
                NurseryProvisionName = "No",
                OfficialSixthFormId = "0",
                OfficialSixthFormName = "Does not have sixth form",
                ResourcedProvisionId = "1",
                ResourcedProvisionName = "Not applicable",
                Easting = 100000,
                Northing = 100000,
                TotalCapacity = 300,
                TotalPupils = 210
            },
            new Establishment
            {
                URN = "100002",
                EstablishmentName = "Test School 2",
                LAId = "002",
                LAName = "Test LA 2",
                PhaseOfEducationId = "P",
                PhaseOfEducationName = "Primary",
                RegionId = "R1",
                RegionName = "North East",
                UrbanRuralId = "U1",
                UrbanRuralName = "Urban",
                TypeOfEstablishmentId = "34",
                TypeOfEstablishmentName = "Academy converter",
                AdmissionsPolicyId = "1",
                AdmissionsPolicyName = "Non-selective",
                GenderId = "3",
                GenderName = "Mixed",
                NurseryProvisionName = "Yes",
                OfficialSixthFormId = "0",
                OfficialSixthFormName = "Does not have sixth form",
                ResourcedProvisionId = "4",
                ResourcedProvisionName = "Resourced provision",
                Easting = 108046,
                Northing = 100000,
                TotalCapacity = 400,
                TotalPupils = 220
            },
            new Establishment
            {
                URN = "100003",
                EstablishmentName = "Test School 3",
                LAId = "003",
                LAName = "Test LA 3",
                PhaseOfEducationId = "P",
                PhaseOfEducationName = "Primary",
                RegionId = "R2",
                RegionName = "South East",
                UrbanRuralId = "R1",
                UrbanRuralName = "Rural",
                TypeOfEstablishmentId = "28",
                TypeOfEstablishmentName = "Community school",
                AdmissionsPolicyId = "1",
                AdmissionsPolicyName = "Non-selective",
                GenderId = "2",
                GenderName = "Girls",
                NurseryProvisionName = "No",
                OfficialSixthFormId = "0",
                OfficialSixthFormName = "Does not have sixth form",
                ResourcedProvisionId = "8",
                ResourcedProvisionName = "SEN unit",
                Easting = 180467,
                Northing = 100000,
                TotalCapacity = 500,
                TotalPupils = 230
            });

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            new SimilarSchoolsPrimaryGroupsEntry { URN = "100001", NeighbourURN = "100002", Rank = "1", Dist = "0.1" },
            new SimilarSchoolsPrimaryGroupsEntry { URN = "100001", NeighbourURN = "100003", Rank = "2", Dist = "0.2" });

        Fixture.SimilarSchoolsPrimaryRepository.SetupValues(
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

        Fixture.AbsenceRepository.SetupEstablishmentAbsence(
            new EstablishmentAbsence { Id = "100001", Abs_Tot_Est_Current_Pct = "4.5", Abs_Persistent_Est_Current_Pct = "12.0" },
            new EstablishmentAbsence { Id = "100002", Abs_Tot_Est_Current_Pct = "5.1", Abs_Persistent_Est_Current_Pct = "13.0" },
            new EstablishmentAbsence { Id = "100003", Abs_Tot_Est_Current_Pct = "6.1", Abs_Persistent_Est_Current_Pct = "14.0" });
        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected("81", "", "").WithGpsExpected("70", "", "")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmExpected("78", "", "").WithGpsExpected("75", "", "")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").ViewSimilarSchools, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist("primary-similar-schools-filter");
        filter.TextContent.Should().Contain("Filters");
        filter.TextContent.Should().Contain("Location");
        filter.TextContent.Should().Contain("Distance");
        filter.TextContent.Should().Contain("Up to 5 miles");
        filter.TextContent.Should().Contain("Region");
        filter.TextContent.Should().Contain("School characteristics");
        filter.TextContent.Should().Contain("Attendance");

        var list = page.ElementWithTestIdShouldExist("primary-similar-schools-list");
        list.TextContent.Should().Contain("Test School 2");
        list.TextContent.Should().Contain("Meeting expected standard in reading, writing and maths");
        list.TextContent.Should().Contain("81%");
        list.TextContent.Should().Contain("Test School 3");
        list.TextContent.Should().Contain("78%");

        page.QuerySelector("#sort-by")?.TextContent.Should().Contain("Meeting expected standard in reading, writing and maths");
        page.QuerySelector("#sort-by")?.TextContent.Should().Contain("Meeting expected standard in grammar, punctuation and spelling");

        var links = list.QuerySelectorAll("a").Select(x => x.GetAttribute("href"));
        links.Should().BeEquivalentTo([
            Routes.PrimarySchool("100001").Comparison("100002").Similarity,
            Routes.PrimarySchool("100001").Comparison("100003").Similarity
        ]);
    }

    [Fact]
    public async Task ViewSimilarSchools_PaginatesPrimarySimilarSchools()
    {
        var establishments = new List<Establishment>
        {
            new()
            {
                URN = "100001",
                EstablishmentName = "Test School 1",
                LAId = "001",
                LAName = "Test LA 1",
                PhaseOfEducationId = "P",
                PhaseOfEducationName = "Primary",
                RegionId = "R1",
                RegionName = "North East",
                UrbanRuralId = "U1",
                UrbanRuralName = "Urban",
                TypeOfEstablishmentId = "34",
                TypeOfEstablishmentName = "Academy converter",
                AdmissionsPolicyId = "1",
                AdmissionsPolicyName = "Non-selective",
                GenderId = "3",
                GenderName = "Mixed",
                NurseryProvisionName = "No",
                OfficialSixthFormId = "0",
                OfficialSixthFormName = "Does not have sixth form",
                ResourcedProvisionId = "1",
                ResourcedProvisionName = "Not applicable",
                Easting = 100000,
                Northing = 100000,
                TotalCapacity = 300,
                TotalPupils = 210
            }
        };

        var groups = new List<SimilarSchoolsPrimaryGroupsEntry>();
        var values = new List<SimilarSchoolsPrimaryValuesEntry>
        {
            new()
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
            }
        };

        for (var i = 0; i < 12; i++)
        {
            var urn = (100002 + i).ToString();
            establishments.Add(new Establishment
            {
                URN = urn,
                EstablishmentName = $"Test School {i + 2}",
                LAId = $"00{i + 2}",
                LAName = $"Test LA {i + 2}",
                PhaseOfEducationId = "P",
                PhaseOfEducationName = "Primary",
                RegionId = "R1",
                RegionName = "North East",
                UrbanRuralId = "U1",
                UrbanRuralName = "Urban",
                TypeOfEstablishmentId = "34",
                TypeOfEstablishmentName = "Academy converter",
                AdmissionsPolicyId = "1",
                AdmissionsPolicyName = "Non-selective",
                GenderId = "3",
                GenderName = "Mixed",
                NurseryProvisionName = "No",
                OfficialSixthFormId = "0",
                OfficialSixthFormName = "Does not have sixth form",
                ResourcedProvisionId = "1",
                ResourcedProvisionName = "Not applicable",
                Easting = 108046 + i,
                Northing = 100000,
                TotalCapacity = 300,
                TotalPupils = 220 + i
            });
            groups.Add(new SimilarSchoolsPrimaryGroupsEntry { URN = "100001", NeighbourURN = urn, Rank = (i + 1).ToString(), Dist = $"0.{i + 1}" });
            values.Add(new SimilarSchoolsPrimaryValuesEntry
            {
                URN = urn,
                PPPerc = "20",
                Polar4QuintilePupils = "2",
                PStability = "95",
                PercentSchSupport = "10",
                PercentEAL = "5",
                IdaciPupils = "0.123",
                PercentageStatementOrEhp = "1.5",
                NumberOfPupils = (220 + i).ToString(),
                ReadMatAverage = "100",
                Ks1PriorRwmAverage = "10"
            });
        }

        Fixture.EstablishmentRepository.SetupEstablishments([.. establishments]);
        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups([.. groups]);
        Fixture.SimilarSchoolsPrimaryRepository.SetupValues([.. values]);
        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            groups.Select((group, index) =>
                Build.Ks2Performance.Establishment(group.NeighbourURN, x => x.WithRwmExpected((100 - index).ToString(), "", "").WithGpsExpected((50 + index).ToString(), "", "")))
                .ToArray());

        var page = await Fixture.RequestPageAsync($"{Routes.PrimarySchool("100001").ViewSimilarSchools}?page=2", HttpStatusCode.OK);

        var list = page.ElementWithTestIdShouldExist("primary-similar-schools-list");
        list.TextContent.Should().Contain("Test School 12");
        list.TextContent.Should().Contain("Test School 13");
        list.TextContent.Should().NotContain("Test School 2");

        page.QuerySelector(".govuk-pagination").Should().NotBeNull();
        page.QuerySelector("a[rel='prev']")?.GetAttribute("href").Should().Contain("page=1");
    }

    [Fact]
    public async Task ViewSimilarSchools_SortsByGpsExpected()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            new Establishment { URN = "100001", EstablishmentName = "Test School 1", LAId = "001", LAName = "Test LA 1", PhaseOfEducationId = "P", PhaseOfEducationName = "Primary", RegionId = "R1", RegionName = "North East", UrbanRuralId = "U1", UrbanRuralName = "Urban", TypeOfEstablishmentId = "34", TypeOfEstablishmentName = "Academy converter", AdmissionsPolicyId = "1", AdmissionsPolicyName = "Non-selective", GenderId = "3", GenderName = "Mixed", NurseryProvisionName = "No", OfficialSixthFormId = "0", OfficialSixthFormName = "Does not have sixth form", ResourcedProvisionId = "1", ResourcedProvisionName = "Not applicable", Easting = 100000, Northing = 100000, TotalCapacity = 300, TotalPupils = 210 },
            new Establishment { URN = "100002", EstablishmentName = "Alpha School", LAId = "002", LAName = "Test LA 2", PhaseOfEducationId = "P", PhaseOfEducationName = "Primary", RegionId = "R1", RegionName = "North East", UrbanRuralId = "U1", UrbanRuralName = "Urban", TypeOfEstablishmentId = "34", TypeOfEstablishmentName = "Academy converter", AdmissionsPolicyId = "1", AdmissionsPolicyName = "Non-selective", GenderId = "3", GenderName = "Mixed", NurseryProvisionName = "No", OfficialSixthFormId = "0", OfficialSixthFormName = "Does not have sixth form", ResourcedProvisionId = "1", ResourcedProvisionName = "Not applicable", Easting = 108046, Northing = 100000, TotalCapacity = 300, TotalPupils = 220 },
            new Establishment { URN = "100003", EstablishmentName = "Beta School", LAId = "003", LAName = "Test LA 3", PhaseOfEducationId = "P", PhaseOfEducationName = "Primary", RegionId = "R1", RegionName = "North East", UrbanRuralId = "U1", UrbanRuralName = "Urban", TypeOfEstablishmentId = "34", TypeOfEstablishmentName = "Academy converter", AdmissionsPolicyId = "1", AdmissionsPolicyName = "Non-selective", GenderId = "3", GenderName = "Mixed", NurseryProvisionName = "No", OfficialSixthFormId = "0", OfficialSixthFormName = "Does not have sixth form", ResourcedProvisionId = "1", ResourcedProvisionName = "Not applicable", Easting = 108047, Northing = 100000, TotalCapacity = 300, TotalPupils = 230 });

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            new SimilarSchoolsPrimaryGroupsEntry { URN = "100001", NeighbourURN = "100002", Rank = "1", Dist = "0.1" },
            new SimilarSchoolsPrimaryGroupsEntry { URN = "100001", NeighbourURN = "100003", Rank = "2", Dist = "0.2" });
        Fixture.SimilarSchoolsPrimaryRepository.SetupValues(
            new SimilarSchoolsPrimaryValuesEntry { URN = "100001", PPPerc = "20", Polar4QuintilePupils = "2", PStability = "95", PercentSchSupport = "10", PercentEAL = "5", IdaciPupils = "0.123", PercentageStatementOrEhp = "1.5", NumberOfPupils = "210", ReadMatAverage = "102", Ks1PriorRwmAverage = "11" },
            new SimilarSchoolsPrimaryValuesEntry { URN = "100002", PPPerc = "20", Polar4QuintilePupils = "2", PStability = "95", PercentSchSupport = "10", PercentEAL = "5", IdaciPupils = "0.123", PercentageStatementOrEhp = "1.5", NumberOfPupils = "220", ReadMatAverage = "101", Ks1PriorRwmAverage = "10" },
            new SimilarSchoolsPrimaryValuesEntry { URN = "100003", PPPerc = "20", Polar4QuintilePupils = "2", PStability = "95", PercentSchSupport = "10", PercentEAL = "5", IdaciPupils = "0.123", PercentageStatementOrEhp = "1.5", NumberOfPupils = "230", ReadMatAverage = "100", Ks1PriorRwmAverage = "9" });
        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected("81", "", "").WithGpsExpected("60", "", "")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmExpected("78", "", "").WithGpsExpected("70", "", "")));

        var page = await Fixture.RequestPageAsync($"{Routes.PrimarySchool("100001").ViewSimilarSchools}?sortBy=GpsExpected", HttpStatusCode.OK);

        var list = page.ElementWithTestIdShouldExist("primary-similar-schools-list");
        list.TextContent.Should().Contain("Beta School");
        list.TextContent.Should().Contain("70%");
        list.TextContent.Should().Contain("Meeting expected standard in grammar, punctuation and spelling");
    }

    [Fact]
    public async Task ViewSimilarSchools_WhenNoSimilarSchoolsGroup_ShowsNoResultsMessageInsteadOfErrorPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            new Establishment { URN = "100001", EstablishmentName = "Test School 1", LAId = "001", LAName = "Test LA 1", PhaseOfEducationId = "P", PhaseOfEducationName = "Primary", RegionId = "R1", RegionName = "North East", UrbanRuralId = "U1", UrbanRuralName = "Urban", TypeOfEstablishmentId = "34", TypeOfEstablishmentName = "Academy converter", AdmissionsPolicyId = "1", AdmissionsPolicyName = "Non-selective", GenderId = "3", GenderName = "Mixed", NurseryProvisionName = "No", OfficialSixthFormId = "0", OfficialSixthFormName = "Does not have sixth form", ResourcedProvisionId = "1", ResourcedProvisionName = "Not applicable", Easting = 100000, Northing = 100000, TotalCapacity = 300, TotalPupils = 210 });

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").ViewSimilarSchools, HttpStatusCode.OK);

        page.Body!.TextContent.Should().Contain("There are no similar schools available for this school.");
        page.QuerySelector("#sort-by").Should().BeNull();
    }
}
