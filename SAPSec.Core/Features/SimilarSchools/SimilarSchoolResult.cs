using SAPSec.Core.Features.Geography;
using SAPSec.Core.Features.Sorting;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.SimilarSchools;

public record SimilarSchoolResult
(
    SimilarSchool SimilarSchool,
    GeographicCoordinates? Coordinates,
    SortOptionValue<DataWithAvailability<decimal>> SortValue
);
