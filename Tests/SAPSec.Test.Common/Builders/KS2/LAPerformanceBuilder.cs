using SAPSec.Data.Dto.KS2.Performance;

namespace SAPSec.Test.Common.Builders.KS2;

public class LAPerformanceBuilder(string laId)
{
    string RwmExpected_Tot_Cohort_LA_Current_Num = string.Empty;
    string RwmExpected_Tot_Cohort_LA_Previous_Num = string.Empty;
    string RwmExpected_Tot_Cohort_LA_Previous2_Num = string.Empty;
    string RwmExpected_Reading_Tot_Cohort_LA_Current_Num = string.Empty;
    string RwmExpected_Reading_Tot_Cohort_LA_Previous_Num = string.Empty;
    string RwmExpected_Reading_Tot_Cohort_LA_Previous2_Num = string.Empty;
    string RwmExpected_Writing_Tot_Cohort_LA_Current_Num = string.Empty;
    string RwmExpected_Writing_Tot_Cohort_LA_Previous_Num = string.Empty;
    string RwmExpected_Writing_Tot_Cohort_LA_Previous2_Num = string.Empty;
    string RwmExpected_Maths_Tot_Cohort_LA_Current_Num = string.Empty;
    string RwmExpected_Maths_Tot_Cohort_LA_Previous_Num = string.Empty;
    string RwmExpected_Maths_Tot_Cohort_LA_Previous2_Num = string.Empty;
    string RwmHigher_Tot_Cohort_LA_Current_Num = string.Empty;
    string RwmHigher_Tot_Cohort_LA_Previous_Num = string.Empty;
    string RwmHigher_Tot_Cohort_LA_Previous2_Num = string.Empty;
    string RwmHigher_Reading_Tot_Cohort_LA_Current_Num = string.Empty;
    string RwmHigher_Reading_Tot_Cohort_LA_Previous_Num = string.Empty;
    string RwmHigher_Reading_Tot_Cohort_LA_Previous2_Num = string.Empty;
    string RwmHigher_Writing_Tot_Cohort_LA_Current_Num = string.Empty;
    string RwmHigher_Writing_Tot_Cohort_LA_Previous_Num = string.Empty;
    string RwmHigher_Writing_Tot_Cohort_LA_Previous2_Num = string.Empty;
    string RwmHigher_Maths_Tot_Cohort_LA_Current_Num = string.Empty;
    string RwmHigher_Maths_Tot_Cohort_LA_Previous_Num = string.Empty;
    string RwmHigher_Maths_Tot_Cohort_LA_Previous2_Num = string.Empty;
    string ReadingScaledScore_Tot_Cohort_LA_Current_Num = string.Empty;
    string ReadingScaledScore_Tot_Cohort_LA_Previous_Num = string.Empty;
    string ReadingScaledScore_Tot_Cohort_LA_Previous2_Num = string.Empty;
    string MathsScaledScore_Tot_Cohort_LA_Current_Num = string.Empty;
    string MathsScaledScore_Tot_Cohort_LA_Previous_Num = string.Empty;
    string MathsScaledScore_Tot_Cohort_LA_Previous2_Num = string.Empty;
    string GpsExpected_Tot_Cohort_LA_Current_Num = string.Empty;
    string GpsExpected_Tot_Cohort_LA_Previous_Num = string.Empty;
    string GpsExpected_Tot_Cohort_LA_Previous2_Num = string.Empty;
    string GpsHigher_Tot_Cohort_LA_Current_Num = string.Empty;
    string GpsHigher_Tot_Cohort_LA_Previous_Num = string.Empty;
    string GpsHigher_Tot_Cohort_LA_Previous2_Num = string.Empty;
    string RwmExpected_Boy_Cohort_LA_Current_Num = string.Empty;
    string RwmExpected_Boy_Cohort_LA_Previous_Num = string.Empty;
    string RwmExpected_Boy_Cohort_LA_Previous2_Num = string.Empty;
    string RwmExpected_Grl_Cohort_LA_Current_Num = string.Empty;
    string RwmExpected_Grl_Cohort_LA_Previous_Num = string.Empty;
    string RwmExpected_Grl_Cohort_LA_Previous2_Num = string.Empty;
    string RwmExpected_Dis_Cohort_LA_Current_Num = string.Empty;
    string RwmExpected_Dis_Cohort_LA_Previous_Num = string.Empty;
    string RwmExpected_Dis_Cohort_LA_Previous2_Num = string.Empty;
    string RwmExpected_NDi_Cohort_LA_Current_Num = string.Empty;
    string RwmExpected_NDi_Cohort_LA_Previous_Num = string.Empty;
    string RwmExpected_NDi_Cohort_LA_Previous2_Num = string.Empty;
    string RwmExpected_EAL_Cohort_LA_Current_Num = string.Empty;
    string RwmExpected_EAL_Cohort_LA_Previous_Num = string.Empty;
    string RwmExpected_EAL_Cohort_LA_Previous2_Num = string.Empty;
    string RwmExpected_Writing_Dis_Cohort_LA_Current_Num = string.Empty;
    string RwmExpected_Writing_Dis_Cohort_LA_Previous_Num = string.Empty;
    string RwmExpected_Writing_Dis_Cohort_LA_Previous2_Num = string.Empty;
    string RwmHigher_Boy_Cohort_LA_Current_Num = string.Empty;
    string RwmHigher_Boy_Cohort_LA_Previous_Num = string.Empty;
    string RwmHigher_Boy_Cohort_LA_Previous2_Num = string.Empty;
    string RwmHigher_Grl_Cohort_LA_Current_Num = string.Empty;
    string RwmHigher_Grl_Cohort_LA_Previous_Num = string.Empty;
    string RwmHigher_Grl_Cohort_LA_Previous2_Num = string.Empty;
    string RwmHigher_Dis_Cohort_LA_Current_Num = string.Empty;
    string RwmHigher_Dis_Cohort_LA_Previous_Num = string.Empty;
    string RwmHigher_Dis_Cohort_LA_Previous2_Num = string.Empty;
    string RwmHigher_NDi_Cohort_LA_Current_Num = string.Empty;
    string RwmHigher_NDi_Cohort_LA_Previous_Num = string.Empty;
    string RwmHigher_NDi_Cohort_LA_Previous2_Num = string.Empty;
    string RwmHigher_EAL_Cohort_LA_Current_Num = string.Empty;
    string RwmHigher_EAL_Cohort_LA_Previous_Num = string.Empty;
    string RwmHigher_EAL_Cohort_LA_Previous2_Num = string.Empty;
    string GpsExpected_Boy_Cohort_LA_Current_Num = string.Empty;
    string GpsExpected_Boy_Cohort_LA_Previous_Num = string.Empty;
    string GpsExpected_Boy_Cohort_LA_Previous2_Num = string.Empty;
    string GpsExpected_Grl_Cohort_LA_Current_Num = string.Empty;
    string GpsExpected_Grl_Cohort_LA_Previous_Num = string.Empty;
    string GpsExpected_Grl_Cohort_LA_Previous2_Num = string.Empty;
    string GpsExpected_Dis_Cohort_LA_Current_Num = string.Empty;
    string GpsExpected_Dis_Cohort_LA_Previous_Num = string.Empty;
    string GpsExpected_Dis_Cohort_LA_Previous2_Num = string.Empty;
    string GpsExpected_NDi_Cohort_LA_Current_Num = string.Empty;
    string GpsExpected_NDi_Cohort_LA_Previous_Num = string.Empty;
    string GpsExpected_NDi_Cohort_LA_Previous2_Num = string.Empty;
    string GpsExpected_EAL_Cohort_LA_Current_Num = string.Empty;
    string GpsExpected_EAL_Cohort_LA_Previous_Num = string.Empty;
    string GpsExpected_EAL_Cohort_LA_Previous2_Num = string.Empty;
    string GpsHigher_Boy_Cohort_LA_Current_Num = string.Empty;
    string GpsHigher_Boy_Cohort_LA_Previous_Num = string.Empty;
    string GpsHigher_Boy_Cohort_LA_Previous2_Num = string.Empty;
    string GpsHigher_Grl_Cohort_LA_Current_Num = string.Empty;
    string GpsHigher_Grl_Cohort_LA_Previous_Num = string.Empty;
    string GpsHigher_Grl_Cohort_LA_Previous2_Num = string.Empty;
    string GpsHigher_Dis_Cohort_LA_Current_Num = string.Empty;
    string GpsHigher_Dis_Cohort_LA_Previous_Num = string.Empty;
    string GpsHigher_Dis_Cohort_LA_Previous2_Num = string.Empty;
    string GpsHigher_NDi_Cohort_LA_Current_Num = string.Empty;
    string GpsHigher_NDi_Cohort_LA_Previous_Num = string.Empty;
    string GpsHigher_NDi_Cohort_LA_Previous2_Num = string.Empty;
    string GpsHigher_EAL_Cohort_LA_Current_Num = string.Empty;
    string GpsHigher_EAL_Cohort_LA_Previous_Num = string.Empty;
    string GpsHigher_EAL_Cohort_LA_Previous2_Num = string.Empty;
    string ReadingScaledScore_Boy_Cohort_LA_Current_Num = string.Empty;
    string ReadingScaledScore_Boy_Cohort_LA_Previous_Num = string.Empty;
    string ReadingScaledScore_Boy_Cohort_LA_Previous2_Num = string.Empty;
    string ReadingScaledScore_Grl_Cohort_LA_Current_Num = string.Empty;
    string ReadingScaledScore_Grl_Cohort_LA_Previous_Num = string.Empty;
    string ReadingScaledScore_Grl_Cohort_LA_Previous2_Num = string.Empty;
    string ReadingScaledScore_Dis_Cohort_LA_Current_Num = string.Empty;
    string ReadingScaledScore_Dis_Cohort_LA_Previous_Num = string.Empty;
    string ReadingScaledScore_Dis_Cohort_LA_Previous2_Num = string.Empty;
    string ReadingScaledScore_NDi_Cohort_LA_Current_Num = string.Empty;
    string ReadingScaledScore_NDi_Cohort_LA_Previous_Num = string.Empty;
    string ReadingScaledScore_NDi_Cohort_LA_Previous2_Num = string.Empty;
    string ReadingScaledScore_EAL_Cohort_LA_Current_Num = string.Empty;
    string ReadingScaledScore_EAL_Cohort_LA_Previous_Num = string.Empty;
    string ReadingScaledScore_EAL_Cohort_LA_Previous2_Num = string.Empty;
    string MathsScaledScore_Boy_Cohort_LA_Current_Num = string.Empty;
    string MathsScaledScore_Boy_Cohort_LA_Previous_Num = string.Empty;
    string MathsScaledScore_Boy_Cohort_LA_Previous2_Num = string.Empty;
    string MathsScaledScore_Grl_Cohort_LA_Current_Num = string.Empty;
    string MathsScaledScore_Grl_Cohort_LA_Previous_Num = string.Empty;
    string MathsScaledScore_Grl_Cohort_LA_Previous2_Num = string.Empty;
    string MathsScaledScore_Dis_Cohort_LA_Current_Num = string.Empty;
    string MathsScaledScore_Dis_Cohort_LA_Previous_Num = string.Empty;
    string MathsScaledScore_Dis_Cohort_LA_Previous2_Num = string.Empty;
    string MathsScaledScore_NDi_Cohort_LA_Current_Num = string.Empty;
    string MathsScaledScore_NDi_Cohort_LA_Previous_Num = string.Empty;
    string MathsScaledScore_NDi_Cohort_LA_Previous2_Num = string.Empty;
    string MathsScaledScore_EAL_Cohort_LA_Current_Num = string.Empty;
    string MathsScaledScore_EAL_Cohort_LA_Previous_Num = string.Empty;
    string MathsScaledScore_EAL_Cohort_LA_Previous2_Num = string.Empty;

    public LAPerformanceBuilder WithRwmExpected(string current, string prev, string prev2)
    {
        RwmExpected_Tot_Cohort_LA_Current_Num = current;
        RwmExpected_Tot_Cohort_LA_Previous_Num = prev;
        RwmExpected_Tot_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithRwmExpectedReading(string current, string prev, string prev2)
    {
        RwmExpected_Reading_Tot_Cohort_LA_Current_Num = current;
        RwmExpected_Reading_Tot_Cohort_LA_Previous_Num = prev;
        RwmExpected_Reading_Tot_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithRwmExpectedWriting(string current, string prev, string prev2)
    {
        RwmExpected_Writing_Tot_Cohort_LA_Current_Num = current;
        RwmExpected_Writing_Tot_Cohort_LA_Previous_Num = prev;
        RwmExpected_Writing_Tot_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithRwmExpectedMaths(string current, string prev, string prev2)
    {
        RwmExpected_Maths_Tot_Cohort_LA_Current_Num = current;
        RwmExpected_Maths_Tot_Cohort_LA_Previous_Num = prev;
        RwmExpected_Maths_Tot_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithRwmHigher(string current, string prev, string prev2)
    {
        RwmHigher_Tot_Cohort_LA_Current_Num = current;
        RwmHigher_Tot_Cohort_LA_Previous_Num = prev;
        RwmHigher_Tot_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithRwmHigherReading(string current, string prev, string prev2)
    {
        RwmHigher_Reading_Tot_Cohort_LA_Current_Num = current;
        RwmHigher_Reading_Tot_Cohort_LA_Previous_Num = prev;
        RwmHigher_Reading_Tot_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithRwmHigherWriting(string current, string prev, string prev2)
    {
        RwmHigher_Writing_Tot_Cohort_LA_Current_Num = current;
        RwmHigher_Writing_Tot_Cohort_LA_Previous_Num = prev;
        RwmHigher_Writing_Tot_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithRwmHigherMaths(string current, string prev, string prev2)
    {
        RwmHigher_Maths_Tot_Cohort_LA_Current_Num = current;
        RwmHigher_Maths_Tot_Cohort_LA_Previous_Num = prev;
        RwmHigher_Maths_Tot_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithReadingScaledScore(string current, string prev, string prev2)
    {
        ReadingScaledScore_Tot_Cohort_LA_Current_Num = current;
        ReadingScaledScore_Tot_Cohort_LA_Previous_Num = prev;
        ReadingScaledScore_Tot_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithMathsScaledScore(string current, string prev, string prev2)
    {
        MathsScaledScore_Tot_Cohort_LA_Current_Num = current;
        MathsScaledScore_Tot_Cohort_LA_Previous_Num = prev;
        MathsScaledScore_Tot_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithGpsExpected(string current, string prev, string prev2)
    {
        GpsExpected_Tot_Cohort_LA_Current_Num = current;
        GpsExpected_Tot_Cohort_LA_Previous_Num = prev;
        GpsExpected_Tot_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithGpsHigher(string current, string prev, string prev2)
    {
        GpsHigher_Tot_Cohort_LA_Current_Num = current;
        GpsHigher_Tot_Cohort_LA_Previous_Num = prev;
        GpsHigher_Tot_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithRwmExpectedBoys(string current, string prev, string prev2)
    {
        RwmExpected_Boy_Cohort_LA_Current_Num = current;
        RwmExpected_Boy_Cohort_LA_Previous_Num = prev;
        RwmExpected_Boy_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithRwmExpectedGirls(string current, string prev, string prev2)
    {
        RwmExpected_Grl_Cohort_LA_Current_Num = current;
        RwmExpected_Grl_Cohort_LA_Previous_Num = prev;
        RwmExpected_Grl_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithRwmExpectedDisadvantaged(string current, string prev, string prev2)
    {
        RwmExpected_Dis_Cohort_LA_Current_Num = current;
        RwmExpected_Dis_Cohort_LA_Previous_Num = prev;
        RwmExpected_Dis_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithRwmExpectedNonDisadvantaged(string current, string prev, string prev2)
    {
        RwmExpected_NDi_Cohort_LA_Current_Num = current;
        RwmExpected_NDi_Cohort_LA_Previous_Num = prev;
        RwmExpected_NDi_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithRwmExpectedEal(string current, string prev, string prev2)
    {
        RwmExpected_EAL_Cohort_LA_Current_Num = current;
        RwmExpected_EAL_Cohort_LA_Previous_Num = prev;
        RwmExpected_EAL_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithRwmExpectedWritingDisadvantaged(string current, string prev, string prev2)
    {
        RwmExpected_Writing_Dis_Cohort_LA_Current_Num = current;
        RwmExpected_Writing_Dis_Cohort_LA_Previous_Num = prev;
        RwmExpected_Writing_Dis_Cohort_LA_Previous2_Num = prev2;

        return this;
    }
    public LAPerformanceBuilder WithRwmHigherBoys(string current, string prev, string prev2)
    {
        RwmHigher_Boy_Cohort_LA_Current_Num = current;
        RwmHigher_Boy_Cohort_LA_Previous_Num = prev;
        RwmHigher_Boy_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithRwmHigherGirls(string current, string prev, string prev2)
    {
        RwmHigher_Grl_Cohort_LA_Current_Num = current;
        RwmHigher_Grl_Cohort_LA_Previous_Num = prev;
        RwmHigher_Grl_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithRwmHigherDisadvantaged(string current, string prev, string prev2)
    {
        RwmHigher_Dis_Cohort_LA_Current_Num = current;
        RwmHigher_Dis_Cohort_LA_Previous_Num = prev;
        RwmHigher_Dis_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithRwmHigherNonDisadvantaged(string current, string prev, string prev2)
    {
        RwmHigher_NDi_Cohort_LA_Current_Num = current;
        RwmHigher_NDi_Cohort_LA_Previous_Num = prev;
        RwmHigher_NDi_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithRwmHigherEal(string current, string prev, string prev2)
    {
        RwmHigher_EAL_Cohort_LA_Current_Num = current;
        RwmHigher_EAL_Cohort_LA_Previous_Num = prev;
        RwmHigher_EAL_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithGpsExpectedBoys(string current, string prev, string prev2)
    {
        GpsExpected_Boy_Cohort_LA_Current_Num = current;
        GpsExpected_Boy_Cohort_LA_Previous_Num = prev;
        GpsExpected_Boy_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithGpsExpectedGirls(string current, string prev, string prev2)
    {
        GpsExpected_Grl_Cohort_LA_Current_Num = current;
        GpsExpected_Grl_Cohort_LA_Previous_Num = prev;
        GpsExpected_Grl_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithGpsExpectedDisadvantaged(string current, string prev, string prev2)
    {
        GpsExpected_Dis_Cohort_LA_Current_Num = current;
        GpsExpected_Dis_Cohort_LA_Previous_Num = prev;
        GpsExpected_Dis_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithGpsExpectedNonDisadvantaged(string current, string prev, string prev2)
    {
        GpsExpected_NDi_Cohort_LA_Current_Num = current;
        GpsExpected_NDi_Cohort_LA_Previous_Num = prev;
        GpsExpected_NDi_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithGpsExpectedEal(string current, string prev, string prev2)
    {
        GpsExpected_EAL_Cohort_LA_Current_Num = current;
        GpsExpected_EAL_Cohort_LA_Previous_Num = prev;
        GpsExpected_EAL_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithGpsHigherBoys(string current, string prev, string prev2)
    {
        GpsHigher_Boy_Cohort_LA_Current_Num = current;
        GpsHigher_Boy_Cohort_LA_Previous_Num = prev;
        GpsHigher_Boy_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithGpsHigherGirls(string current, string prev, string prev2)
    {
        GpsHigher_Grl_Cohort_LA_Current_Num = current;
        GpsHigher_Grl_Cohort_LA_Previous_Num = prev;
        GpsHigher_Grl_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithGpsHigherDisadvantaged(string current, string prev, string prev2)
    {
        GpsHigher_Dis_Cohort_LA_Current_Num = current;
        GpsHigher_Dis_Cohort_LA_Previous_Num = prev;
        GpsHigher_Dis_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithGpsHigherNonDisadvantaged(string current, string prev, string prev2)
    {
        GpsHigher_NDi_Cohort_LA_Current_Num = current;
        GpsHigher_NDi_Cohort_LA_Previous_Num = prev;
        GpsHigher_NDi_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithGpsHigherEal(string current, string prev, string prev2)
    {
        GpsHigher_EAL_Cohort_LA_Current_Num = current;
        GpsHigher_EAL_Cohort_LA_Previous_Num = prev;
        GpsHigher_EAL_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithReadingScaledScoreBoys(string current, string prev, string prev2)
    {
        ReadingScaledScore_Boy_Cohort_LA_Current_Num = current;
        ReadingScaledScore_Boy_Cohort_LA_Previous_Num = prev;
        ReadingScaledScore_Boy_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithReadingScaledScoreGirls(string current, string prev, string prev2)
    {
        ReadingScaledScore_Grl_Cohort_LA_Current_Num = current;
        ReadingScaledScore_Grl_Cohort_LA_Previous_Num = prev;
        ReadingScaledScore_Grl_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithReadingScaledScoreDisadvantaged(string current, string prev, string prev2)
    {
        ReadingScaledScore_Dis_Cohort_LA_Current_Num = current;
        ReadingScaledScore_Dis_Cohort_LA_Previous_Num = prev;
        ReadingScaledScore_Dis_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithReadingScaledScoreNonDisadvantaged(string current, string prev, string prev2)
    {
        ReadingScaledScore_NDi_Cohort_LA_Current_Num = current;
        ReadingScaledScore_NDi_Cohort_LA_Previous_Num = prev;
        ReadingScaledScore_NDi_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithReadingScaledScoreEal(string current, string prev, string prev2)
    {
        ReadingScaledScore_EAL_Cohort_LA_Current_Num = current;
        ReadingScaledScore_EAL_Cohort_LA_Previous_Num = prev;
        ReadingScaledScore_EAL_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithMathsScaledScoreBoys(string current, string prev, string prev2)
    {
        MathsScaledScore_Boy_Cohort_LA_Current_Num = current;
        MathsScaledScore_Boy_Cohort_LA_Previous_Num = prev;
        MathsScaledScore_Boy_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithMathsScaledScoreGirls(string current, string prev, string prev2)
    {
        MathsScaledScore_Grl_Cohort_LA_Current_Num = current;
        MathsScaledScore_Grl_Cohort_LA_Previous_Num = prev;
        MathsScaledScore_Grl_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithMathsScaledScoreDisadvantaged(string current, string prev, string prev2)
    {
        MathsScaledScore_Dis_Cohort_LA_Current_Num = current;
        MathsScaledScore_Dis_Cohort_LA_Previous_Num = prev;
        MathsScaledScore_Dis_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithMathsScaledScoreNonDisadvantaged(string current, string prev, string prev2)
    {
        MathsScaledScore_NDi_Cohort_LA_Current_Num = current;
        MathsScaledScore_NDi_Cohort_LA_Previous_Num = prev;
        MathsScaledScore_NDi_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithMathsScaledScoreEal(string current, string prev, string prev2)
    {
        MathsScaledScore_EAL_Cohort_LA_Current_Num = current;
        MathsScaledScore_EAL_Cohort_LA_Previous_Num = prev;
        MathsScaledScore_EAL_Cohort_LA_Previous2_Num = prev2;

        return this;
    }
    public LAPerformance Build() =>
        new()
        {
            Id = laId,
            RwmExpected_Tot_Cohort_LA_Current_Num = RwmExpected_Tot_Cohort_LA_Current_Num,
            RwmExpected_Tot_Cohort_LA_Previous_Num = RwmExpected_Tot_Cohort_LA_Previous_Num,
            RwmExpected_Tot_Cohort_LA_Previous2_Num = RwmExpected_Tot_Cohort_LA_Previous2_Num,
            RwmExpected_Reading_Tot_Cohort_LA_Current_Num = RwmExpected_Reading_Tot_Cohort_LA_Current_Num,
            RwmExpected_Reading_Tot_Cohort_LA_Previous_Num = RwmExpected_Reading_Tot_Cohort_LA_Previous_Num,
            RwmExpected_Reading_Tot_Cohort_LA_Previous2_Num = RwmExpected_Reading_Tot_Cohort_LA_Previous2_Num,
            RwmExpected_Writing_Tot_Cohort_LA_Current_Num = RwmExpected_Writing_Tot_Cohort_LA_Current_Num,
            RwmExpected_Writing_Tot_Cohort_LA_Previous_Num = RwmExpected_Writing_Tot_Cohort_LA_Previous_Num,
            RwmExpected_Writing_Tot_Cohort_LA_Previous2_Num = RwmExpected_Writing_Tot_Cohort_LA_Previous2_Num,
            RwmExpected_Maths_Tot_Cohort_LA_Current_Num = RwmExpected_Maths_Tot_Cohort_LA_Current_Num,
            RwmExpected_Maths_Tot_Cohort_LA_Previous_Num = RwmExpected_Maths_Tot_Cohort_LA_Previous_Num,
            RwmExpected_Maths_Tot_Cohort_LA_Previous2_Num = RwmExpected_Maths_Tot_Cohort_LA_Previous2_Num,
            RwmHigher_Tot_Cohort_LA_Current_Num = RwmHigher_Tot_Cohort_LA_Current_Num,
            RwmHigher_Tot_Cohort_LA_Previous_Num = RwmHigher_Tot_Cohort_LA_Previous_Num,
            RwmHigher_Tot_Cohort_LA_Previous2_Num = RwmHigher_Tot_Cohort_LA_Previous2_Num,
            RwmHigher_Reading_Tot_Cohort_LA_Current_Num = RwmHigher_Reading_Tot_Cohort_LA_Current_Num,
            RwmHigher_Reading_Tot_Cohort_LA_Previous_Num = RwmHigher_Reading_Tot_Cohort_LA_Previous_Num,
            RwmHigher_Reading_Tot_Cohort_LA_Previous2_Num = RwmHigher_Reading_Tot_Cohort_LA_Previous2_Num,
            RwmHigher_Writing_Tot_Cohort_LA_Current_Num = RwmHigher_Writing_Tot_Cohort_LA_Current_Num,
            RwmHigher_Writing_Tot_Cohort_LA_Previous_Num = RwmHigher_Writing_Tot_Cohort_LA_Previous_Num,
            RwmHigher_Writing_Tot_Cohort_LA_Previous2_Num = RwmHigher_Writing_Tot_Cohort_LA_Previous2_Num,
            RwmHigher_Maths_Tot_Cohort_LA_Current_Num = RwmHigher_Maths_Tot_Cohort_LA_Current_Num,
            RwmHigher_Maths_Tot_Cohort_LA_Previous_Num = RwmHigher_Maths_Tot_Cohort_LA_Previous_Num,
            RwmHigher_Maths_Tot_Cohort_LA_Previous2_Num = RwmHigher_Maths_Tot_Cohort_LA_Previous2_Num,
            ReadingScaledScore_Tot_Cohort_LA_Current_Num = ReadingScaledScore_Tot_Cohort_LA_Current_Num,
            ReadingScaledScore_Tot_Cohort_LA_Previous_Num = ReadingScaledScore_Tot_Cohort_LA_Previous_Num,
            ReadingScaledScore_Tot_Cohort_LA_Previous2_Num = ReadingScaledScore_Tot_Cohort_LA_Previous2_Num,
            MathsScaledScore_Tot_Cohort_LA_Current_Num = MathsScaledScore_Tot_Cohort_LA_Current_Num,
            MathsScaledScore_Tot_Cohort_LA_Previous_Num = MathsScaledScore_Tot_Cohort_LA_Previous_Num,
            MathsScaledScore_Tot_Cohort_LA_Previous2_Num = MathsScaledScore_Tot_Cohort_LA_Previous2_Num,
            GpsExpected_Tot_Cohort_LA_Current_Num = GpsExpected_Tot_Cohort_LA_Current_Num,
            GpsExpected_Tot_Cohort_LA_Previous_Num = GpsExpected_Tot_Cohort_LA_Previous_Num,
            GpsExpected_Tot_Cohort_LA_Previous2_Num = GpsExpected_Tot_Cohort_LA_Previous2_Num,
            GpsHigher_Tot_Cohort_LA_Current_Num = GpsHigher_Tot_Cohort_LA_Current_Num,
            GpsHigher_Tot_Cohort_LA_Previous_Num = GpsHigher_Tot_Cohort_LA_Previous_Num,
            GpsHigher_Tot_Cohort_LA_Previous2_Num = GpsHigher_Tot_Cohort_LA_Previous2_Num,
            RwmExpected_Boy_Cohort_LA_Current_Num = RwmExpected_Boy_Cohort_LA_Current_Num,
            RwmExpected_Boy_Cohort_LA_Previous_Num = RwmExpected_Boy_Cohort_LA_Previous_Num,
            RwmExpected_Boy_Cohort_LA_Previous2_Num = RwmExpected_Boy_Cohort_LA_Previous2_Num,
            RwmExpected_Grl_Cohort_LA_Current_Num = RwmExpected_Grl_Cohort_LA_Current_Num,
            RwmExpected_Grl_Cohort_LA_Previous_Num = RwmExpected_Grl_Cohort_LA_Previous_Num,
            RwmExpected_Grl_Cohort_LA_Previous2_Num = RwmExpected_Grl_Cohort_LA_Previous2_Num,
            RwmExpected_Dis_Cohort_LA_Current_Num = RwmExpected_Dis_Cohort_LA_Current_Num,
            RwmExpected_Dis_Cohort_LA_Previous_Num = RwmExpected_Dis_Cohort_LA_Previous_Num,
            RwmExpected_Dis_Cohort_LA_Previous2_Num = RwmExpected_Dis_Cohort_LA_Previous2_Num,
            RwmExpected_NDi_Cohort_LA_Current_Num = RwmExpected_NDi_Cohort_LA_Current_Num,
            RwmExpected_NDi_Cohort_LA_Previous_Num = RwmExpected_NDi_Cohort_LA_Previous_Num,
            RwmExpected_NDi_Cohort_LA_Previous2_Num = RwmExpected_NDi_Cohort_LA_Previous2_Num,
            RwmExpected_EAL_Cohort_LA_Current_Num = RwmExpected_EAL_Cohort_LA_Current_Num,
            RwmExpected_EAL_Cohort_LA_Previous_Num = RwmExpected_EAL_Cohort_LA_Previous_Num,
            RwmExpected_EAL_Cohort_LA_Previous2_Num = RwmExpected_EAL_Cohort_LA_Previous2_Num,
            RwmExpected_Writing_Dis_Cohort_LA_Current_Num = RwmExpected_Writing_Dis_Cohort_LA_Current_Num,
            RwmExpected_Writing_Dis_Cohort_LA_Previous_Num = RwmExpected_Writing_Dis_Cohort_LA_Previous_Num,
            RwmExpected_Writing_Dis_Cohort_LA_Previous2_Num = RwmExpected_Writing_Dis_Cohort_LA_Previous2_Num,
            RwmHigher_Boy_Cohort_LA_Current_Num = RwmHigher_Boy_Cohort_LA_Current_Num,
            RwmHigher_Boy_Cohort_LA_Previous_Num = RwmHigher_Boy_Cohort_LA_Previous_Num,
            RwmHigher_Boy_Cohort_LA_Previous2_Num = RwmHigher_Boy_Cohort_LA_Previous2_Num,
            RwmHigher_Grl_Cohort_LA_Current_Num = RwmHigher_Grl_Cohort_LA_Current_Num,
            RwmHigher_Grl_Cohort_LA_Previous_Num = RwmHigher_Grl_Cohort_LA_Previous_Num,
            RwmHigher_Grl_Cohort_LA_Previous2_Num = RwmHigher_Grl_Cohort_LA_Previous2_Num,
            RwmHigher_Dis_Cohort_LA_Current_Num = RwmHigher_Dis_Cohort_LA_Current_Num,
            RwmHigher_Dis_Cohort_LA_Previous_Num = RwmHigher_Dis_Cohort_LA_Previous_Num,
            RwmHigher_Dis_Cohort_LA_Previous2_Num = RwmHigher_Dis_Cohort_LA_Previous2_Num,
            RwmHigher_NDi_Cohort_LA_Current_Num = RwmHigher_NDi_Cohort_LA_Current_Num,
            RwmHigher_NDi_Cohort_LA_Previous_Num = RwmHigher_NDi_Cohort_LA_Previous_Num,
            RwmHigher_NDi_Cohort_LA_Previous2_Num = RwmHigher_NDi_Cohort_LA_Previous2_Num,
            RwmHigher_EAL_Cohort_LA_Current_Num = RwmHigher_EAL_Cohort_LA_Current_Num,
            RwmHigher_EAL_Cohort_LA_Previous_Num = RwmHigher_EAL_Cohort_LA_Previous_Num,
            RwmHigher_EAL_Cohort_LA_Previous2_Num = RwmHigher_EAL_Cohort_LA_Previous2_Num,
            GpsExpected_Boy_Cohort_LA_Current_Num = GpsExpected_Boy_Cohort_LA_Current_Num,
            GpsExpected_Boy_Cohort_LA_Previous_Num = GpsExpected_Boy_Cohort_LA_Previous_Num,
            GpsExpected_Boy_Cohort_LA_Previous2_Num = GpsExpected_Boy_Cohort_LA_Previous2_Num,
            GpsExpected_Grl_Cohort_LA_Current_Num = GpsExpected_Grl_Cohort_LA_Current_Num,
            GpsExpected_Grl_Cohort_LA_Previous_Num = GpsExpected_Grl_Cohort_LA_Previous_Num,
            GpsExpected_Grl_Cohort_LA_Previous2_Num = GpsExpected_Grl_Cohort_LA_Previous2_Num,
            GpsExpected_Dis_Cohort_LA_Current_Num = GpsExpected_Dis_Cohort_LA_Current_Num,
            GpsExpected_Dis_Cohort_LA_Previous_Num = GpsExpected_Dis_Cohort_LA_Previous_Num,
            GpsExpected_Dis_Cohort_LA_Previous2_Num = GpsExpected_Dis_Cohort_LA_Previous2_Num,
            GpsExpected_NDi_Cohort_LA_Current_Num = GpsExpected_NDi_Cohort_LA_Current_Num,
            GpsExpected_NDi_Cohort_LA_Previous_Num = GpsExpected_NDi_Cohort_LA_Previous_Num,
            GpsExpected_NDi_Cohort_LA_Previous2_Num = GpsExpected_NDi_Cohort_LA_Previous2_Num,
            GpsExpected_EAL_Cohort_LA_Current_Num = GpsExpected_EAL_Cohort_LA_Current_Num,
            GpsExpected_EAL_Cohort_LA_Previous_Num = GpsExpected_EAL_Cohort_LA_Previous_Num,
            GpsExpected_EAL_Cohort_LA_Previous2_Num = GpsExpected_EAL_Cohort_LA_Previous2_Num,
            GpsHigher_Boy_Cohort_LA_Current_Num = GpsHigher_Boy_Cohort_LA_Current_Num,
            GpsHigher_Boy_Cohort_LA_Previous_Num = GpsHigher_Boy_Cohort_LA_Previous_Num,
            GpsHigher_Boy_Cohort_LA_Previous2_Num = GpsHigher_Boy_Cohort_LA_Previous2_Num,
            GpsHigher_Grl_Cohort_LA_Current_Num = GpsHigher_Grl_Cohort_LA_Current_Num,
            GpsHigher_Grl_Cohort_LA_Previous_Num = GpsHigher_Grl_Cohort_LA_Previous_Num,
            GpsHigher_Grl_Cohort_LA_Previous2_Num = GpsHigher_Grl_Cohort_LA_Previous2_Num,
            GpsHigher_Dis_Cohort_LA_Current_Num = GpsHigher_Dis_Cohort_LA_Current_Num,
            GpsHigher_Dis_Cohort_LA_Previous_Num = GpsHigher_Dis_Cohort_LA_Previous_Num,
            GpsHigher_Dis_Cohort_LA_Previous2_Num = GpsHigher_Dis_Cohort_LA_Previous2_Num,
            GpsHigher_NDi_Cohort_LA_Current_Num = GpsHigher_NDi_Cohort_LA_Current_Num,
            GpsHigher_NDi_Cohort_LA_Previous_Num = GpsHigher_NDi_Cohort_LA_Previous_Num,
            GpsHigher_NDi_Cohort_LA_Previous2_Num = GpsHigher_NDi_Cohort_LA_Previous2_Num,
            GpsHigher_EAL_Cohort_LA_Current_Num = GpsHigher_EAL_Cohort_LA_Current_Num,
            GpsHigher_EAL_Cohort_LA_Previous_Num = GpsHigher_EAL_Cohort_LA_Previous_Num,
            GpsHigher_EAL_Cohort_LA_Previous2_Num = GpsHigher_EAL_Cohort_LA_Previous2_Num,
            ReadingScaledScore_Boy_Cohort_LA_Current_Num = ReadingScaledScore_Boy_Cohort_LA_Current_Num,
            ReadingScaledScore_Boy_Cohort_LA_Previous_Num = ReadingScaledScore_Boy_Cohort_LA_Previous_Num,
            ReadingScaledScore_Boy_Cohort_LA_Previous2_Num = ReadingScaledScore_Boy_Cohort_LA_Previous2_Num,
            ReadingScaledScore_Grl_Cohort_LA_Current_Num = ReadingScaledScore_Grl_Cohort_LA_Current_Num,
            ReadingScaledScore_Grl_Cohort_LA_Previous_Num = ReadingScaledScore_Grl_Cohort_LA_Previous_Num,
            ReadingScaledScore_Grl_Cohort_LA_Previous2_Num = ReadingScaledScore_Grl_Cohort_LA_Previous2_Num,
            ReadingScaledScore_Dis_Cohort_LA_Current_Num = ReadingScaledScore_Dis_Cohort_LA_Current_Num,
            ReadingScaledScore_Dis_Cohort_LA_Previous_Num = ReadingScaledScore_Dis_Cohort_LA_Previous_Num,
            ReadingScaledScore_Dis_Cohort_LA_Previous2_Num = ReadingScaledScore_Dis_Cohort_LA_Previous2_Num,
            ReadingScaledScore_NDi_Cohort_LA_Current_Num = ReadingScaledScore_NDi_Cohort_LA_Current_Num,
            ReadingScaledScore_NDi_Cohort_LA_Previous_Num = ReadingScaledScore_NDi_Cohort_LA_Previous_Num,
            ReadingScaledScore_NDi_Cohort_LA_Previous2_Num = ReadingScaledScore_NDi_Cohort_LA_Previous2_Num,
            ReadingScaledScore_EAL_Cohort_LA_Current_Num = ReadingScaledScore_EAL_Cohort_LA_Current_Num,
            ReadingScaledScore_EAL_Cohort_LA_Previous_Num = ReadingScaledScore_EAL_Cohort_LA_Previous_Num,
            ReadingScaledScore_EAL_Cohort_LA_Previous2_Num = ReadingScaledScore_EAL_Cohort_LA_Previous2_Num,
            MathsScaledScore_Boy_Cohort_LA_Current_Num = MathsScaledScore_Boy_Cohort_LA_Current_Num,
            MathsScaledScore_Boy_Cohort_LA_Previous_Num = MathsScaledScore_Boy_Cohort_LA_Previous_Num,
            MathsScaledScore_Boy_Cohort_LA_Previous2_Num = MathsScaledScore_Boy_Cohort_LA_Previous2_Num,
            MathsScaledScore_Grl_Cohort_LA_Current_Num = MathsScaledScore_Grl_Cohort_LA_Current_Num,
            MathsScaledScore_Grl_Cohort_LA_Previous_Num = MathsScaledScore_Grl_Cohort_LA_Previous_Num,
            MathsScaledScore_Grl_Cohort_LA_Previous2_Num = MathsScaledScore_Grl_Cohort_LA_Previous2_Num,
            MathsScaledScore_Dis_Cohort_LA_Current_Num = MathsScaledScore_Dis_Cohort_LA_Current_Num,
            MathsScaledScore_Dis_Cohort_LA_Previous_Num = MathsScaledScore_Dis_Cohort_LA_Previous_Num,
            MathsScaledScore_Dis_Cohort_LA_Previous2_Num = MathsScaledScore_Dis_Cohort_LA_Previous2_Num,
            MathsScaledScore_NDi_Cohort_LA_Current_Num = MathsScaledScore_NDi_Cohort_LA_Current_Num,
            MathsScaledScore_NDi_Cohort_LA_Previous_Num = MathsScaledScore_NDi_Cohort_LA_Previous_Num,
            MathsScaledScore_NDi_Cohort_LA_Previous2_Num = MathsScaledScore_NDi_Cohort_LA_Previous2_Num,
            MathsScaledScore_EAL_Cohort_LA_Current_Num = MathsScaledScore_EAL_Cohort_LA_Current_Num,
            MathsScaledScore_EAL_Cohort_LA_Previous_Num = MathsScaledScore_EAL_Cohort_LA_Previous_Num,
            MathsScaledScore_EAL_Cohort_LA_Previous2_Num = MathsScaledScore_EAL_Cohort_LA_Previous2_Num,
        };
}
