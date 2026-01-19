using SAPSec.Core.Model;

namespace SAPSec.Web.Helpers;

/// <summary>
/// Display helper methods for rendering DataWithAvailability values.
/// Content editors can modify display text here.
/// </summary>
public static class DisplayHelpers
{
    #region Default Display Text - Content editors can modify these

    private const string NotAvailableText = "No available data";
    private const string RedactedText = "Data has been redacted";
    private const string NotApplicableText = "Not applicable";

    #endregion

    /// <summary>
    /// Renders a string value with appropriate fallback for unavailable data.
    /// </summary>
    public static string Display(this DataWithAvailability<string> data)
    {
        return data.Availability switch
        {
            DataAvailability.Available => data.Value ?? NotAvailableText,
            DataAvailability.Redacted => RedactedText,
            DataAvailability.NotApplicable => NotApplicableText,
            DataAvailability.Low => data.Value ?? NotAvailableText,
            _ => NotAvailableText
        };
    }

    /// <summary>
    /// Renders a boolean provision value with custom text.
    /// </summary>
    public static string DisplayAs(
        this DataWithAvailability<bool> data,
        string trueText,
        string falseText)
    {
        return data.Availability switch
        {
            DataAvailability.Available => data.Value ? trueText : falseText,
            DataAvailability.NotApplicable => NotApplicableText,
            DataAvailability.Redacted => RedactedText,
            _ => NotAvailableText
        };
    }

    /// <summary>
    /// Renders governance structure enum as display text.
    /// Content editors can modify the display strings here.
    /// </summary>
    public static string Display(this DataWithAvailability<GovernanceType> data)
    {
        if (data.Availability != DataAvailability.Available)
        {
            return NotAvailableText;
        }

        return data.Value switch
        {
            GovernanceType.MultiAcademyTrust => "Multi-academy trust (MAT)",
            GovernanceType.SingleAcademyTrust => "Single-academy trust (SAT)",
            GovernanceType.LocalAuthorityMaintained => "Local authority maintained",
            GovernanceType.NonMaintainedSpecialSchool => "Non-maintained special school",
            GovernanceType.Independent => "Independent",
            GovernanceType.FurtherHigherEducation => "Further/Higher education",
            GovernanceType.Other => "Other",
            _ => NotAvailableText
        };
    }

    /// <summary>
    /// Renders age range from low and high values.
    /// </summary>
    public static string DisplayAgeRange(
        this DataWithAvailability<int> low,
        DataWithAvailability<int> high)
    {
        if (!low.IsAvailable)
        {
            return NotAvailableText;
        }

        if (!high.IsAvailable)
        {
            return low.Value.ToString();
        }

        return $"{low.Value} to {high.Value}";
    }

    /// <summary>
    /// Renders local authority with code in parentheses.
    /// </summary>
    public static string DisplayWithCode(
        this DataWithAvailability<string> name,
        DataWithAvailability<string> code)
    {
        if (!name.IsAvailable)
        {
            return NotAvailableText;
        }

        return code.IsAvailable
            ? $"{name.Value} ({code.Value})"
            : name.Value!;
    }
}