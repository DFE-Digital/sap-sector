namespace SAPSec.Core.Features.SimilarSchools
{
    public interface ISimilarSchoolsSecondaryRepository
    {
        Task<IReadOnlyCollection<string>> GetSimilarSchoolUrnsAsync(string urn);
        Task<(SimilarSchool, IReadOnlyCollection<SimilarSchool>)> GetSimilarSchoolsGroupAsync(string urn);
        Task<IReadOnlyCollection<SimilarSchoolsSecondaryValues>> GetSecondaryValuesByUrnsAsync(
            IEnumerable<string> urns);
    }
}
