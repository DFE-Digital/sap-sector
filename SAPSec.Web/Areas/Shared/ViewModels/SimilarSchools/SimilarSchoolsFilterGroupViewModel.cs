using SAPSec.Core.Features.SimilarSchools.UseCases;

public record SimilarSchoolsFilterGroupViewModel(
    string Heading,
    List<SimilarSchoolsAvailableFilter> Filters);