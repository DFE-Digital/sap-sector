using SAPSec.Core.Features.Availability;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public abstract record SimilarSchoolsAvailableFilter(
    string Key,
    string Name,
    DataWithAvailability<string>? CurrentSchoolValue);
