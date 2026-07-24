using SAPSec.Data.Dto.KS2.Performance;

namespace SAPSec.Test.Common.Builders;

public class EnglandPerformanceBuilder()
{
    string RwmExpected_Tot_Cohort_Eng_Current_Num = string.Empty;
    string RwmExpected_Tot_Cohort_Eng_Previous_Num = string.Empty;
    string RwmExpected_Tot_Cohort_Eng_Previous2_Num = string.Empty;
    string RwmExpected_Reading_Tot_Cohort_Eng_Current_Num = string.Empty;
    string RwmExpected_Reading_Tot_Cohort_Eng_Previous_Num = string.Empty;
    string RwmExpected_Reading_Tot_Cohort_Eng_Previous2_Num = string.Empty;
    string RwmExpected_Writing_Tot_Cohort_Eng_Current_Num = string.Empty;
    string RwmExpected_Writing_Tot_Cohort_Eng_Previous_Num = string.Empty;
    string RwmExpected_Writing_Tot_Cohort_Eng_Previous2_Num = string.Empty;
    string RwmExpected_Maths_Tot_Cohort_Eng_Current_Num = string.Empty;
    string RwmExpected_Maths_Tot_Cohort_Eng_Previous_Num = string.Empty;
    string RwmExpected_Maths_Tot_Cohort_Eng_Previous2_Num = string.Empty;
    string ReadingScaledScore_Tot_Cohort_Eng_Current_Num = string.Empty;
    string ReadingScaledScore_Tot_Cohort_Eng_Previous_Num = string.Empty;
    string ReadingScaledScore_Tot_Cohort_Eng_Previous2_Num = string.Empty;
    string GpsExpected_Tot_Cohort_Eng_Current_Num = string.Empty;
    string GpsExpected_Tot_Cohort_Eng_Previous_Num = string.Empty;
    string GpsExpected_Tot_Cohort_Eng_Previous2_Num = string.Empty;
    string GpsHigher_Tot_Cohort_Eng_Current_Num = string.Empty;
    string GpsHigher_Tot_Cohort_Eng_Previous_Num = string.Empty;
    string GpsHigher_Tot_Cohort_Eng_Previous2_Num = string.Empty;

    public EnglandPerformanceBuilder WithRwmExpected(string current, string prev, string prev2)
    {
        RwmExpected_Tot_Cohort_Eng_Current_Num = current;
        RwmExpected_Tot_Cohort_Eng_Previous_Num = prev;
        RwmExpected_Tot_Cohort_Eng_Previous2_Num = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithReadingScaledScore(string current, string prev, string prev2)
    {
        ReadingScaledScore_Tot_Cohort_Eng_Current_Num = current;
        ReadingScaledScore_Tot_Cohort_Eng_Previous_Num = prev;
        ReadingScaledScore_Tot_Cohort_Eng_Previous2_Num = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithRwmExpectedReading(string current, string prev, string prev2)
    {
        RwmExpected_Reading_Tot_Cohort_Eng_Current_Num = current;
        RwmExpected_Reading_Tot_Cohort_Eng_Previous_Num = prev;
        RwmExpected_Reading_Tot_Cohort_Eng_Previous2_Num = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithRwmExpectedWriting(string current, string prev, string prev2)
    {
        RwmExpected_Writing_Tot_Cohort_Eng_Current_Num = current;
        RwmExpected_Writing_Tot_Cohort_Eng_Previous_Num = prev;
        RwmExpected_Writing_Tot_Cohort_Eng_Previous2_Num = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithRwmExpectedMaths(string current, string prev, string prev2)
    {
        RwmExpected_Maths_Tot_Cohort_Eng_Current_Num = current;
        RwmExpected_Maths_Tot_Cohort_Eng_Previous_Num = prev;
        RwmExpected_Maths_Tot_Cohort_Eng_Previous2_Num = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithGpsExpected(string current, string prev, string prev2)
    {
        GpsExpected_Tot_Cohort_Eng_Current_Num = current;
        GpsExpected_Tot_Cohort_Eng_Previous_Num = prev;
        GpsExpected_Tot_Cohort_Eng_Previous2_Num = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithGpsHigher(string current, string prev, string prev2)
    {
        GpsHigher_Tot_Cohort_Eng_Current_Num = current;
        GpsHigher_Tot_Cohort_Eng_Previous_Num = prev;
        GpsHigher_Tot_Cohort_Eng_Previous2_Num = prev2;

        return this;
    }

    public EnglandPerformance Build() =>
        new()
        {
            Id = "National",
            RwmExpected_Tot_Cohort_Eng_Current_Num = RwmExpected_Tot_Cohort_Eng_Current_Num,
            RwmExpected_Tot_Cohort_Eng_Previous_Num = RwmExpected_Tot_Cohort_Eng_Previous_Num,
            RwmExpected_Tot_Cohort_Eng_Previous2_Num = RwmExpected_Tot_Cohort_Eng_Previous2_Num,
            RwmExpected_Reading_Tot_Cohort_Eng_Current_Num = RwmExpected_Reading_Tot_Cohort_Eng_Current_Num,
            RwmExpected_Reading_Tot_Cohort_Eng_Previous_Num = RwmExpected_Reading_Tot_Cohort_Eng_Previous_Num,
            RwmExpected_Reading_Tot_Cohort_Eng_Previous2_Num = RwmExpected_Reading_Tot_Cohort_Eng_Previous2_Num,
            RwmExpected_Writing_Tot_Cohort_Eng_Current_Num = RwmExpected_Writing_Tot_Cohort_Eng_Current_Num,
            RwmExpected_Writing_Tot_Cohort_Eng_Previous_Num = RwmExpected_Writing_Tot_Cohort_Eng_Previous_Num,
            RwmExpected_Writing_Tot_Cohort_Eng_Previous2_Num = RwmExpected_Writing_Tot_Cohort_Eng_Previous2_Num,
            RwmExpected_Maths_Tot_Cohort_Eng_Current_Num = RwmExpected_Maths_Tot_Cohort_Eng_Current_Num,
            RwmExpected_Maths_Tot_Cohort_Eng_Previous_Num = RwmExpected_Maths_Tot_Cohort_Eng_Previous_Num,
            RwmExpected_Maths_Tot_Cohort_Eng_Previous2_Num = RwmExpected_Maths_Tot_Cohort_Eng_Previous2_Num,
            ReadingScaledScore_Tot_Cohort_Eng_Current_Num = ReadingScaledScore_Tot_Cohort_Eng_Current_Num,
            ReadingScaledScore_Tot_Cohort_Eng_Previous_Num = ReadingScaledScore_Tot_Cohort_Eng_Previous_Num,
            ReadingScaledScore_Tot_Cohort_Eng_Previous2_Num = ReadingScaledScore_Tot_Cohort_Eng_Previous2_Num,
            GpsExpected_Tot_Cohort_Eng_Current_Num = GpsExpected_Tot_Cohort_Eng_Current_Num,
            GpsExpected_Tot_Cohort_Eng_Previous_Num = GpsExpected_Tot_Cohort_Eng_Previous_Num,
            GpsExpected_Tot_Cohort_Eng_Previous2_Num = GpsExpected_Tot_Cohort_Eng_Previous2_Num,
            GpsHigher_Tot_Cohort_Eng_Current_Num = GpsHigher_Tot_Cohort_Eng_Current_Num,
            GpsHigher_Tot_Cohort_Eng_Previous_Num = GpsHigher_Tot_Cohort_Eng_Previous_Num,
            GpsHigher_Tot_Cohort_Eng_Previous2_Num = GpsHigher_Tot_Cohort_Eng_Previous2_Num,
        };
}
