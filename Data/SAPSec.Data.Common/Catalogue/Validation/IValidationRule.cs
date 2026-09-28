using SAPData.Models;

namespace SAPSec.Data.Common.Catalogue.Validation;

/// <summary>
/// One check on the data map. Each rule is its own class in <c>Rules/</c>; <see cref="CatalogueValidator.Rules"/>
/// lists the ones that run. To add a rule, write a class and add it to that list.
/// </summary>
public interface IValidationRule
{
    /// <summary>Short name shown in each issue, e.g. <c>unknown-value</c>.</summary>
    string Name { get; }

    /// <summary>What the rule checks, in one sentence.</summary>
    string Description { get; }

    IEnumerable<ValidationIssue> Check(ValidationContext context);
}

/// <summary>
/// What a rule checks: the data map rows and, when available, the source files' columns and values.
/// Rules that compare with the files return nothing when <see cref="Profiles"/> is null.
/// </summary>
public sealed record ValidationContext(IReadOnlyList<DataMapRow> Rows, SourceProfiles? Profiles);

public sealed record ValidationIssue(string Rule, string View, string Property, string Message)
{
    public override string ToString() => $"[{Rule}] {View}.{Property}: {Message}";
}

internal static class RuleText
{
    public static string View(DataMapRow r) => $"{r.Type}/{r.Range}";

    public static string File(DataMapRow r) => r.FileName.Trim().TrimStart('﻿');

    public static ValidationIssue Issue(this IValidationRule rule, DataMapRow r, string message) =>
        new(rule.Name, View(r), r.PropertyName, message);
}
