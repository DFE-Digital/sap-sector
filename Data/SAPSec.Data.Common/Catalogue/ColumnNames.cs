using System.Text;

namespace SAPSec.Data.Common.Catalogue;

/// <summary>
/// Normalises a source column name the way the pipeline names raw table columns
/// (SAPData.GenerateRawTables.Sanitise / GenerateViews.DbCol): lower case, non-alphanumeric → '_'.
/// </summary>
public static class ColumnNames
{
    public static string Normalise(string? header)
    {
        if (string.IsNullOrWhiteSpace(header))
            return header ?? "";

        var s = header.Trim().TrimStart('﻿').ToLowerInvariant();
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
            sb.Append(char.IsLetterOrDigit(ch) ? ch : '_');

        return sb.ToString();
    }
}
