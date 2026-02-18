namespace SAPSec.Infrastructure.Repositories.Json
{
    public class SimilarSchoolsPrimaryValuesRow : ISimilarSchoolsRow
    {
        public required string URN { get; set; }
        public required string PPPerc { get; set; }
        public required string Polar4QuintilePupils { get; set; }
        public required string PStability { get; set; }
        public required string PercentSchSupport { get; set; }
        public required string PercentEAL { get; set; }
        public required string IdaciPupils { get; set; }
        public required string PercentageStatementOrEhp { get; set; }
        public required string NumberOfPupils { get; set; }
        public required string ReadMatAverage { get; set; }
    }
}
