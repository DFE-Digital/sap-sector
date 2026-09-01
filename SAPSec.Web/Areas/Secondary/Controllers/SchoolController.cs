using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Measures.Attendance;
using SAPSec.Core.Features.Measures.Secondary;
using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.UseCases;
using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.Areas.Shared.ViewModels.School;
using SAPSec.Web.Constants;
using SAPSec.Web.Filters;
using SAPSec.Web.Services;
using SAPSec.Web.ViewModels;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Secondary.Controllers;

/// <summary>
/// Controller for school details pages.
/// Single Responsibility: HTTP handling and view selection only.
/// </summary>
[Area("Secondary")]
[Route("school/secondary/{urn}")]
[Authorize]
[RequireSchoolPhase(ExpectedSchoolPhase.Secondary)]
public class SchoolController(
        IUseCase<GetSchoolKs4HeadlineMeasuresRequest, GetSchoolKs4HeadlineMeasuresResponse> getSchoolKs4HeadlineMeasuresUseCase,
        IUseCase<GetSchoolKs4CoreSubjectsMeasuresRequest, GetSchoolKs4CoreSubjectsMeasuresResponse> getSchoolKs4CoreSubjectsUseCase,
        IUseCase<GetSchoolAttendanceMeasuresRequest, GetSchoolAttendanceMeasuresResponse> getAttendanceMeasuresUseCase,
        IRequestSchoolAccessor requestSchoolAccessor,
        ILogger<SchoolController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string urn)
    {
        var school = await requestSchoolAccessor.GetAsync(HttpContext, urn);

        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewDataAsync(school);
        return View(school);
    }

    [HttpGet]
    [Route("school-details")]
    public async Task<IActionResult> SchoolDetails(string urn)
    {
        var school = await requestSchoolAccessor.GetAsync(HttpContext, urn);
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewDataAsync(school);
        return View(school);
    }

    [HttpGet]
    [Route("what-is-a-similar-school")]
    public async Task<IActionResult> WhatIsASimilarSchool(string urn)
    {
        var school = await requestSchoolAccessor.GetAsync(HttpContext, urn);
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewDataAsync(school);
        return View(school);
    }

    [HttpGet]
    [Route("ks4-headline-measures")]
    public async Task<IActionResult> Ks4HeadlineMeasures(string urn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getSchoolKs4HeadlineMeasuresUseCase.Execute(new(urn, filters));

        PopulateViewData(response.School);

        var model = new ViewModels.School.Ks4HeadlineMeasuresPageViewModel
        {
            School = SchoolInfoViewModel.FromSchoolInfo(response.School),
            Attainment8 = MeasureViewModel.FromSecondaryMeasure(response.Attainment8, response.School),
            EnglishMaths = MeasureViewModel.FromSecondaryMeasure(response.EnglishMaths, response.School),
            Destinations = MeasureViewModel.FromSecondaryMeasure(response.Destinations, response.School)
        };

        return View(model);
    }

    [HttpGet]
    [Route("ks4-core-subjects")]
    public async Task<IActionResult> Ks4CoreSubjects(string urn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getSchoolKs4CoreSubjectsUseCase.Execute(new(urn, filters));

        PopulateViewData(response.School);

        var model = new ViewModels.School.Ks4CoreSubjectsPageViewModel
        {
            School = SchoolInfoViewModel.FromSchoolInfo(response.School),
            Measures = [
                MeasureViewModel.FromSecondaryMeasure(response.EnglishLanguage, response.School),
                MeasureViewModel.FromSecondaryMeasure(response.EnglishLiterature, response.School),
                MeasureViewModel.FromSecondaryMeasure(response.Maths, response.School),
                MeasureViewModel.FromSecondaryMeasure(response.CombinedScience, response.School),
                MeasureViewModel.FromSecondaryMeasure(response.Biology, response.School),
                MeasureViewModel.FromSecondaryMeasure(response.Chemistry, response.School),
                MeasureViewModel.FromSecondaryMeasure(response.Physics, response.School)
            ]
        };

        return View(model);
    }

    [HttpGet]
    [Route("attendance")]
    public async Task<IActionResult> Attendance(string urn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getAttendanceMeasuresUseCase.Execute(new(MeasurePhase.Secondary, urn, filters));

        PopulateViewData(response.School);

        var model = new AttendancePageViewModel
        {
            School = SchoolInfoViewModel.FromSchoolInfo(response.School),
            Absence = MeasureViewModel.FromPrimaryMeasure(response.Absence, response.School)
        };

        return View(model);
    }

    private void PopulateViewData(SchoolInfo currentSchool)
    {
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(currentSchool.Urn);
        ViewData[ViewDataKeys.SchoolLayout] = SchoolLayoutModel.FromSchoolInfo(currentSchool);
        ViewData[ViewDataKeys.SchoolNavigation] = SchoolSideNavigationViewModel.CreateSecondary(
            Url,
            currentSchool.Urn,
            ControllerContext.ActionDescriptor.ActionName);
    }

    private void SetSchoolViewDataAsync(SchoolDetails school)
    {
        ViewData[ViewDataKeys.SchoolDetails] = school;

        if (Url is null)
        {
            return;
        }

        ViewData[ViewDataKeys.SchoolNavigation] = SchoolSideNavigationViewModel.CreateSecondary(
            Url,
            school.Urn,
            ControllerContext.ActionDescriptor.ActionName);
    }
}
