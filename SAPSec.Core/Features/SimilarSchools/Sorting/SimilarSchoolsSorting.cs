using SAPSec.Core.Features.Sorting;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.SimilarSchools.Sorting;

public class SimilarSchoolsSorting(string sortBy)
{
    public IEnumerable<SortedItem<SimilarSchool, DataWithAvailability<string>>> Sort(IEnumerable<SimilarSchool> items)
    {
        return sortBy.ToLowerInvariant() switch
        {
            "engmat" => Sort(items, "EngMat", "English and maths GCSEs (Grade 5 and above)",
                i => i.EnglishMathsGcseGrade5AndAbovePercentage),

            "englang" => Sort(items, "EngLang", "English language GCSE (Grade 5 and above)",
                i => i.EnglishLanguageGcseGrade5AndAbovePercentage),

            "englit" => Sort(items, "EngLit", "English literature GCSE (Grade 5 and above)",
                i => i.EnglishLiteratureGcseGrade5AndAbovePercentage),

            "maths" => Sort(items, "Maths", "Mathematics GCSE (Grade 5 and above)",
                i => i.MathsGcseGrade5AndAbovePercentage),

            "combsci" => Sort(items, "CombSci", "Combined science (double award) GCSE (Grade 5-5 and above)",
                i => i.CombinedScienceGcseGrade55AndAbovePercentage),

            "bio" => Sort(items, "Bio", "Biology GCSE (Grade 5 and above)",
                i => i.BiologyGcseGrade5AndAbovePercentage),

            "chem" => Sort(items, "Chem", "Chemistry GCSE (Grade 5 and above)",
                i => i.ChemistryGcseGrade5AndAbovePercentage),

            "phys" => Sort(items, "Phys", "Physics GCSE (Grade 5 and above)",
                i => i.PhysicsGcseGrade5AndAbovePercentage),

            _ => Sort(items, "Att8", "Attainment 8",
                i => i.Attainment8Score)
        };
    }

    private IEnumerable<SortedItem<SimilarSchool, DataWithAvailability<string>>> Sort(IEnumerable<SimilarSchool> items, string sortKey, string sortName, Func<SimilarSchool, DataWithAvailability<decimal>> property) =>
        items
            .Select(item => new SortedItem<SimilarSchool, DataWithAvailability<decimal>>(
                item,
                new SortOptionValue<DataWithAvailability<decimal>>(sortKey, sortName, property(item))))
            .OrderByDescending(i => i.Value.Value, DataWithAvailability<decimal>.Comparer)
            .Select(item => new SortedItem<SimilarSchool, DataWithAvailability<string>>(
                item.Item,
                new SortOptionValue<DataWithAvailability<string>>(item.Value.Key, item.Value.Name, sortKey.ToLowerInvariant() switch
                {
                    "att8" => item.Value.Value.Map(v => v.ToString("0.0")),
                    _ => item.Value.Value.Map(v => v.ToString("0.0\\%"))
                })));

    public IEnumerable<SortOption> GetPossibleOptions(string sortBy)
    {
        var att8Selected = !new[] {
            "engmat",
            "englang",
            "englit",
            "maths",
            "combsci",
            "bio",
            "chem",
            "phys",
        }.Contains(sortBy.ToLowerInvariant());

        yield return new("Att8", "Attainment 8", att8Selected);
        yield return new("EngMat", "English and maths GCSEs (Grade 5 and above)", sortBy.ToLowerInvariant() == "engmat");
        yield return new("EngLang", "English language GCSE (Grade 5 and above)", sortBy.ToLowerInvariant() == "englang");
        yield return new("EngLit", "English literature GCSE (Grade 5 and above)", sortBy.ToLowerInvariant() == "englit");
        yield return new("Maths", "Mathematics GCSE (Grade 5 and above)", sortBy.ToLowerInvariant() == "maths");
        yield return new("CombSci", "Combined science (double award) GCSE (Grade 5-5 and above)", sortBy.ToLowerInvariant() == "combsci");
        yield return new("Bio", "Biology GCSE (Grade 5 and above)", sortBy.ToLowerInvariant() == "bio");
        yield return new("Chem", "Chemistry GCSE (Grade 5 and above)", sortBy.ToLowerInvariant() == "chem");
        yield return new("Phys", "Physics GCSE (Grade 5 and above)", sortBy.ToLowerInvariant() == "phys");
    }
}