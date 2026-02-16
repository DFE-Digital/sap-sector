using CsvHelper.Configuration;

namespace SapSec.SimilarSchoolsJsonGenerator.Models
{
    public class SimilarSchoolsSecondaryValuesRow : ISimilarSchoolsRow
    {
        public required string Urn { get; set; }
        public required string AverageKS2ReadingScore { get; set; }
        public required string AverageKS2MathsScore { get; set; }
        public required string PupilPremiumPercentage { get; set; }
        public required string PercentagePupilsWithEal { get; set; }
        public required string AveragePolar4Quintile { get; set; }
        public required string PupilStabilityRate { get; set; }
        public required string AverageIdaciScore { get; set; }
        public required string PercentagePupilsWithSen { get; set; }
        public required string NumberOfPupils { get; set; }
        public required string PercentagePupilsWithEhc { get; set; }
        public required string Attainment8Score { get; set; }
    }

    public class SimilarSchoolsSecondaryValuesRowMapping : ClassMap<SimilarSchoolsSecondaryValuesRow>
    {
        public SimilarSchoolsSecondaryValuesRowMapping()
        {
            Map(m => m.Urn).Name("urn");
            Map(m => m.AverageKS2ReadingScore).Name("ks2_rp");
            Map(m => m.AverageKS2MathsScore).Name("ks2_mp");
            Map(m => m.PupilPremiumPercentage).Name("pp_perc");
            Map(m => m.PercentagePupilsWithEal).Name("percent_eal");
            Map(m => m.AveragePolar4Quintile).Name("polar4quintile_pupils");
            Map(m => m.PupilStabilityRate).Name("p_stability");
            Map(m => m.AverageIdaciScore).Name("idaci_pupils");
            Map(m => m.PercentagePupilsWithSen).Name("percent_sch_support");
            Map(m => m.NumberOfPupils).Name("number_of_pupils");
            Map(m => m.PercentagePupilsWithEhc).Name("percent_statement_or_ehp");
            Map(m => m.Attainment8Score).Name("att8scr");
        }
    }
}
