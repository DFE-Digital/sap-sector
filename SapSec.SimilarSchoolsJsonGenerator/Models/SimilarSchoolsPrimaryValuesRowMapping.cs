using CsvHelper.Configuration;
using SAPSec.Core.Features.SimilarSchools;

namespace SapSec.SimilarSchoolsJsonGenerator.Models
{
    public class SimilarSchoolsPrimaryValuesRowMapping : ClassMap<SimilarSchoolsPrimaryValuesRow>
    {
        public SimilarSchoolsPrimaryValuesRowMapping()
        {
            Map(m => m.URN).Name("urn");
            Map(m => m.PPPerc).Name("pp_perc");
            Map(m => m.Polar4QuintilePupils).Name("polar4quintile_pupils");
            Map(m => m.PStability).Name("p_stability");
            Map(m => m.PercentSchSupport).Name("percent_sch_support");
            Map(m => m.PercentEAL).Name("percent_eal");
            Map(m => m.IdaciPupils).Name("idaci_pupils");
            Map(m => m.PercentageStatementOrEhp).Name("percent_statement_or_ehp");
            Map(m => m.NumberOfPupils).Name("number_of_pupils");
            Map(m => m.ReadMatAverage).Name("readmat_average");
        }
    }
}
