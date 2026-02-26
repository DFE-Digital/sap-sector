namespace SAPSec.Core.Features.SimilarSchools
{
    public class SimilarSchoolsSecondaryValuesRow : ISimilarSchoolsRow
    {
        public required string URN { get; set; }

        public required string Ks2Rp { get; set; }

        public required string Ks2Mp { get; set; }

        public required string PpPerc { get; set; }

        public required string PercentEal { get; set; }

        public required string Polar4QuintilePupils { get; set; }

        public required string PStability { get; set; }

        public required string IdaciPupils { get; set; }

        public required string PercentSchSupport { get; set; }

        public required string NumberOfPupils { get; set; }

        public required string PercentStatementOrEhp { get; set; }

        public required string Att8Scr { get; set; }
    }
}
