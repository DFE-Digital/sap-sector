namespace SAPSec.Core.Features.SimilarSchools
{
    public class SimilarSchoolsSecondaryGroupsRow : ISimilarSchoolsGroupsRow
    {
        public required string URN { get; set; }
        public required string NeighbourURN { get; set; }
        public required string Dist { get; set; }
        public required string Rank { get; set; }
    }
}
