using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Services;
using SAPSec.Web.Constants;
using SAPSec.Web.Helpers;
using SAPSec.Web.MockData;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Controllers;

/// <summary>
/// Controller for school details pages.
/// Single Responsibility: HTTP handling and view selection only.
/// </summary>
[Route("school/{urn}")]
public class SchoolController : Controller
{
    private readonly ISchoolDetailsService _schoolDetailsService;
    private readonly ILogger<SchoolController> _logger;

    public SchoolController(
        ISchoolDetailsService schoolDetailsService,
        ILogger<SchoolController> logger)
    {
        _schoolDetailsService = schoolDetailsService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string urn)
    {
        var school = _schoolDetailsService.TryGetByUrnAsync(urn);

        if (school is null)
        {
            _logger.LogInformation("School with URN {Urn} was not found", urn);
            return NotFound();
        }

        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewData(school);
        return View(school);
    }

    [HttpGet]
    [Route("school-details")]
    public async Task<IActionResult> SchoolDetails(string urn)
    {
        var school = _schoolDetailsService.TryGetByUrnAsync(urn);
        if (school != null)
        {
            SetSchoolViewData(school);
            ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
            return View(school);
        }
        else
        {
            _logger.LogInformation($"{urn} was not found on School Controller");
            return RedirectToAction("Error");
        }
    }

    [HttpGet]
    [Route("what-is-a-similar-school")]
    public async Task<IActionResult> WhatIsASimilarSchool(string urn)
    {
        var school = _schoolDetailsService.TryGetByUrnAsync(urn);
        if (school != null)
        {
            ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
            return View(school);
        }
        else
        {
            _logger.LogInformation($"{urn} was not found on School Controller");
            return RedirectToAction("Error");
        }
    }

    [HttpGet]
    [Route("view-similar-schools")]
    public async Task<IActionResult> ViewSimilarSchools(
        string urn,
        [FromQuery] SimilarSchoolsFilterViewModel filters,
        [FromQuery] string sortBy = "Attainment 8",
        [FromQuery] int page = 1)
    {
        var school = _schoolDetailsService.TryGetByUrnAsync(urn);
        if (school is null)
        {
            _logger.LogInformation("{Urn} was not found on School Controller", urn);
            return RedirectToAction("Error");
        }

        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewData(school);
        // TODO: Replace with actual database queries:
        //   1. Query v_similar_schools_secondary_groups WHERE urn = {urn}
        //   2. Join with v_similar_schools_secondary_values ON neighbour_urn = urn
        //   3. Join with v_establishment ON neighbour_urn = urn
        var allSchools = MockSimilarSchoolsData.GetSimilarSchools(int.Parse(urn));

        // Apply filters
        var filtered = ApplyFilters(allSchools, filters);

        // Apply sort
        filtered = ApplySort(filtered, sortBy);

        // Pagination
        const int pageSize = 10;
        var totalResults = filtered.Count;
        var pagedSchools = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var viewModel = new SimilarSchoolsPageViewModel
        {
            // SchoolDetails base properties (from the looked-up school)
            EstablishmentName = school.Name.Display(),
            PhaseOfEducation = school.PhaseOfEducation.Display(),
            Urn = int.Parse(urn),
            Schools = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Filters = filters,
            SortBy = sortBy,
            CurrentPage = page,
            PageSize = pageSize,
            TotalResults = filtered.Count,
            MapSchools = allSchools
        };

        return View(viewModel);
    }

    [HttpGet]
    [Route("attendance")]
    public async Task<IActionResult> Attendance(string urn)
    {
        var school = _schoolDetailsService.TryGetByUrn(urn);
        if (school != null)
        {
            ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
            return View(school);
        }
        else
        {
            _logger.LogInformation($"{urn} was not found on School Controller");
            return RedirectToAction("Error");
        }
    }

    [HttpGet]
    [Route("ks4-headline-measures")]
    public async Task<IActionResult> Ks4HeadlineMeasures(string urn)
    {
        var school = _schoolDetailsService.TryGetByUrn(urn);
        if (school != null)
        {
            ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
            return View(school);
        }
        else
        {
            _logger.LogInformation($"{urn} was not found on School Controller");
            return RedirectToAction("Error");
        }
    }

    [HttpGet]
    [Route("ks4-core-subjects")]
    public async Task<IActionResult> Ks4CoreSubjects(string urn)
    {
        var school = _schoolDetailsService.TryGetByUrn(urn);
        if (school != null)
        {
            ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
            return View(school);
        }
        else
        {
            _logger.LogInformation($"{urn} was not found on School Controller");
            return RedirectToAction("Error");
        }
    }

    #region Similar Schools - Private Helpers

    private static List<SimilarSchoolViewModel> ApplyFilters(
        List<SimilarSchoolViewModel> schools,
        SimilarSchoolsFilterViewModel filters)
    {
        var result = schools.AsEnumerable();

        // Location filters
        if (filters.SelectedRegions.Any())
        {
            result = result.Where(s =>
                filters.SelectedRegions.Contains(s.Region ?? string.Empty, StringComparer.OrdinalIgnoreCase));
        }

        if (filters.SelectedUrbanOrRural.Any())
        {
            result = result.Where(s =>
                filters.SelectedUrbanOrRural.Contains(s.UrbanOrRural ?? string.Empty, StringComparer.OrdinalIgnoreCase));
        }

        // School characteristics
        if (filters.SelectedPhaseOfEducation.Any())
        {
            result = result.Where(s =>
                filters.SelectedPhaseOfEducation.Contains(s.PhaseOfEducation ?? string.Empty, StringComparer.OrdinalIgnoreCase));
        }

        if (filters.SelectedGenderOfEntry.Any())
        {
            result = result.Where(s =>
                filters.SelectedGenderOfEntry.Contains(s.Gender ?? string.Empty, StringComparer.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(filters.SixthForm))
        {
            var hasSixthForm = filters.SixthForm == "Yes";
            result = result.Where(s => s.HasSixthForm == hasSixthForm);
        }

        if (!string.IsNullOrEmpty(filters.NurseryProvision))
        {
            var hasNursery = filters.NurseryProvision == "Yes";
            result = result.Where(s => s.HasNurseryProvision == hasNursery);
        }

        if (filters.SelectedAdmissionsPolicy.Any())
        {
            result = result.Where(s =>
                filters.SelectedAdmissionsPolicy.Contains(s.AdmissionsPolicy ?? string.Empty, StringComparer.OrdinalIgnoreCase));
        }

        if (filters.SelectedGovernanceStructure.Any())
        {
            result = result.Where(s =>
                filters.SelectedGovernanceStructure.Contains(s.TypeOfEstablishment ?? string.Empty, StringComparer.OrdinalIgnoreCase));
        }

        // Attendance filters
        if (!string.IsNullOrEmpty(filters.OverallAbsenceRate))
        {
            result = filters.OverallAbsenceRate switch
            {
                "Below 3%" => result.Where(s => s.OverallAbsenceRate < 3),
                "3% to 5%" => result.Where(s => s.OverallAbsenceRate >= 3 && s.OverallAbsenceRate < 5),
                "5% to 7%" => result.Where(s => s.OverallAbsenceRate >= 5 && s.OverallAbsenceRate < 7),
                "Above 7%" => result.Where(s => s.OverallAbsenceRate >= 7),
                _ => result
            };
        }

        if (!string.IsNullOrEmpty(filters.PersistentAbsenceRate))
        {
            result = filters.PersistentAbsenceRate switch
            {
                "Below 3%" => result.Where(s => s.PersistentAbsenceRate < 3),
                "3% to 5%" => result.Where(s => s.PersistentAbsenceRate >= 3 && s.PersistentAbsenceRate < 5),
                "5% to 7%" => result.Where(s => s.PersistentAbsenceRate >= 5 && s.PersistentAbsenceRate < 7),
                "Above 7%" => result.Where(s => s.PersistentAbsenceRate >= 7),
                _ => result
            };
        }

        return result.ToList();
    }

    private static List<SimilarSchoolViewModel> ApplySort(
        List<SimilarSchoolViewModel> schools,
        string sortBy)
    {
        return sortBy switch
        {
            "Attainment 8" => schools.OrderByDescending(s => s.Att8Scr).ToList(),
            "School name" => schools.OrderBy(s => s.EstablishmentName).ToList(),
            "Similarity" => schools.OrderBy(s => s.Rank).ToList(),
            "Overall absence rate" => schools.OrderBy(s => s.OverallAbsenceRate).ToList(),
            "Persistent absence rate" => schools.OrderBy(s => s.PersistentAbsenceRate).ToList(),
            _ => schools.OrderBy(s => s.Rank).ToList()
        };
    }
    #endregion
    private void SetSchoolViewData(Core.Model.SchoolDetails school)
    {
        ViewData["SchoolDetails"] = school;
    }
}