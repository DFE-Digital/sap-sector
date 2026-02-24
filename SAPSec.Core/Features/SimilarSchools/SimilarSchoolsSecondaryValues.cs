namespace SAPSec.Core.Features.SimilarSchools;

public class SimilarSchoolsSecondaryValues
{
    public string Urn { get; init; } = default!;

    public decimal Ks2Rp { get; init; }
    public decimal Ks2Mp { get; init; }

    public decimal PpPerc { get; init; }
    public decimal PercentEal { get; init; }

    public int Polar4QuintilePupils { get; init; }

    public decimal PStability { get; init; }
    public decimal IdaciPupils { get; init; }
    public decimal PercentSchSupport { get; init; }

    public int NumberOfPupils { get; init; }
    public decimal PercentStatementOrEhp { get; init; }
}