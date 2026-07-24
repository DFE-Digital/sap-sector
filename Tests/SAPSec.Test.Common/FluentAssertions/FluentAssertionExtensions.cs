using FluentAssertions;
using FluentAssertions.Collections;

namespace SAPSec.Test.Common.FluentAssertions;

public static class FluentAssertionExtensions
{
    public static void AllBeDifferent(this GenericCollectionAssertions<IEnumerable<string>> assertions)
    {
        var values = assertions.Subject.ToList();

        for (var i = 0; i < values.Count; i++)
        {
            for (var j = 0; j < values.Count; j++)
            {
                if (i == j)
                {
                    continue;
                }

                values[i].Should().NotEqual(values[j]);
            }
        }
    }
}
