using System.Globalization;
using SAPSec.Core.Features.Measures;
using SAPSec.Test.Common.Builders;
using static SAPSec.Core.Features.Measures.Measures.Secondary;
using KS4Performance = SAPSec.Data.Dto.KS4.Performance;

namespace SAPSec.Core.Tests.Features.Measures.Secondary;

public partial class GetComparisonKs4CoreSubjectsMeasuresUseCaseTests
{
    public static IEnumerable<object[]> CoreSubjectFilterCombinations()
    {
        (Type Definition, string ResponseProperty, string FieldPrefix)[] subjects =
        [
            (typeof(Ks4EnglishLanguage), "EnglishLanguage", "EngLang"),
            (typeof(Ks4EnglishLiterature), "EnglishLiterature", "EngLit"),
            (typeof(Ks4Maths), "Maths", "Maths"),
            (typeof(Ks4CombinedScience), "CombinedScience", "CombSci"),
            (typeof(Ks4Biology), "Biology", "Bio"),
            (typeof(Ks4Chemistry), "Chemistry", "Chem"),
            (typeof(Ks4Physics), "Physics", "Physics")
        ];

        // These expectations come from the data field naming convention, independently
        // of the production selectors.
        Dictionary<string, string> gradeCodes = new()
        {
            ["4"] = "49", ["5"] = "59", ["7"] = "79",
            ["44"] = "49", ["55"] = "59", ["77"] = "79"
        };
        Dictionary<string, string> characteristicCodes = new()
        {
            ["tot"] = "Sum", ["boy"] = "Boy", ["grl"] = "Grl",
            ["dis"] = "Dis", ["ndi"] = "NDi", ["eal"] = "EAL", ["nmo"] = "NMo"
        };

        foreach (var subject in subjects)
        {
            var filters = subject.Definition.GetNestedType("Filters")!;
            var gradeFilter = filters.GetNestedType("Grade")!;
            var characteristicFilter = filters.GetNestedType("PupilCharacteristic");
            var gradeKey = (string)gradeFilter.GetField("Key")!.GetValue(null)!;
            var characteristicKey = (string?)characteristicFilter?.GetField("Key")!.GetValue(null) ?? "";
            var grades = FilterValues(gradeFilter);
            var characteristics = characteristicFilter is null
                ? ["tot"]
                : FilterValues(characteristicFilter);

            foreach (var grade in grades)
            foreach (var characteristic in characteristics)
            foreach (var enabled in characteristicFilter is null ? new[] { true } : new[] { true, false })
            {
                var establishmentCode = enabled ? characteristicCodes[characteristic] : "Sum";
                var englandCode = establishmentCode switch
                {
                    "Sum" => "Tot",
                    "NMo" => "",
                    _ => establishmentCode
                };

                // The national data contains Physics breakdowns only for all pupils,
                // boys and girls; the remaining characteristics have no England values.
                if (subject.Definition == typeof(Ks4Physics)
                    && englandCode is "Dis" or "NDi" or "EAL")
                    englandCode = "";

                yield return
                [
                    subject.ResponseProperty, subject.FieldPrefix + gradeCodes[grade],
                    gradeKey, grade, characteristicKey, characteristic,
                    establishmentCode, englandCode, enabled
                ];
            }
        }
    }

    private static string[] FilterValues(Type filter) =>
        ((SAPSec.Core.Features.Measures.Measures.FilterValueDefinition[])filter.GetNestedType("Values")!
            .GetField("AllValues")!.GetValue(null)!)
        .Select(value => value.Value).ToArray();

    [Theory]
    [MemberData(nameof(CoreSubjectFilterCombinations))]
    public async Task CoreSubjects_EveryFilterCombination_SelectsExpectedFields(
        string responseProperty, string fieldPrefix,
        string gradeKey, string grade, string characteristicKey, string characteristic,
        string establishmentCode, string englandCode, bool characteristicFilterEnabled)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Secondary()),
            Build.Establishment("100002", "Comparator School", x => x.Secondary()));
        _similarSchoolsRepo.SetupGroups(Build.SecondaryGroup("100001", ["100002"]));
        _featureFlagService.Ks4CoreSubjectsPupilCharacteristicsEnabled = characteristicFilterEnabled;

        var current = new KS4Performance.EstablishmentPerformance { Id = "100001" };
        var comparator = new KS4Performance.EstablishmentPerformance { Id = "100002" };
        var england = new KS4Performance.EnglandPerformance { Id = "National" };
        var currentValues = PopulatePercentageFields(current, 10);
        var comparatorValues = PopulatePercentageFields(comparator, 40);
        var englandValues = PopulatePercentageFields(england, 70);
        _performanceRepo.SetupEstablishmentPerformance(current, comparator);
        _performanceRepo.SetupEnglandPerformance(england);

        Dictionary<string, string> filters = new() { [gradeKey] = grade };
        if (characteristicKey.Length > 0)
            filters[characteristicKey] = characteristic;

        var response = await _sut.Execute(Request("100001", "100002", filters));
        var measure = (Measure)response.GetType().GetProperty(responseProperty)!.GetValue(response)!;

        measure.Series.Should().Equal(
            ExpectedSeries(MeasureSeriesType.CurrentSchool, currentValues, fieldPrefix, establishmentCode, "Est"),
            ExpectedSeries(MeasureSeriesType.ComparatorSchool, comparatorValues, fieldPrefix, establishmentCode, "Est"),
            ExpectedSeries(MeasureSeriesType.EnglandSchoolsAverage, englandValues, fieldPrefix, englandCode, "Eng"));

        measure.Filters.Single(filter => filter.Key == gradeKey)
            .Options.Single(option => option.Selected).Key.Should().Be(grade);
        if (characteristicKey.Length > 0 && characteristicFilterEnabled)
            measure.Filters.Single(filter => filter.Key == characteristicKey)
                .Options.Single(option => option.Selected).Key.Should().Be(characteristic);
        else
            measure.Filters.Should().NotContain(filter => filter.Key == characteristicKey);
    }

    private static Dictionary<string, decimal> PopulatePercentageFields(object performance, int offset)
    {
        var values = new Dictionary<string, decimal>();
        var properties = performance.GetType().GetProperties()
            .Where(property => property.Name.EndsWith("_Pct", StringComparison.Ordinal))
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        // Give every field a different value so selecting another grade,
        // characteristic, subject, year or school cannot pass accidentally.
        for (var index = 0; index < properties.Length; index++)
        {
            var value = offset + (index + 1) / 1000m;
            properties[index].SetValue(performance, value.ToString(CultureInfo.InvariantCulture));
            values.Add(properties[index].Name, value);
        }
        return values;
    }

    private static MeasureSeries ExpectedSeries(
        MeasureSeriesType seriesType, Dictionary<string, decimal> values,
        string fieldPrefix, string characteristicCode, string scope)
    {
        if (characteristicCode.Length == 0)
            return new(seriesType, null, null, null);

        decimal Value(string year) => values[$"{fieldPrefix}_{characteristicCode}_{scope}_{year}_Pct"];
        return new(seriesType, Value("Current"), Value("Previous"), Value("Previous2"));
    }
}
