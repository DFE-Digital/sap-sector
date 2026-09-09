using SAPSec.Core.Features.Availability;
using SAPSec.Core.Features.SimilarSchools.UseCases;

namespace SAPSec.Core.Features.SimilarSchools;

public record SimilarSchoolsNumericRangeAvailableFilter(
    string Key,
    string Name,
    SimilarSchoolsNumericRangeAvailableFilterField From,
    SimilarSchoolsNumericRangeAvailableFilterField To,
    DataWithAvailability<string>? CurrentSchoolValue,
    IReadOnlyCollection<ValidationError> ValidationErrors)
    : SimilarSchoolsAvailableFilter(Key, Name, CurrentSchoolValue);

public record SimilarSchoolsNumericRangeAvailableFilterField(string Key, string Value);