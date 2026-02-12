using SAPSec.Core.Features.Sorting;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.SimilarSchools.Sorting;

public class SimilarSchoolsSorting(string sortBy)
{
    public IEnumerable<SortedItem<SimilarSchool, DataWithAvailability<decimal>>> Sort(IEnumerable<SimilarSchool> items)
    {
        return sortBy switch
        {
            "EngMat" => Sort(items, "EngMat", "English and maths GCSEs (Grade 5 and above)",
                i => i.EnglishMathsGcseGrade5AndAbovePercentage),

            "EngLan" => Sort(items, "EngLan", "English language GCSE (Grade 5 and above)",
                i => i.EnglishLanguageGcseGrade5AndAbovePercentage),

            "EngLit" => Sort(items, "EngLit", "English literature GCSE (Grade 5 and above)",
                i => i.EnglishLiteratureGcseGrade5AndAbovePercentage),

            "Mat" => Sort(items, "Mat", "Mathematics GCSE (Grade 5 and above)",
                i => i.MathsGcseGrade5AndAbovePercentage),

            "CombSci" => Sort(items, "CombSci", "Combined science (double award) GCSE (Grade 5-5 and above)",
                i => i.CombinedSciencGcseGrade55AndAbovePercentage),

            "Bio" => Sort(items, "Bio", "Biology GCSE (Grade 5 and above)",
                i => i.BiologyGcseGrade5AndAbovePercentage),

            "Chem" => Sort(items, "Chem", "Chemistry GCSE (Grade 5 and above)",
                i => i.ChemistryGcseGrade5AndAbovePercentage),

            "Phys" => Sort(items, "Phys", "Physics GCSE (Grade 5 and above)",
                i => i.PhysicsGcseGrade5AndAbovePercentage),

            _ => Sort(items, "Att8", "Attainment 8",
                i => i.Attainment8Score)
        };
    }

    private IEnumerable<SortedItem<SimilarSchool, DataWithAvailability<decimal>>> Sort(IEnumerable<SimilarSchool> items, string sortKey, string sortName, Func<SimilarSchool, DataWithAvailability<decimal>> property) =>
        items
            .Select(item => new SortedItem<SimilarSchool, DataWithAvailability<decimal>>(
                item,
                new SortOptionValue<DataWithAvailability<decimal>>(sortKey, sortName, property(item))))
            .OrderByDescending(i => i.Value.Value, DataWithAvailability<decimal>.Comparer);

    public IEnumerable<SortOption> GetPossibleOptions(string sortBy)
    {
        yield return new("Att8", "Attainment 8", string.IsNullOrWhiteSpace(sortBy) || sortBy == "Att8");
        yield return new("EngMat", "English and maths GCSEs (Grade 5 and above)", sortBy == "EngMat");
        yield return new("EngLan", "English language GCSE (Grade 5 and above)", sortBy == "EngLan");
        yield return new("EngLit", "English literature GCSE (Grade 5 and above)", sortBy == "EngLit");
        yield return new("Mat", "Mathematics GCSE (Grade 5 and above)", sortBy == "Mat");
        yield return new("CombSci", "Combined science (double award) GCSE (Grade 5-5 and above)", sortBy == "CombSci");
        yield return new("Bio", "Biology GCSE (Grade 5 and above)", sortBy == "Bio");
        yield return new("Chem", "Chemistry GCSE (Grade 5 and above)", sortBy == "Chem");
        yield return new("Phys", "Physics GCSE (Grade 5 and above)", sortBy == "Phys");
    }
}