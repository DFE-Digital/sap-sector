namespace SAPSec.Core.Features.SimilarSchools.Sorting;

public record SimilarSchoolSortItem<TSortData>(SimilarSchool SimilarSchool, TSortData? SortData)
    where TSortData : class;