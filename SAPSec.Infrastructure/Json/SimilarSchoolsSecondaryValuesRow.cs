namespace SAPSec.Infrastructure.Json
{
    public class SimilarSchoolsSecondaryValuesRow : ISimilarSchoolsRow
    {
        public required string URN { get; set; }
        public required string KS2RP { get; set; }
        public required string KS2MP { get; set; }
        public required string PPPerc { get; set; }
        public required string PercentEAL { get; set; }
        public required string Polar4QuintilePupils { get; set; }
        public required string PStability { get; set; }
        public required string IdaciPupils { get; set; }
        public required string PercentSchSupport { get; set; }
        public required string NumberOfPupils { get; set; }
        public required string PercentageStatementOrEHP { get; set; }
        public required string Att8Scr { get; set; }
    }
}
