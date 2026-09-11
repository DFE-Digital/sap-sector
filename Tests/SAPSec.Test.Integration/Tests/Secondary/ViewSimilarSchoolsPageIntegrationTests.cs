using FluentAssertions;
using SAPSec.Data.Dto;
using SAPSec.Data.Dto.Absence;
using SAPSec.Data.Dto.SimilarSchools.Secondary;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Secondary;

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
                PhaseOfEducationName = "Secondary",
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
                PhaseOfEducationName = "Secondary",
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
                PhaseOfEducationName = "Secondary",
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

        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(
            new SimilarSchoolsSecondaryGroupsEntry { URN = "100001", NeighbourURN = "100002", Rank = "1", Dist = "0.1" },
            new SimilarSchoolsSecondaryGroupsEntry { URN = "100001", NeighbourURN = "100003", Rank = "2", Dist = "0.2" });

        Fixture.SimilarSchoolsSecondaryRepository.SetupValues(
            new SimilarSchoolsSecondaryValuesEntry
            {
                URN = "100001",
                PPPerc = "20.2",
                Polar4QuintilePupils = "2",
                PStability = "95",
                PercentSchSupport = "10",
                PercentEAL = "5",
                IdaciPupils = "0.123",
                PercentageStatementOrEHP = "1.5",
                NumberOfPupils = "210",
                Att8Scr = "102.4",
                KS2MRP = "11.4"
            },
            new SimilarSchoolsSecondaryValuesEntry
            {
                URN = "100002",
                PPPerc = "25.4",
                Polar4QuintilePupils = "3",
                PStability = "94",
                PercentSchSupport = "12",
                PercentEAL = "6",
                IdaciPupils = "0.223",
                PercentageStatementOrEHP = "2.5",
                NumberOfPupils = "220",
                Att8Scr = "101.4",
                KS2MRP = "10.4"
            },
            new SimilarSchoolsSecondaryValuesEntry
            {
                URN = "100003",
                PPPerc = "30.5",
                Polar4QuintilePupils = "4",
                PStability = "93",
                PercentSchSupport = "14",
                PercentEAL = "7",
                IdaciPupils = "0.323",
                PercentageStatementOrEHP = "3.5",
                NumberOfPupils = "230",
                Att8Scr = "100.4",
                KS2MRP = "9.4"
            });

        Fixture.AbsenceRepository.SetupEstablishmentAbsence(
            new EstablishmentAbsence { Id = "100001", Abs_Tot_Est_Current_Pct = "4.5", Abs_Persistent_Est_Current_Pct = "12.0" },
            new EstablishmentAbsence { Id = "100002", Abs_Tot_Est_Current_Pct = "5.1", Abs_Persistent_Est_Current_Pct = "13.0" },
            new EstablishmentAbsence { Id = "100003", Abs_Tot_Est_Current_Pct = "6.1", Abs_Persistent_Est_Current_Pct = "14.0" });
        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithAttainment8("81", "", "").WithEngMaths59("70", "", "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithAttainment8("78", "", "").WithEngMaths59("75", "", "")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").ViewSimilarSchools, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist("secondary-similar-schools-filter");
        filter.TextContent.Should().Contain("Filters");
        filter.TextContent.Should().Contain("Location");
        filter.TextContent.Should().Contain("Distance");
        filter.TextContent.Should().Contain("Up to 5 miles");
        filter.TextContent.Should().Contain("Region");
        filter.TextContent.Should().Contain("School characteristics");
        filter.TextContent.Should().Contain("Attendance");

        var list = page.ElementWithTestIdShouldExist("secondary-similar-schools-list");
        list.TextContent.Should().Contain("Test School 2");
        list.TextContent.Should().Contain("Attainment 8");
        list.TextContent.Should().Contain("81.0");
        list.TextContent.Should().Contain("Test School 3");
        list.TextContent.Should().Contain("78.0");

        page.QuerySelector("#sort-by")?.TextContent.Should().Contain("Attainment 8");
        page.QuerySelector("#sort-by")?.TextContent.Should().Contain("English and maths GCSEs (Grade 5 and above)");

        var links = list.QuerySelectorAll("a").Select(x => x.GetAttribute("href"));
        links.Should().BeEquivalentTo([
            Routes.SecondarySchool("100001").Comparison("100002").Similarity,
            Routes.SecondarySchool("100001").Comparison("100003").Similarity
        ]);
    }

    [Fact]
    public async Task ViewSimilarSchools_PaginatesSecondarySimilarSchools()
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
                PhaseOfEducationName = "Secondary",
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

        var groups = new List<SimilarSchoolsSecondaryGroupsEntry>();
        var values = new List<SimilarSchoolsSecondaryValuesEntry>
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
                PercentageStatementOrEHP = "1.5",
                NumberOfPupils = "210",
                Att8Scr = "102.4",
                KS2MRP = "11.4"
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
                PhaseOfEducationName = "Secondary",
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
            groups.Add(new SimilarSchoolsSecondaryGroupsEntry { URN = "100001", NeighbourURN = urn, Rank = (i + 1).ToString(), Dist = $"0.{i + 1}" });
            values.Add(new SimilarSchoolsSecondaryValuesEntry
            {
                URN = urn,
                PPPerc = "20",
                Polar4QuintilePupils = "2",
                PStability = "95",
                PercentSchSupport = "10",
                PercentEAL = "5",
                IdaciPupils = "0.123",
                PercentageStatementOrEHP = "1.5",
                NumberOfPupils = (220 + i).ToString(),
                Att8Scr = "100",
                KS2MRP = "10"
            });
        }

        Fixture.EstablishmentRepository.SetupEstablishments([.. establishments]);
        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups([.. groups]);
        Fixture.SimilarSchoolsSecondaryRepository.SetupValues([.. values]);
        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            groups.Select((group, index) =>
                Build.Ks4Performance.Establishment(group.NeighbourURN, x => x.WithAttainment8((100 - index).ToString(), "", "").WithEngMaths59((50 + index).ToString(), "", "")))
                .ToArray());

        var page = await Fixture.RequestPageAsync($"{Routes.SecondarySchool("100001").ViewSimilarSchools}?page=2", HttpStatusCode.OK);

        var list = page.ElementWithTestIdShouldExist("secondary-similar-schools-list");
        list.TextContent.Should().Contain("Test School 12");
        list.TextContent.Should().Contain("Test School 13");
        list.TextContent.Should().NotContain("Test School 2");

        page.QuerySelector(".govuk-pagination").Should().NotBeNull();
        page.QuerySelector("a[rel='prev']")?.GetAttribute("href").Should().Contain("page=1");
    }

    [Fact]
    public async Task ViewSimilarSchools_SortsByEngMaths()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            new Establishment { URN = "100001", EstablishmentName = "Test School 1", LAId = "001", LAName = "Test LA 1", PhaseOfEducationId = "P", PhaseOfEducationName = "Secondary", RegionId = "R1", RegionName = "North East", UrbanRuralId = "U1", UrbanRuralName = "Urban", TypeOfEstablishmentId = "34", TypeOfEstablishmentName = "Academy converter", AdmissionsPolicyId = "1", AdmissionsPolicyName = "Non-selective", GenderId = "3", GenderName = "Mixed", NurseryProvisionName = "No", OfficialSixthFormId = "0", OfficialSixthFormName = "Does not have sixth form", ResourcedProvisionId = "1", ResourcedProvisionName = "Not applicable", Easting = 100000, Northing = 100000, TotalCapacity = 300, TotalPupils = 210 },
            new Establishment { URN = "100002", EstablishmentName = "Alpha School", LAId = "002", LAName = "Test LA 2", PhaseOfEducationId = "P", PhaseOfEducationName = "Secondary", RegionId = "R1", RegionName = "North East", UrbanRuralId = "U1", UrbanRuralName = "Urban", TypeOfEstablishmentId = "34", TypeOfEstablishmentName = "Academy converter", AdmissionsPolicyId = "1", AdmissionsPolicyName = "Non-selective", GenderId = "3", GenderName = "Mixed", NurseryProvisionName = "No", OfficialSixthFormId = "0", OfficialSixthFormName = "Does not have sixth form", ResourcedProvisionId = "1", ResourcedProvisionName = "Not applicable", Easting = 108046, Northing = 100000, TotalCapacity = 300, TotalPupils = 220 },
            new Establishment { URN = "100003", EstablishmentName = "Beta School", LAId = "003", LAName = "Test LA 3", PhaseOfEducationId = "P", PhaseOfEducationName = "Secondary", RegionId = "R1", RegionName = "North East", UrbanRuralId = "U1", UrbanRuralName = "Urban", TypeOfEstablishmentId = "34", TypeOfEstablishmentName = "Academy converter", AdmissionsPolicyId = "1", AdmissionsPolicyName = "Non-selective", GenderId = "3", GenderName = "Mixed", NurseryProvisionName = "No", OfficialSixthFormId = "0", OfficialSixthFormName = "Does not have sixth form", ResourcedProvisionId = "1", ResourcedProvisionName = "Not applicable", Easting = 108047, Northing = 100000, TotalCapacity = 300, TotalPupils = 230 });

        Fixture.SimilarSchoolsSecondaryRepository.SetupGroups(
            new SimilarSchoolsSecondaryGroupsEntry { URN = "100001", NeighbourURN = "100002", Rank = "1", Dist = "0.1" },
            new SimilarSchoolsSecondaryGroupsEntry { URN = "100001", NeighbourURN = "100003", Rank = "2", Dist = "0.2" });
        Fixture.SimilarSchoolsSecondaryRepository.SetupValues(
            new SimilarSchoolsSecondaryValuesEntry
            {
                URN = "100001",
                PPPerc = "20",
                Polar4QuintilePupils = "2",
                PStability = "95",
                PercentSchSupport = "10",
                PercentEAL = "5",
                IdaciPupils = "0.123",
                PercentageStatementOrEHP = "1.5",
                NumberOfPupils = "210",
                Att8Scr = "102",
                KS2MRP = "11"
            },
            new SimilarSchoolsSecondaryValuesEntry
            {
                URN = "100002",
                PPPerc = "20",
                Polar4QuintilePupils = "2",
                PStability = "95",
                PercentSchSupport = "10",
                PercentEAL = "5",
                IdaciPupils = "0.123",
                PercentageStatementOrEHP = "1.5",
                NumberOfPupils = "220",
                Att8Scr = "101",
                KS2MRP = "10"
            },
            new SimilarSchoolsSecondaryValuesEntry
            {
                URN = "100003",
                PPPerc = "20",
                Polar4QuintilePupils = "2",
                PStability = "95",
                PercentSchSupport = "10",
                PercentEAL = "5",
                IdaciPupils = "0.123",
                PercentageStatementOrEHP = "1.5",
                NumberOfPupils = "230",
                Att8Scr = "100",
                KS2MRP = "9"
            });
        Fixture.Ks4PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithAttainment8("81", "", "").WithEngMaths59("60", "", "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithAttainment8("78", "", "").WithEngMaths59("70", "", "")));

        var page = await Fixture.RequestPageAsync($"{Routes.SecondarySchool("100001").ViewSimilarSchools}?sortBy=EngMat", HttpStatusCode.OK);

        var list = page.ElementWithTestIdShouldExist("secondary-similar-schools-list");
        list.TextContent.Should().Contain("Beta School");
        list.TextContent.Should().Contain("70%");
        list.TextContent.Should().Contain("English and maths GCSEs (Grade 5 and above)");
    }

    [Fact]
    public async Task ViewSimilarSchools_WhenNoSimilarSchoolsGroup_ShowsNoResultsMessageInsteadOfErrorPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            new Establishment { URN = "100001", EstablishmentName = "Test School 1", LAId = "001", LAName = "Test LA 1", PhaseOfEducationId = "S", PhaseOfEducationName = "Secondary", RegionId = "R1", RegionName = "North East", UrbanRuralId = "U1", UrbanRuralName = "Urban", TypeOfEstablishmentId = "34", TypeOfEstablishmentName = "Academy converter", AdmissionsPolicyId = "1", AdmissionsPolicyName = "Non-selective", GenderId = "3", GenderName = "Mixed", NurseryProvisionName = "No", OfficialSixthFormId = "0", OfficialSixthFormName = "Does not have sixth form", ResourcedProvisionId = "1", ResourcedProvisionName = "Not applicable", Easting = 100000, Northing = 100000, TotalCapacity = 300, TotalPupils = 210 });

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").ViewSimilarSchools, HttpStatusCode.OK);

        page.Body!.TextContent.Should().Contain("There are no similar schools available for this school.");
        page.QuerySelector("#sort-by").Should().BeNull();
    }
}
