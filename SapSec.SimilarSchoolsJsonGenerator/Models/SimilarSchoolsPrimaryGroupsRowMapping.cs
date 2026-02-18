using CsvHelper.Configuration;
using SAPSec.Infrastructure.Repositories.Json;

namespace SapSec.SimilarSchoolsJsonGenerator.Models
{
    public class SimilarSchoolsPrimaryGroupsRowMapping : ClassMap<SimilarSchoolsPrimaryGroupsRow>
    {
        public SimilarSchoolsPrimaryGroupsRowMapping()
        {
            Map(m => m.URN).Name("urn");
            Map(m => m.NeighbourURN).Name("neighbour_urn");
            Map(m => m.Dist).Name("dist");
            Map(m => m.Rank).Name("rank");
        }
    }
}
