using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using SAPSec.Data;

namespace SAPSec.Data.Common.Catalogue.Definitions;

/// <summary>
/// Reads the data map definitions (SAPData/DataMap/Definitions/*.json, built into this assembly) into the measure
/// sets and column sets that expand to data map rows. The format is described in dataset.schema.json and the
/// catalogue README; errors name the file and the place in it.
/// </summary>
internal static partial class JsonDefinitions
{
    private const string ResourcePrefix = "DataMap/Definitions/";

    private static readonly Dictionary<string, Breakdown> PupilGroups = new(StringComparer.Ordinal)
    {
        ["Total"] = Breakdowns.Total,
        ["Sum"] = Breakdowns.Sum,
        ["Boys"] = Breakdowns.Boys,
        ["Girls"] = Breakdowns.Girls,
        ["Disadvantaged"] = Breakdowns.Disadvantaged,
        ["NotDisadvantaged"] = Breakdowns.NotDisadvantaged,
        ["Eal"] = Breakdowns.Eal,
        ["FirstLanguageEnglish"] = Breakdowns.FirstLanguageEnglish,
        ["Mobile"] = Breakdowns.Mobile,
        ["NonMobile"] = Breakdowns.NonMobile,
    };

    private static readonly Dictionary<Period, int> YearsBefore = new()
    {
        [Period.Current] = 0,
        [Period.Previous] = 1,
        [Period.Previous2] = 2,
    };

    // {name} or {name.field}: a forEach variable.
    [GeneratedRegex(@"\{(?<name>[A-Za-z][A-Za-z0-9]*)(?:\.(?<field>[A-Za-z][A-Za-z0-9]*))?\}")]
    private static partial Regex Variable();

    /// <summary>Every dataset listed in catalogue.json, in order.</summary>
    public static IReadOnlyList<IDataMapDefinition> Load()
    {
        var index = Read("catalogue.json");
        return Array(index, "datasets")
            .Select(file => file!.GetValue<string>())
            .SelectMany(file => new DatasetFile(file, Read(file)).Definitions())
            .ToList();
    }

    /// <summary>One dataset file's definitions, e.g. for testing a file's format.</summary>
    internal static IReadOnlyList<IDataMapDefinition> LoadDataset(string file, string json) =>
        new DatasetFile(file, Parse(file, json)).Definitions().ToList();

    private static JsonObject Read(string file)
    {
        using var stream = typeof(JsonDefinitions).Assembly.GetManifestResourceStream(ResourcePrefix + file)
            ?? throw new CatalogueException($"{file}: not found in SAPData/DataMap/Definitions.");
        using var reader = new StreamReader(stream);
        return Parse(file, reader.ReadToEnd());
    }

    private static JsonObject Parse(string file, string json)
    {
        try
        {
            return JsonNode.Parse(json, documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip })?.AsObject()
                ?? throw new CatalogueException($"{file}: is empty.");
        }
        catch (JsonException e)
        {
            throw new CatalogueException($"{file}: isn't valid JSON{(e.LineNumber is { } line ? $" (line {line + 1})" : "")}: {e.Message}");
        }
    }

    private static JsonArray Array(JsonObject node, string name) =>
        node[name] as JsonArray ?? throw new CatalogueException($"{node.GetPath()}.{name}: expected a list.");

    /// <summary>One dataset file: its lists, pupil group sets, measure sets and column sets.</summary>
    private sealed class DatasetFile(string file, JsonObject root)
    {
        private static readonly HashSet<string> FileKeys = ["$schema", "dataset", "description", "lists", "pupilGroupSets", "measureSets", "columnSets"];
        private static readonly HashSet<string> SetKeys = ["dataset", "subtype", "description", "forEach", "yearsFrom", "periods", "pupilGroups", "sources", "measures"];
        private static readonly HashSet<string> SourceKeys = ["id", "description", "scopes", "periods", "publisher", "file", "key", "where", "pupilGroups"];
        private static readonly HashSet<string> MeasureKeys = ["name", "description", "column", "unit", "dataType", "nameTemplate", "pupilGroups", "scopes", "periods", "skip", "where", "whereBySource", "columnBySource"];
        private static readonly HashSet<string> LoopKeys = ["forEach", "description", "measures"];
        private static readonly HashSet<string> ColumnSetKeys = ["dataset", "description", "scope", "publisher", "file", "key", "columns"];

        public IEnumerable<IDataMapDefinition> Definitions()
        {
            Wrap(root, () => CheckKeys(root, FileKeys));

            foreach (var set in Optional(root, "measureSets"))
            {
                foreach (var expanded in Wrap(set!, () => Expand(set!.AsObject(), Variables.None).ToList()))
                    yield return Wrap(set, () => BuildMeasureSet(expanded));
            }

            foreach (var set in Optional(root, "columnSets"))
                yield return Wrap(set!, () => BuildColumnSet(set!.AsObject()));
        }

        // A set with forEach becomes one set per combination of values.
        private IEnumerable<JsonObject> Expand(JsonObject set, Variables outer)
        {
            if (set["forEach"] is not JsonObject forEach)
                return [set];

            return Combinations(forEach, outer).Select(vars =>
            {
                var copy = Substitute(set, vars).AsObject();
                copy.Remove("forEach");
                return copy;
            });
        }

        private MeasureSet BuildMeasureSet(JsonObject set)
        {
            CheckKeys(set, SetKeys);
            var dataset = Text(set, "dataset", required: false) ?? Text(root, "dataset")!;
            var result = new MeasureSet(dataset, Text(set, "subtype")!);

            var yearsFrom = Text(set, "yearsFrom")!;
            var current = DataYear(set, yearsFrom);
            var periods = set["periods"] is null ? [Period.Current, Period.Previous, Period.Previous2] : Enums<Period>(set, "periods");
            foreach (var period in periods)
                result.Year(period, new AcademicYear(current - YearsBefore[period]));

            if (set["pupilGroups"] is not null)
                result.Breakdowns(Groups(set, "pupilGroups"));

            var sourcesById = new Dictionary<string, List<Source>>(StringComparer.Ordinal);
            foreach (var node in Array(set, "sources"))
            {
                var source = node!.AsObject();
                CheckKeys(source, SourceKeys);
                var id = Text(source, "id", required: false);

                foreach (var scope in Enums<Scope>(source, "scopes"))
                {
                    foreach (var period in source["periods"] is null ? periods : Enums<Period>(source, "periods"))
                    {
                        var built = BuildSource(source, scope, new AcademicYear(current - YearsBefore[period]));
                        result.Source(scope, period, built);
                        if (id is not null)
                            (sourcesById.TryGetValue(id, out var list) ? list : sourcesById[id] = []).Add(built);
                    }
                }
            }

            foreach (var measure in Measures(Array(set, "measures"), Variables.None))
                result.Metric(BuildMetric(measure, sourcesById));

            return result;
        }

        // Measures in order; a { "forEach", "measures" } block repeats its measures for each combination.
        private IEnumerable<JsonObject> Measures(JsonArray nodes, Variables outer)
        {
            foreach (var node in nodes)
            {
                var measure = node!.AsObject();
                if (measure["forEach"] is not JsonObject forEach)
                {
                    // Outside a loop the original is kept, so errors can say where it is in the file.
                    yield return outer.IsEmpty ? measure : (JsonObject)Substitute(measure, outer);
                    continue;
                }

                CheckKeys(measure, LoopKeys);
                foreach (var vars in Combinations(forEach, outer))
                    foreach (var inner in Measures(Array(measure, "measures"), vars))
                        yield return inner;
            }
        }

        private Source BuildSource(JsonObject node, Scope scope, AcademicYear year)
        {
            string WithYear(string text) => text.Replace("{year}", year.Code);

            var source = Catalogue.Source.From(Text(node, "publisher")!, WithYear(Text(node, "file")!));

            source.KeyedBy(node["key"] switch
            {
                JsonObject byScope => byScope[scope.ToString()]?.GetValue<string>()
                    ?? throw new CatalogueException($"{node.GetPath()}.key: no key column for {scope}."),
                JsonValue value => value.GetValue<string>(),
                _ => throw new CatalogueException($"{node.GetPath()}.key: expected a column name, or one per scope."),
            });

            if (node["where"] is JsonObject where)
            {
                foreach (var (column, value) in where)
                {
                    // A per-scope value applies only to the scopes it names.
                    var values = value is JsonObject byScope ? byScope[scope.ToString()] : value;
                    if (values is not null)
                        source.Where(column, Strings(values).Select(WithYear).ToArray());
                }
            }

            foreach (var (group, filters) in GroupFilters(node))
                source.Provides(group, filters.Select(f => (f.Column, WithYear(f.Value))).ToArray());

            return source;
        }

        // "pupilGroups": a set name, an object of group → filters, or a list of either.
        private IEnumerable<(Breakdown Group, (string Column, string Value)[] Filters)> GroupFilters(JsonObject source)
        {
            var node = source["pupilGroups"];
            var parts = node switch
            {
                null => [],
                JsonArray list => list.Select(p => p!),
                _ => [node],
            };

            foreach (var part in parts)
            {
                var groups = part is JsonValue name
                    ? root["pupilGroupSets"]?[name.GetValue<string>()] as JsonObject
                      ?? throw new CatalogueException($"{part.GetPath()}: no pupil group set named '{name}' in pupilGroupSets.")
                    : part.AsObject();

                foreach (var (group, filters) in groups)
                {
                    var columns = filters?.AsObject() ?? throw new CatalogueException($"{groups.GetPath()}.{group}: expected filters, e.g. {{ \"breakdown\": \"Boys\" }}.");
                    yield return (Group(group, groups), columns.Select(f => (f.Key, f.Value!.GetValue<string>())).ToArray());
                }
            }
        }

        private Metric BuildMetric(JsonObject node, Dictionary<string, List<Source>> sourcesById)
        {
            CheckKeys(node, MeasureKeys);
            var metric = new Metric(Text(node, "name")!, Text(node, "column", required: false) ?? "");

            switch (Text(node, "unit", required: false))
            {
                case null or "Num": break;
                case "Pct": metric.Percentage(); break;
                case var unit: throw new CatalogueException($"{node.GetPath()}.unit: '{unit}' should be Num or Pct.");
            }

            if (Text(node, "dataType", required: false) is { } dataType)
            {
                var types = System.Enum.GetValues<DataType>().Where(d => d.DataMapValue() == dataType).ToList();
                metric.OfType(types.Count == 1
                    ? types[0]
                    : throw new CatalogueException($"{node.GetPath()}.dataType: '{dataType}' should be double, string or string array."));
            }

            if (Text(node, "nameTemplate", required: false) is { } template)
                metric.Named(template);
            if (node["pupilGroups"] is not null)
                metric.For([.. Groups(node, "pupilGroups")]);
            if (node["scopes"] is not null)
                metric.In([.. Enums<Scope>(node, "scopes")]);
            if (node["periods"] is not null)
                metric.During([.. Enums<Period>(node, "periods")]);

            foreach (var skip in Optional(node, "skip"))
                metric.Skip(Enum<Scope>(skip!.AsObject(), "scope"), Enum<Period>(skip.AsObject(), "period"));

            if (node["where"] is JsonObject where)
                foreach (var (column, values) in where)
                    metric.Where(column, Strings(values!).ToArray());

            if (node["whereBySource"] is JsonObject whereBySource)
            {
                foreach (var (id, filters) in whereBySource)
                    foreach (var source in SourcesNamed(id, whereBySource, sourcesById))
                        foreach (var (column, values) in filters!.AsObject())
                            metric.Where(source, column, Strings(values!).ToArray());
            }

            if (node["columnBySource"] is JsonObject columnBySource)
            {
                foreach (var (id, column) in columnBySource)
                {
                    foreach (var source in SourcesNamed(id, columnBySource, sourcesById))
                    {
                        if (column is JsonObject byGroup)
                            foreach (var (group, field) in byGroup)
                                metric.Field(source, Group(group, byGroup), field!.GetValue<string>());
                        else
                            metric.Field(source, column!.GetValue<string>());
                    }
                }
            }

            return metric;
        }

        private ColumnSet BuildColumnSet(JsonObject node)
        {
            CheckKeys(node, ColumnSetKeys);
            var source = Catalogue.Source.From(Text(node, "publisher")!, Text(node, "file")!).KeyedBy(Text(node, "key")!);
            var set = new ColumnSet(Text(node, "dataset")!, Text(node, "scope")!, source);

            var columns = node["columns"] as JsonObject ?? throw new CatalogueException($"{node.GetPath()}.columns: expected property → column pairs.");
            foreach (var (property, column) in columns)
                set.Column(property, column!.GetValue<string>());

            return set;
        }

        private static IEnumerable<Source> SourcesNamed(string id, JsonObject parent, Dictionary<string, List<Source>> sourcesById) =>
            sourcesById.TryGetValue(id, out var sources)
                ? sources
                : throw new CatalogueException($"{parent.GetPath()}.{id}: no source with \"id\": \"{id}\" in this measure set.");

        private int DataYear(JsonObject set, string name) =>
            typeof(DataYears).GetField(name, BindingFlags.Public | BindingFlags.Static)?.GetRawConstantValue() as int?
            ?? throw new CatalogueException($"{set.GetPath()}.yearsFrom: '{name}' isn't a year in Data/SAPSec.Data/DataYears.cs.");

        // forEach: each variable is a list, the name of a list in "lists", or a {variable.field} naming one.
        // Later variables repeat inside earlier ones, like nested loops.
        private IEnumerable<Variables> Combinations(JsonObject forEach, Variables outer)
        {
            IEnumerable<Variables> combinations = [outer];
            foreach (var (name, values) in forEach)
            {
                combinations = combinations.SelectMany(vars => ListFor(forEach, name, values, vars).Select(item => vars.With(name, item!)));
            }

            return combinations.ToList();
        }

        private JsonArray ListFor(JsonObject forEach, string name, JsonNode? values, Variables vars) => values switch
        {
            JsonArray list => list,
            JsonValue listName when root["lists"]?[vars.Replace(listName.GetValue<string>())] is JsonArray list => list,
            _ => throw new CatalogueException($"{forEach.GetPath()}.{name}: expected a list, or the name of one in \"lists\"."),
        };

        private static JsonNode Substitute(JsonNode node, Variables vars) => node switch
        {
            JsonObject obj => new JsonObject(obj.Select(p => KeyValuePair.Create(p.Key, p.Value is null ? null : Substitute(p.Value, vars)))),
            JsonArray array => new JsonArray(array.Select(item => item is null ? null : Substitute(item, vars)).ToArray()),
            JsonValue value when value.TryGetValue<string>(out var text) => JsonValue.Create(vars.Replace(text)),
            _ => node.DeepClone(),
        };

        private T Wrap<T>(JsonNode node, Func<T> build)
        {
            try
            {
                return build();
            }
            catch (CatalogueException e) when (!e.Message.StartsWith(file))
            {
                throw new CatalogueException($"{file}: {e.Message}");
            }
            catch (Exception e) when (e is InvalidOperationException or FormatException or KeyNotFoundException)
            {
                throw new CatalogueException($"{file}: {node.GetPath()}: {e.Message}");
            }
        }

        private void Wrap(JsonNode node, Action check) => Wrap(node, () => { check(); return 0; });

        private static void CheckKeys(JsonObject node, HashSet<string> allowed)
        {
            var unknown = node.Select(p => p.Key).Where(k => !allowed.Contains(k)).ToList();
            if (unknown.Count > 0)
            {
                var name = node["name"] is JsonValue value && value.TryGetValue<string>(out var text) ? $" ({text})" : "";
                throw new CatalogueException($"{node.GetPath()}{name}: unknown setting(s) {string.Join(", ", unknown.Select(k => $"'{k}'"))}; expected one of {string.Join(", ", allowed)}.");
            }
        }

        private static IEnumerable<JsonNode?> Optional(JsonObject node, string name) => node[name] as JsonArray ?? [];

        private static string? Text(JsonObject node, string name, bool required = true) =>
            node[name]?.GetValue<string>() ?? (required ? throw new CatalogueException($"{node.GetPath()}: '{name}' is required.") : null);

        private static IEnumerable<string> Strings(JsonNode node) => node switch
        {
            JsonArray list => list.Select(v => v!.GetValue<string>()),
            _ => [node.GetValue<string>()],
        };

        private static T Enum<T>(JsonObject node, string name) where T : struct, Enum =>
            System.Enum.TryParse<T>(Text(node, name), out var value) && System.Enum.IsDefined(value)
                ? value
                : throw new CatalogueException($"{node.GetPath()}.{name}: '{Text(node, name)}' should be one of {string.Join(", ", System.Enum.GetNames<T>())}.");

        private static List<T> Enums<T>(JsonObject node, string name) where T : struct, Enum =>
            Array(node, name).Select(v => System.Enum.TryParse<T>(v!.GetValue<string>(), out var value) && System.Enum.IsDefined(value)
                    ? value
                    : throw new CatalogueException($"{node.GetPath()}.{name}: '{v}' should be one of {string.Join(", ", System.Enum.GetNames<T>())}."))
                .ToList();

        private static Breakdown Group(string name, JsonNode parent) =>
            PupilGroups.TryGetValue(name, out var group)
                ? group
                : throw new CatalogueException($"{parent.GetPath()}: unknown pupil group '{name}'; expected one of {string.Join(", ", PupilGroups.Keys)}.");

        private static List<Breakdown> Groups(JsonObject node, string name) =>
            Array(node, name).Select(g => Group(g!.GetValue<string>(), node)).ToList();
    }

    /// <summary>forEach variables in scope: name → the current item.</summary>
    private sealed class Variables
    {
        private readonly Dictionary<string, JsonNode> _values;

        private Variables(Dictionary<string, JsonNode> values) => _values = values;

        public static Variables None { get; } = new(new Dictionary<string, JsonNode>(StringComparer.Ordinal));

        public bool IsEmpty => _values.Count == 0;

        public Variables With(string name, JsonNode value) => new(new Dictionary<string, JsonNode>(_values, StringComparer.Ordinal) { [name] = value });

        // Replaces {name} and {name.field} for variables in scope; other placeholders ({year}, {metric}, …) are kept.
        public string Replace(string text) => _values.Count == 0 ? text : Variable().Replace(text, match =>
        {
            if (!_values.TryGetValue(match.Groups["name"].Value, out var item))
                return match.Value;

            var value = match.Groups["field"].Success
                ? item[match.Groups["field"].Value] ?? throw new CatalogueException($"{item.GetPath()}: no '{match.Groups["field"].Value}' for {match.Value}.")
                : item;
            return value.GetValue<string>();
        });
    }
}
