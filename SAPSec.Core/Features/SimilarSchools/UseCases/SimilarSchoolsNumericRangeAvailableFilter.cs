using SAPSec.Core.Features.Availability;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public record SimilarSchoolsNumericRangeAvailableFilter(
    string Key,
    string Name,
    SimilarSchoolsNumericRangeAvailableFilterField From,
    SimilarSchoolsNumericRangeAvailableFilterField To,
    DataWithAvailability<string>? CurrentSchoolValue,
    IReadOnlyCollection<ValidationError> ValidationErrors)
    : SimilarSchoolsAvailableFilter(Key, Name, CurrentSchoolValue);
