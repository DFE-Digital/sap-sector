using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Controllers;

[Route("school/{urn}/view-similar-schools/{comparisonUrn}")]
public class SimilarSchoolsComparisonController : Controller
{
    [HttpGet]
    public IActionResult Index(string urn, string comparisonUrn)
    {
        var comparisonSchool = GetMockCompareSchool(int.Parse(comparisonUrn));
        var school = GetMockSchool(int.Parse(urn));

        var model = new SimilarSchoolsComparisonViewModel()
        {
            School = school,
            CompareSchool = comparisonSchool
        };

        SetComparisonSchoolViewData(model);
        return View(model);
    }
    
    
    [HttpGet]
    [Route("Ks4HeadlineMeasures")]
    public IActionResult Ks4HeadlineMeasures(string urn, string comparisonUrn)
    {
        var comparisonSchool = GetMockCompareSchool(int.Parse(comparisonUrn));
        var school = GetMockSchool(int.Parse(urn));

        var model = new SimilarSchoolsComparisonViewModel()
        {
            School = school,
            CompareSchool = comparisonSchool
        };

        SetComparisonSchoolViewData(model);
        return View(model);
    }

    [HttpGet]
    [Route("Ks4CoreSubjects")]
    public IActionResult Ks4CoreSubjects(string urn, string comparisonUrn)
    {
        var comparisonSchool = GetMockCompareSchool(int.Parse(comparisonUrn));
        var school = GetMockSchool(int.Parse(urn));

        var model = new SimilarSchoolsComparisonViewModel()
        {
            School = school,
            CompareSchool = comparisonSchool
        };

        SetComparisonSchoolViewData(model);
        return View(model);
    }

    [HttpGet]
    [Route("attendance")]
    public IActionResult Attendance(string urn, string comparisonUrn)
    {
        var comparisonSchool = GetMockCompareSchool(int.Parse(comparisonUrn));
        var school = GetMockSchool(int.Parse(urn));

        var model = new SimilarSchoolsComparisonViewModel()
        {
            School = school,
            CompareSchool = comparisonSchool
        };

        SetComparisonSchoolViewData(model);
        return View(model);
    }

    [HttpGet]
    [Route("SchoolDetails")]
    public IActionResult SchoolDetails(string urn, string comparisonUrn)
    {
        var comparisonSchool = GetMockCompareSchool(int.Parse(comparisonUrn));
        var school = GetMockSchool(int.Parse(urn));

        var model = new SimilarSchoolsComparisonViewModel()
        {
            School = school,
            CompareSchool = comparisonSchool
        };

        SetComparisonSchoolViewData(model);
        return View(model);
    }

    private void SetComparisonSchoolViewData(SimilarSchoolsComparisonViewModel data)
    {
        ViewData["ComparisonSchool"] = data;
    }
    
    private static SimilarSchoolViewModel GetMockSchool(int urn)
    {
        return new SimilarSchoolViewModel()
        {
            Urn = urn, NeighbourUrn = 100750, Dist = 0.3573, Rank = 1,
            Ks2Rp = 107.67, Ks2Mp = 106.23, PpPerc = 23.29, PercentEal = 30.77,
            Polar4QuintilePupils = 4, PStability = 90.21, IdaciPupils = 0.201,
            PercentSchSupport = 14.73, NumberOfPupils = 958, PercentStatementOrEhp = 1.57,
            Att8Scr = 60.7,
            EstablishmentName = "Abbey Academy",
            Street = "Abbey Way", Town = "Sheffield", County = "South Yorkshire", Postcode = "S17 5DE",
            PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy",
            Region = "Yorkshire and The Humber", UrbanOrRural = "Urban",
            AdmissionsPolicy = "Comprehensive", Gender = "Mixed",
            HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None",
            SchoolCapacity = 1200, OverallAbsenceRate = 5.2, PersistentAbsenceRate = 12.1
        };
    }

    private static SimilarSchoolViewModel GetMockCompareSchool(int urn)
    {
        return new SimilarSchoolViewModel()
        {
            Urn = urn, NeighbourUrn = 102153, Dist = 0.3646, Rank = 2,
            Ks2Rp = 106.89, Ks2Mp = 105.41, PpPerc = 28.15, PercentEal = 25.60,
            Polar4QuintilePupils = 4, PStability = 91.10, IdaciPupils = 0.195,
            PercentSchSupport = 12.80, NumberOfPupils = 1120, PercentStatementOrEhp = 1.42,
            Att8Scr = 53.2,
            EstablishmentName = "Wentworth High School",
            Street = "Sycamore Rise", Town = "Bakewell", County = "Derbyshire", Postcode = "DE45 1GH",
            PhaseOfEducation = "Secondary", TypeOfEstablishment = "Foundation school",
            Region = "East Midlands", UrbanOrRural = "Rural",
            AdmissionsPolicy = "Comprehensive", Gender = "Mixed",
            HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None",
            SchoolCapacity = 800, OverallAbsenceRate = 4.8, PersistentAbsenceRate = 10.3
        };
    }
}