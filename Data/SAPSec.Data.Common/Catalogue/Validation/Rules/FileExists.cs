namespace SAPSec.Data.Common.Catalogue.Validation.Rules;

/// <summary>Every file the data map reads exists.</summary>
public sealed class FileExists : IValidationRule
{
    public const string RuleName = "unknown-file";

    public string Name => RuleName;

    public string Description => "Every file the data map reads exists.";

    public IEnumerable<ValidationIssue> Check(ValidationContext context)
    {
        if (context.Profiles is null)
            yield break;

        foreach (var r in context.Rows)
        {
            if (!context.Profiles.Files.ContainsKey(RuleText.File(r)))
                yield return this.Issue(r, $"no profile for source file '{RuleText.File(r)}'; regenerate the source profiles");
        }
    }
}
