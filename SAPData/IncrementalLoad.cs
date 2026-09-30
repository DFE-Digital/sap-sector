using System.Security.Cryptography;
using System.Text;

namespace SAPData;

/// <summary>
/// Rebuilds only what changed, decided by the database at run time rather than a hand-kept list.
/// </summary>
/// <remarks>
/// Each raw table is reloaded when its source file (or table definition) differs from the one it was last loaded
/// from, or when it doesn't exist. Reloading drops the table with CASCADE, which also drops every view built on it.
/// Each view is then rebuilt when it doesn't exist or its SQL (including the helper functions it calls) has changed.
/// Fingerprints are kept in <see cref="RawTableLog"/> and <see cref="ViewLog"/>. Uses psql's \gset and \if.
/// </remarks>
internal static class IncrementalLoad
{
    public const string RawTableLog = "raw_table_loads";
    public const string ViewLog = "view_builds";

    public static string Fingerprint(string sourceFilePath, string definition)
    {
        using var sha = SHA256.Create();
        using (var stream = File.OpenRead(sourceFilePath))
        {
            var buffer = new byte[1 << 20];
            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                sha.TransformBlock(buffer, 0, read, null, 0);
        }

        var definitionBytes = Encoding.UTF8.GetBytes(definition);
        sha.TransformFinalBlock(definitionBytes, 0, definitionBytes.Length);
        return Convert.ToHexStringLower(sha.Hash!);
    }

    public static string Fingerprint(string text) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    /// <summary>Creates the fingerprint tables. A full rebuild clears them so everything reloads and is recorded again.</summary>
    public static string LogTablesSql(bool rebuildAll)
    {
        var sql = new StringBuilder();
        sql.AppendLine($"CREATE TABLE IF NOT EXISTS {RawTableLog} (table_name text PRIMARY KEY, source_file text NOT NULL, fingerprint text NOT NULL, loaded_at timestamptz NOT NULL);");
        sql.AppendLine($"CREATE TABLE IF NOT EXISTS {ViewLog} (view_name text PRIMARY KEY, fingerprint text NOT NULL, built_at timestamptz NOT NULL);");
        if (rebuildAll)
            sql.AppendLine($"TRUNCATE {RawTableLog}, {ViewLog};");
        return sql.ToString();
    }

    /// <summary>Decides whether to reload a raw table and, if so, drops and recreates it (the COPY follows in 02_*).</summary>
    public static string RawTableCreate(string table, string sourceFile, string fingerprint, string createTable, bool forced)
    {
        var condition = forced
            ? "true"
            : $"to_regclass('{table}') IS NULL OR NOT EXISTS (SELECT 1 FROM {RawTableLog} WHERE table_name = '{table}' AND fingerprint = '{fingerprint}')";

        var sql = new StringBuilder();
        sql.AppendLine($"-- {sourceFile}");
        sql.AppendLine($"SELECT CASE WHEN {condition} THEN 'true' ELSE 'false' END AS load_{table} \\gset");
        sql.AppendLine($"\\if :load_{table}");
        sql.AppendLine($"\\echo 'Reloading {table} from {sourceFile}{(forced ? " (forced)" : "")}'");
        sql.AppendLine($"DELETE FROM {RawTableLog} WHERE table_name = '{table}';");
        sql.AppendLine($"DROP TABLE IF EXISTS {table} CASCADE;");
        sql.AppendLine(createTable);
        sql.AppendLine("\\else");
        sql.AppendLine($"\\echo 'Unchanged: {table}'");
        sql.AppendLine("\\endif");
        sql.AppendLine();
        return sql.ToString();
    }

    /// <summary>Loads a reloaded raw table and records its fingerprint only once the COPY has succeeded.</summary>
    public static string RawTableCopy(string table, string sourceFile, string fingerprint, string copy)
    {
        var sql = new StringBuilder();
        sql.AppendLine($"\\if :load_{table}");
        sql.AppendLine(copy);
        sql.AppendLine($"INSERT INTO {RawTableLog} (table_name, source_file, fingerprint, loaded_at) VALUES ('{table}', '{sourceFile}', '{fingerprint}', now())");
        sql.AppendLine("  ON CONFLICT (table_name) DO UPDATE SET source_file = EXCLUDED.source_file, fingerprint = EXCLUDED.fingerprint, loaded_at = EXCLUDED.loaded_at;");
        sql.AppendLine("\\endif");
        sql.AppendLine();
        return sql.ToString();
    }

    /// <summary>
    /// Drops raw tables this process loaded from a file that is no longer supplied, e.g. yesterday's GIAS extract,
    /// whose date is part of its file name and so of its table name.
    /// </summary>
    public static string DropSupersededTablesSql(IEnumerable<string> currentTables)
    {
        var tables = string.Join(", ", currentTables.Order(StringComparer.Ordinal).Select(t => $"'{t}'"));

        var sql = new StringBuilder();
        sql.AppendLine("-- Raw tables whose source file is no longer supplied");
        sql.AppendLine("DO $$");
        sql.AppendLine("DECLARE");
        sql.AppendLine("  r record;");
        sql.AppendLine("BEGIN");
        sql.AppendLine($"  FOR r IN SELECT table_name, source_file FROM {RawTableLog} WHERE table_name <> ALL (ARRAY[{tables}]::text[]) LOOP");
        sql.AppendLine("    RAISE NOTICE 'Dropping superseded raw table % (from %)', r.table_name, r.source_file;");
        sql.AppendLine("    EXECUTE format('DROP TABLE IF EXISTS %I CASCADE', r.table_name);");
        sql.AppendLine($"    DELETE FROM {RawTableLog} WHERE table_name = r.table_name;");
        sql.AppendLine("  END LOOP;");
        sql.AppendLine("END $$;");
        return sql.ToString();
    }

    /// <summary>Runs a view's SQL file only when the view is missing or its SQL has changed.</summary>
    public static string View(string view, string sqlFile, string fingerprint)
    {
        var sql = new StringBuilder();
        sql.AppendLine($"SELECT CASE WHEN to_regclass('{view}') IS NULL OR NOT EXISTS (SELECT 1 FROM {ViewLog} WHERE view_name = '{view}' AND fingerprint = '{fingerprint}') THEN 'true' ELSE 'false' END AS build_{view} \\gset");
        sql.AppendLine($"\\if :build_{view}");
        sql.AppendLine($"\\ir {sqlFile}");
        sql.AppendLine($"INSERT INTO {ViewLog} (view_name, fingerprint, built_at) VALUES ('{view}', '{fingerprint}', now())");
        sql.AppendLine("  ON CONFLICT (view_name) DO UPDATE SET fingerprint = EXCLUDED.fingerprint, built_at = EXCLUDED.built_at;");
        sql.AppendLine("\\else");
        sql.AppendLine($"\\echo 'Unchanged: {view}'");
        sql.AppendLine("\\endif");
        return sql.ToString();
    }
}
