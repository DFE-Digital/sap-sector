using CsvHelper.Configuration;

namespace SapSec.SimilarSchoolsJsonGenerator.Models
{
    public class SimilarSchoolsSecondaryGroupsRow : ISimilarSchoolsGroupsRow
    {
        public required string Urn { get; set; }
        public required string NeighbourUrn { get; set; }
        public required string Distance { get; set; }
        public required string Rank { get; set; }
    }

    public class SimilarSchoolsSecondaryGroupsRowMapping : ClassMap<SimilarSchoolsSecondaryGroupsRow>
    {
        public SimilarSchoolsSecondaryGroupsRowMapping()
        {
            Map(m => m.Urn).Name("urn");
            Map(m => m.NeighbourUrn).Name("neighbour_urn");
            Map(m => m.Distance).Name("dist");
            Map(m => m.Rank).Name("rank");
        }
    }
}
