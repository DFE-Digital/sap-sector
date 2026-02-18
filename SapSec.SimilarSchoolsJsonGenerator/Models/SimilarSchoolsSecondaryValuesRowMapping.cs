using CsvHelper.Configuration;
using SAPSec.Core.Features.SimilarSchools;

namespace SapSec.SimilarSchoolsJsonGenerator.Models
{
    public class SimilarSchoolsSecondaryValuesRowMapping : ClassMap<SimilarSchoolsSecondaryValuesRow>
    {
        public SimilarSchoolsSecondaryValuesRowMapping()
        {
            Map(m => m.URN).Name("urn");
            Map(m => m.KS2RP).Name("ks2_rp");
            Map(m => m.KS2MP).Name("ks2_mp");
            Map(m => m.PPPerc).Name("pp_perc");
            Map(m => m.PercentEAL).Name("percent_eal");
            Map(m => m.Polar4QuintilePupils).Name("polar4quintile_pupils");
            Map(m => m.PStability).Name("p_stability");
            Map(m => m.IdaciPupils).Name("idaci_pupils");
            Map(m => m.PercentSchSupport).Name("percent_sch_support");
            Map(m => m.NumberOfPupils).Name("number_of_pupils");
            Map(m => m.PercentageStatementOrEHP).Name("percent_statement_or_ehp");
            Map(m => m.Att8Scr).Name("att8scr");
        }
    }
}
