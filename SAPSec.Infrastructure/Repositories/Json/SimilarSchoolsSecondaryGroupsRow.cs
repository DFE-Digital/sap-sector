namespace SAPSec.Infrastructure.Repositories.Json
{
    public class SimilarSchoolsSecondaryGroupsRow : ISimilarSchoolsGroupsRow
    {
        public required string URN { get; set; }
        public required string NeighbourURN { get; set; }
        public required string Dist { get; set; }
        public required string Rank { get; set; }
    }
}
