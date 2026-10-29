using SAPSec.Core.Features.SimilarSchools;

public record SimilarSchoolsFilterGroupViewModel(
    string Heading,
    List<SimilarSchoolsAvailableFilter> Filters);