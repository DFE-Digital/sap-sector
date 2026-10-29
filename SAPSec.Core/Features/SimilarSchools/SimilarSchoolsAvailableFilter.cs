using SAPSec.Core.Features.Availability;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using System.Text.Json.Serialization;

namespace SAPSec.Core.Features.SimilarSchools;

[JsonDerivedType(typeof(SimilarSchoolsSingleValueAvailableFilter), "single")]
[JsonDerivedType(typeof(SimilarSchoolsMultiValueAvailableFilter), "multi")]
[JsonDerivedType(typeof(SimilarSchoolsNumericRangeAvailableFilter), "range")]
public abstract record SimilarSchoolsAvailableFilter(
    string Key,
    string Name,
    DataWithAvailability<string>? CurrentSchoolValue);
