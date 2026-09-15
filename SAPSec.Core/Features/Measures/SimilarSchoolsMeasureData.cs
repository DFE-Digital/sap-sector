using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures;

public record SimilarSchoolsMeasureData<T>(
    SchoolMeasureData<T> CurrentSchool,
    IReadOnlyCollection<SchoolMeasureData<T>> SimilarSchools)
    where T : class, IMeasureData;

