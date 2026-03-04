using SAPSec.Core.Features.SimilarSchools;

namespace SAPSec.Core.Tests.Features.SimilarSchools;

public class SimilarSchoolsSecondaryValuesTests
{
    [Fact]
    public void Ks2AverageScore_ReturnsMeanOfReadingAndMaths()
    {
        var values = new SimilarSchoolsSecondaryValues
        {
            Urn = "123456",
            Ks2ReadingScore = 104.5m,
            Ks2MathsScore = 105.5m
        };

        Assert.Equal(105.0m, values.Ks2AverageScore);
    }
}
