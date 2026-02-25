namespace SAPSec.Core.Features.SimilarSchools
{
    public class SimilarSchoolsPrimaryGroupsRow : ISimilarSchoolsGroupsRow
    {
        public required string URN { get; set; }
        public required string SimilarURN { get; set; }
        public required string Dist { get; set; }
        public required string Rank { get; set; }
    }
}
