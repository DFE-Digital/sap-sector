using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures;

public record ComparisonMeasureData<T>(
    SchoolMeasureData<T> CurrentSchool,
    SchoolMeasureData<T> SimilarSchool)
    where T : class, IMeasureData;

