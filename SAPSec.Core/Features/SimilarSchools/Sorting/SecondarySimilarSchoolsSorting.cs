using SAPSec.Core.Features.Availability;
using SAPSec.Core.Features.Sorting;
using SAPSec.Data.Dto.KS4.Performance;

namespace SAPSec.Core.Features.SimilarSchools.Sorting;

public class SecondarySimilarSchoolsSorting(string sortBy)
{
    private const string PercentFormat = "0\\%";
    private const int PercentDecimalPlaces = 0;

    private const string ScaledScoreFormat = "0.0";
    private const int ScaledScoreDecimalPlaces = 1;

    public IEnumerable<SortedItem<SimilarSchool, DataWithAvailability<string>>> Sort(
        IEnumerable<SimilarSchoolSortItem<EstablishmentPerformance>> items)
    {
        return sortBy.ToLowerInvariant() switch
        {
            "engmat" => Sort(
                items,
                "EngMat",
                "English and maths GCSEs (Grade 5 and above)",
                i => DataWithAvailability.FromDecimalString(i?.EngMaths59_Tot_Est_Current_Pct),
                PercentFormat,
                PercentDecimalPlaces),

            "englang" => Sort(
                items,
                "EngLang",
                "English language GCSE (Grade 5 and above)",
                i => DataWithAvailability.FromDecimalString(i?.EngLang59_Sum_Est_Current_Pct),
                PercentFormat,
                PercentDecimalPlaces),

            "englit" => Sort(
                items,
                "EngLit",
                "English literature GCSE (Grade 5 and above)",
                i => DataWithAvailability.FromDecimalString(i?.EngLit59_Sum_Est_Current_Pct),
                PercentFormat,
                PercentDecimalPlaces),

            "maths" => Sort(
                items,
                "Maths",
                "Mathematics GCSE (Grade 5 and above)",
                i => DataWithAvailability.FromDecimalString(i?.Maths59_Sum_Est_Current_Pct),
                PercentFormat,
                PercentDecimalPlaces),

            "combsci" => Sort(
                items,
                "CombSci",
                "Combined science (double award) GCSE (Grade 5-5 and above)",
                i => DataWithAvailability.FromDecimalString(i?.CombSci59_Sum_Est_Current_Pct),
                PercentFormat,
                PercentDecimalPlaces),

            "bio" => Sort(
                items,
                "Bio",
                "Biology GCSE (Grade 5 and above)",
                i => DataWithAvailability.FromDecimalString(i?.Bio59_Sum_Est_Current_Pct),
                PercentFormat,
                PercentDecimalPlaces),

            "chem" => Sort(
                items,
                "Chem",
                "Chemistry GCSE (Grade 5 and above)",
                i => DataWithAvailability.FromDecimalString(i?.Chem59_Sum_Est_Current_Pct),
                PercentFormat,
                PercentDecimalPlaces),

            "phys" => Sort(
                items,
                "Phys",
                "Physics GCSE (Grade 5 and above)",
                i => DataWithAvailability.FromDecimalString(i?.Physics59_Sum_Est_Current_Pct),
                PercentFormat,
                PercentDecimalPlaces),

            _ => Sort(
                items,
                "Att8",
                "Attainment 8",
                i => DataWithAvailability.FromDecimalString(i?.Attainment8_Tot_Est_Current_Num),
                ScaledScoreFormat,
                ScaledScoreDecimalPlaces)
        };
    }

    private IEnumerable<SortedItem<SimilarSchool, DataWithAvailability<string>>> Sort(
        IEnumerable<SimilarSchoolSortItem<EstablishmentPerformance>> items,
        string sortKey,
        string sortName,
        Func<EstablishmentPerformance?, DataWithAvailability<decimal>> property,
        string displayFormat,
        int decimalPlaces) =>
        SimilarSchoolsSortEngine.Sort(
            items,
            sortKey,
            sortName,
            property,
            displayFormat,
            decimalPlaces);

    public IEnumerable<SortOption> GetPossibleOptions(string sortBy)
    {
        var normalized = sortBy?.ToLowerInvariant() ?? string.Empty;

        var att8Selected = !new[] {
            "engmat",
            "englang",
            "englit",
            "maths",
            "combsci",
            "bio",
            "chem",
            "phys",
        }.Contains(normalized);

        yield return new("Att8", "Attainment 8", att8Selected);
        yield return new("EngMat", "English and maths GCSEs (Grade 5 and above)", normalized == "engmat");
        yield return new("EngLang", "English language GCSE (Grade 5 and above)", normalized == "englang");
        yield return new("EngLit", "English literature GCSE (Grade 5 and above)", normalized == "englit");
        yield return new("Maths", "Mathematics GCSE (Grade 5 and above)", normalized == "maths");
        yield return new("CombSci", "Combined science (double award) GCSE (Grade 5-5 and above)", normalized == "combsci");
        yield return new("Bio", "Biology GCSE (Grade 5 and above)", normalized == "bio");
        yield return new("Chem", "Chemistry GCSE (Grade 5 and above)", normalized == "chem");
        yield return new("Phys", "Physics GCSE (Grade 5 and above)", normalized == "phys");
    }
}