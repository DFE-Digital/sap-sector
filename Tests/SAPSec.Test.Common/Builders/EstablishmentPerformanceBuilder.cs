using SAPSec.Data.Dto.KS2.Performance;

namespace SAPSec.Test.Common.Builders;

public class EstablishmentPerformanceBuilder(string urn)
{
    string RwmExpected_Tot_Cohort_Est_Current_Num = string.Empty;
    string RwmExpected_Tot_Cohort_Est_Previous_Num = string.Empty;
    string RwmExpected_Tot_Cohort_Est_Previous2_Num = string.Empty;
    string RwmExpected_Reading_Tot_Cohort_Est_Current_Num = string.Empty;
    string RwmExpected_Reading_Tot_Cohort_Est_Previous_Num = string.Empty;
    string RwmExpected_Reading_Tot_Cohort_Est_Previous2_Num = string.Empty;
    string RwmExpected_Writing_Tot_Cohort_Est_Current_Num = string.Empty;
    string RwmExpected_Writing_Tot_Cohort_Est_Previous_Num = string.Empty;
    string RwmExpected_Writing_Tot_Cohort_Est_Previous2_Num = string.Empty;
    string RwmExpected_Maths_Tot_Cohort_Est_Current_Num = string.Empty;
    string RwmExpected_Maths_Tot_Cohort_Est_Previous_Num = string.Empty;
    string RwmExpected_Maths_Tot_Cohort_Est_Previous2_Num = string.Empty;
    string RwmHigher_Tot_Cohort_Est_Current_Num = string.Empty;
    string RwmHigher_Tot_Cohort_Est_Previous_Num = string.Empty;
    string RwmHigher_Tot_Cohort_Est_Previous2_Num = string.Empty;
    string ReadingScaledScore_Tot_Cohort_Est_Current_Num = string.Empty;
    string ReadingScaledScore_Tot_Cohort_Est_Previous_Num = string.Empty;
    string ReadingScaledScore_Tot_Cohort_Est_Previous2_Num = string.Empty;
    string MathsScaledScore_Tot_Cohort_Est_Current_Num = string.Empty;
    string MathsScaledScore_Tot_Cohort_Est_Previous_Num = string.Empty;
    string MathsScaledScore_Tot_Cohort_Est_Previous2_Num = string.Empty;
    string GpsExpected_Tot_Cohort_Est_Current_Num = string.Empty;
    string GpsExpected_Tot_Cohort_Est_Previous_Num = string.Empty;
    string GpsExpected_Tot_Cohort_Est_Previous2_Num = string.Empty;
    string GpsHigher_Tot_Cohort_Est_Current_Num = string.Empty;
    string GpsHigher_Tot_Cohort_Est_Previous_Num = string.Empty;
    string GpsHigher_Tot_Cohort_Est_Previous2_Num = string.Empty;
    //string RwmExpected_Reading_Tot_Cohort_Est_Current_Num = string.Empty;
    //string RwmExpected_Reading_Tot_Cohort_Est_Previous_Num = string.Empty;
    //string RwmExpected_Reading_Tot_Cohort_Est_Previous2_Num = string.Empty;
    //string RwmExpected_Writing_Tot_Cohort_Est_Current_Num = string.Empty;
    //string RwmExpected_Writing_Tot_Cohort_Est_Previous_Num = string.Empty;
    //string RwmExpected_Writing_Tot_Cohort_Est_Previous2_Num = string.Empty;
    //string RwmExpected_Maths_Tot_Cohort_Est_Current_Num = string.Empty;
    //string RwmExpected_Maths_Tot_Cohort_Est_Previous_Num = string.Empty;
    //string RwmExpected_Maths_Tot_Cohort_Est_Previous2_Num = string.Empty;

    public EstablishmentPerformanceBuilder WithRwmExpected(string current, string prev, string prev2)
    {
        RwmExpected_Tot_Cohort_Est_Current_Num = current;
        RwmExpected_Tot_Cohort_Est_Previous_Num = prev;
        RwmExpected_Tot_Cohort_Est_Previous2_Num = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithRwmExpectedReading(string current, string prev, string prev2)
    {
        RwmExpected_Reading_Tot_Cohort_Est_Current_Num = current;
        RwmExpected_Reading_Tot_Cohort_Est_Previous_Num = prev;
        RwmExpected_Reading_Tot_Cohort_Est_Previous2_Num = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithRwmExpectedWriting(string current, string prev, string prev2)
    {
        RwmExpected_Writing_Tot_Cohort_Est_Current_Num = current;
        RwmExpected_Writing_Tot_Cohort_Est_Previous_Num = prev;
        RwmExpected_Writing_Tot_Cohort_Est_Previous2_Num = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithRwmExpectedMaths(string current, string prev, string prev2)
    {
        RwmExpected_Maths_Tot_Cohort_Est_Current_Num = current;
        RwmExpected_Maths_Tot_Cohort_Est_Previous_Num = prev;
        RwmExpected_Maths_Tot_Cohort_Est_Previous2_Num = prev2;

        return this;
    }
    
    public EstablishmentPerformanceBuilder WithRwmHigher(string current, string prev, string prev2)
    {
        RwmHigher_Tot_Cohort_Est_Current_Num = current;
        RwmHigher_Tot_Cohort_Est_Previous_Num = prev;
        RwmHigher_Tot_Cohort_Est_Previous2_Num = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithReadingScaledScore(string current, string prev, string prev2)
    {
        ReadingScaledScore_Tot_Cohort_Est_Current_Num = current;
        ReadingScaledScore_Tot_Cohort_Est_Previous_Num = prev;
        ReadingScaledScore_Tot_Cohort_Est_Previous2_Num = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithMathsScaledScore(string current, string prev, string prev2)
    {
        MathsScaledScore_Tot_Cohort_Est_Current_Num = current;
        MathsScaledScore_Tot_Cohort_Est_Previous_Num = prev;
        MathsScaledScore_Tot_Cohort_Est_Previous2_Num = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithGpsExpected(string current, string prev, string prev2)
    {
        GpsExpected_Tot_Cohort_Est_Current_Num = current;
        GpsExpected_Tot_Cohort_Est_Previous_Num = prev;
        GpsExpected_Tot_Cohort_Est_Previous2_Num = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithGpsHigher(string current, string prev, string prev2)
    {
        GpsHigher_Tot_Cohort_Est_Current_Num = current;
        GpsHigher_Tot_Cohort_Est_Previous_Num = prev;
        GpsHigher_Tot_Cohort_Est_Previous2_Num = prev2;

        return this;
    }

    public EstablishmentPerformance Build() =>
        new EstablishmentPerformance()
        {
            Id = urn,
            RwmExpected_Tot_Cohort_Est_Current_Num = RwmExpected_Tot_Cohort_Est_Current_Num,
            RwmExpected_Tot_Cohort_Est_Previous_Num = RwmExpected_Tot_Cohort_Est_Previous_Num,
            RwmExpected_Tot_Cohort_Est_Previous2_Num = RwmExpected_Tot_Cohort_Est_Previous2_Num,
            RwmExpected_Reading_Tot_Cohort_Est_Current_Num = RwmExpected_Reading_Tot_Cohort_Est_Current_Num,
            RwmExpected_Reading_Tot_Cohort_Est_Previous_Num = RwmExpected_Reading_Tot_Cohort_Est_Previous_Num,
            RwmExpected_Reading_Tot_Cohort_Est_Previous2_Num = RwmExpected_Reading_Tot_Cohort_Est_Previous2_Num,
            RwmExpected_Writing_Tot_Cohort_Est_Current_Num = RwmExpected_Writing_Tot_Cohort_Est_Current_Num,
            RwmExpected_Writing_Tot_Cohort_Est_Previous_Num = RwmExpected_Writing_Tot_Cohort_Est_Previous_Num,
            RwmExpected_Writing_Tot_Cohort_Est_Previous2_Num = RwmExpected_Writing_Tot_Cohort_Est_Previous2_Num,
            RwmExpected_Maths_Tot_Cohort_Est_Current_Num = RwmExpected_Maths_Tot_Cohort_Est_Current_Num,
            RwmExpected_Maths_Tot_Cohort_Est_Previous_Num = RwmExpected_Maths_Tot_Cohort_Est_Previous_Num,
            RwmExpected_Maths_Tot_Cohort_Est_Previous2_Num = RwmExpected_Maths_Tot_Cohort_Est_Previous2_Num,
            RwmHigher_Tot_Cohort_Est_Current_Num = RwmHigher_Tot_Cohort_Est_Current_Num,
            RwmHigher_Tot_Cohort_Est_Previous_Num = RwmHigher_Tot_Cohort_Est_Previous_Num,
            RwmHigher_Tot_Cohort_Est_Previous2_Num = RwmHigher_Tot_Cohort_Est_Previous2_Num,
            ReadingScaledScore_Tot_Cohort_Est_Current_Num = ReadingScaledScore_Tot_Cohort_Est_Current_Num,
            ReadingScaledScore_Tot_Cohort_Est_Previous_Num = ReadingScaledScore_Tot_Cohort_Est_Previous_Num,
            ReadingScaledScore_Tot_Cohort_Est_Previous2_Num = ReadingScaledScore_Tot_Cohort_Est_Previous2_Num,
            MathsScaledScore_Tot_Cohort_Est_Current_Num = MathsScaledScore_Tot_Cohort_Est_Current_Num,
            MathsScaledScore_Tot_Cohort_Est_Previous_Num = MathsScaledScore_Tot_Cohort_Est_Previous_Num,
            MathsScaledScore_Tot_Cohort_Est_Previous2_Num = MathsScaledScore_Tot_Cohort_Est_Previous2_Num,
            GpsExpected_Tot_Cohort_Est_Current_Num = GpsExpected_Tot_Cohort_Est_Current_Num,
            GpsExpected_Tot_Cohort_Est_Previous_Num = GpsExpected_Tot_Cohort_Est_Previous_Num,
            GpsExpected_Tot_Cohort_Est_Previous2_Num = GpsExpected_Tot_Cohort_Est_Previous2_Num,
            GpsHigher_Tot_Cohort_Est_Current_Num = GpsHigher_Tot_Cohort_Est_Current_Num,
            GpsHigher_Tot_Cohort_Est_Previous_Num = GpsHigher_Tot_Cohort_Est_Previous_Num,
            GpsHigher_Tot_Cohort_Est_Previous2_Num = GpsHigher_Tot_Cohort_Est_Previous2_Num,
        };
}
