using CsvHelper.Configuration;
using SAPSec.Core.Features.SimilarSchools;

namespace SapSec.SimilarSchoolsJsonGenerator.Models
{
    public class SimilarSchoolsSecondaryGroupsRowMapping : ClassMap<SimilarSchoolsSecondaryGroupsRow>
    {
        public SimilarSchoolsSecondaryGroupsRowMapping()
        {
            Map(m => m.URN).Name("urn");
            Map(m => m.NeighbourURN).Name("neighbour_urn");
            Map(m => m.Dist).Name("dist");
            Map(m => m.Rank).Name("rank");
        }
    }
}
