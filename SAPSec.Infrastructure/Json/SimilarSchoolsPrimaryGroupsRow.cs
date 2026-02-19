namespace SAPSec.Infrastructure.Json
{
    public class SimilarSchoolsPrimaryGroupsRow : ISimilarSchoolsGroupsRow
    {
        public required string URN { get; set; }
        public required string NeighbourURN { get; set; }
        public required string Dist { get; set; }
        public required string Rank { get; set; }
    }
}
