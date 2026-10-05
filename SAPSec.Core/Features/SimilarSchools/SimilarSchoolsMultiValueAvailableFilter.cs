using SAPSec.Core.Features.Availability;
using SAPSec.Core.Filtering;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public record SimilarSchoolsMultiValueAvailableFilter(
    string Key,
    string Name,
    IReadOnlyCollection<FilterOption> Options,
    DataWithAvailability<string>? CurrentSchoolValue)
    : SimilarSchoolsAvailableFilter(Key, Name, CurrentSchoolValue);
