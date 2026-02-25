using SAPSec.Core.Features.Filtering;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public interface ISimilarSchoolsFilter
{
    string Name { get; }
    FilterType Type { get; }
}
