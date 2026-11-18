using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.FeatureFlags;
using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Measures.Primary;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Filtering;
using SAPSec.Web.Filters;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Controllers;

/// <summary>
/// Demo pages for components that are built ahead of the page they belong on, so they can be
/// reviewed and accessibility tested. Placeholder data only; behind feature flags.
/// </summary>
[Controller]
[Route("component-examples")]
public class ComponentExamplesController(IKs2ProgressPupilNumbersProvider pupilNumbersProvider) : Controller
{
    public const string Ks2ProgressNumberOfPupilsPath = "/component-examples/ks2-progress-number-of-pupils";

    internal const string HtmlPrefix = "ks2-progress";
    internal const string SubjectKey = $"{HtmlPrefix}-subject";
    internal const string CharacteristicKey = $"{HtmlPrefix}-characteristic";
    internal const string PageKey = "page";
    internal const string ComparisonPage = "comparison";

    private const int LatestYear = 2024;

    private static readonly (string Key, string Name)[] Subjects =
    [
        ("reading", "Reading"),
        ("writing", "Writing"),
        ("maths", "Maths")
    ];

    private static readonly (string Key, string Name)[] Characteristics =
    [
        (IKs2ProgressPupilNumbersProvider.AllPupils, "All pupils"),
        ("boy", "Boys"),
        ("grl", "Girls"),
        ("dis", "Disadvantaged pupils"),
        ("ndi", "Non-disadvantaged pupils"),
        ("eal", "English as an additional language"),
        ("nmo", "Non-mobile pupils"),
        ("hpa", "Pupils with high prior attainment"),
        ("mpa", "Pupils with medium prior attainment"),
        ("lpa", "Pupils with low prior attainment")
    ];

    private static readonly SchoolInfo CurrentSchool = Placeholder("900001", "Example Primary School");
    private static readonly SchoolInfo SimilarSchool = Placeholder("900002", "Example Similar Primary School");

    private static readonly TopPerformer[] PlaceholderTopPerformers =
    [
        new(1, "900003", "Example Top Primary School", 4.2m),
        new(2, "900004", "Example Second Primary School", 3.1m),
        new(3, CurrentSchool.Urn, CurrentSchool.Name, 1.8m, IsCurrentSchool: true),
        new(4, "900005", "Example Fourth Primary School", 0.9m),
        new(5, "900006", "Example Fifth Primary School", -0.4m)
    ];

    [HttpGet("ks2-progress-number-of-pupils")]
    [RequireFeatureFlag(Flags.EnableKs2ProgressNumberOfPupils)]
    public async Task<IActionResult> Ks2ProgressNumberOfPupils(
        [FromQuery(Name = PageKey)] string? page,
        [FromQuery(Name = SubjectKey)] string? subject,
        [FromQuery(Name = CharacteristicKey)] string? characteristic)
    {
        var isComparison = string.Equals(page, ComparisonPage, StringComparison.OrdinalIgnoreCase);
        var subjectKey = Subjects.Any(s => s.Key == subject) ? subject! : Subjects[0].Key;
        var characteristicKey = Characteristics.Any(c => c.Key == characteristic) ? characteristic! : Characteristics[0].Key;
        var progressSubject = Enum.Parse<Ks2ProgressSubject>(subjectKey, ignoreCase: true);

        var measure = BuildPlaceholderMeasure(isComparison, subjectKey, characteristicKey);
        var measureViewModel = isComparison
            ? MeasureViewModel.FromPrimaryComparisonMeasure(measure, CurrentSchool, SimilarSchool)
            : MeasureViewModel.FromPrimaryMeasure(measure, CurrentSchool);

        var schools = isComparison ? new[] { CurrentSchool, SimilarSchool } : new[] { CurrentSchool };
        var topPerformers = isComparison ? null : PlaceholderTopPerformers;

        var urns = schools.Select(s => s.Urn).Concat(topPerformers?.Select(t => t.Urn) ?? []);
        var numbers = await pupilNumbersProvider.GetAsync(urns, progressSubject, characteristicKey);

        measureViewModel = measureViewModel with
        {
            NumberOfPupils = MeasureNumberOfPupilsViewModel.Create(
                HtmlPrefix,
                LatestYear,
                schools.Select(s => (s.Name, numbers.GetValueOrDefault(s.Urn))).ToList(),
                topPerformers?
                    .OrderBy(t => t.Rank)
                    .Select(t => (t.Name, numbers.GetValueOrDefault(t.Urn)))
                    .ToList())
        };

        return View(new Ks2ProgressNumberOfPupilsExampleViewModel(isComparison, measureViewModel));
    }

    private static Measure BuildPlaceholderMeasure(bool isComparison, string subject, string characteristic)
    {
        IReadOnlyCollection<MeasureAvailableFilter> filters =
        [
            new(SubjectKey, "Subject", Subjects.Select(s => new FilterOption(s.Key, s.Name, s.Key == subject)).ToList()),
            new(CharacteristicKey, "Pupil characteristic", Characteristics.Select(c => new FilterOption(c.Key, c.Name, c.Key == characteristic)).ToList())
        ];

        IReadOnlyCollection<MeasureSeries> series = isComparison
            ?
            [
                new(MeasureSeriesType.CurrentSchool, 1.8m, 0.6m, -0.2m),
                new(MeasureSeriesType.ComparatorSchool, 0.9m, 1.2m, 0.4m),
                new(MeasureSeriesType.EnglandSchoolsAverage, 0m, 0m, 0m)
            ]
            :
            [
                new(MeasureSeriesType.CurrentSchool, 1.8m, 0.6m, -0.2m),
                new(MeasureSeriesType.SimilarSchoolsAverage, 0.7m, 0.5m, 0.3m),
                new(MeasureSeriesType.LASchoolsAverage, 0.2m, 0.1m, 0.4m),
                new(MeasureSeriesType.EnglandSchoolsAverage, 0m, 0m, 0m)
            ];

        return new Measure(
            HtmlPrefix,
            "Progress score (placeholder data)",
            LatestYear,
            MeasureDataType.Score,
            filters,
            series,
            isComparison ? null : PlaceholderTopPerformers);
    }

    private static SchoolInfo Placeholder(string urn, string name) =>
        new(urn, name, new LocalAuthority("000", "Example local authority"), new Address());
}

public record Ks2ProgressNumberOfPupilsExampleViewModel(bool IsComparison, MeasureViewModel Measure);
