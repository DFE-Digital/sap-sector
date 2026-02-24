using System.Text.Json.Serialization;

namespace SAPSec.Core.Features.SimilarSchools
{
    public class SimilarSchoolsSecondaryValuesRow : ISimilarSchoolsRow
    {
        [JsonPropertyName("URN")]
        public required string URN { get; set; }

        [JsonPropertyName("KS2RP")]
        public required string KS2RP { get; set; }

        // JSON key is "kS2MP" (not "KS2MP")
        [JsonPropertyName("kS2MP")]
        public required string KS2MP { get; set; }

        [JsonPropertyName("PPPerc")]
        public required string PPPerc { get; set; }

        [JsonPropertyName("PercentEAL")]
        public required string PercentEAL { get; set; }

        [JsonPropertyName("Polar4QuintilePupils")]
        public required string Polar4QuintilePupils { get; set; }

        [JsonPropertyName("PStability")]
        public required string PStability { get; set; }

        [JsonPropertyName("IdaciPupils")]
        public required string IdaciPupils { get; set; }

        [JsonPropertyName("PercentSchSupport")]
        public required string PercentSchSupport { get; set; }

        [JsonPropertyName("NumberOfPupils")]
        public required string NumberOfPupils { get; set; }

        // JSON key is "PercentStatementOrEHP"
        [JsonPropertyName("PercentStatementOrEHP")]
        public required string PercentageStatementOrEHP { get; set; }

        [JsonPropertyName("Att8Scr")]
        public required string Att8Scr { get; set; }
    }
}
