using System.Globalization;
using CsvHelper;

namespace SAPSec.Infrastructure.Tests.Generators;

public class DestinationsDataMapTests
{
    [Theory]
    [InlineData("Tot", "Total", "Total")]
    [InlineData("EAL", "First language", "Known or believed to be other than English")]
    public void SchoolEducationAndEmploymentUseTheCorrectPupilGroup(
        string characteristic, string topic, string breakdown)
    {
        using var reader = new StreamReader(Path.Combine(AppContext.BaseDirectory, "DataMap", "datamap.csv"));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Read();
        csv.ReadHeader();
        var checkedRows = 0;

        while (csv.Read())
        {
            var property = csv.GetField("PropertyName") ?? "";
            if (csv.GetField("Range") != "Establishment" ||
                !(property.StartsWith($"Education_{characteristic}_Est_") ||
                  property.StartsWith($"Employment_{characteristic}_Est_")))
                continue;

            var filters = Enumerable.Range(1, 9)
                .Where(i => !string.IsNullOrEmpty(csv.GetField(i == 1 ? "Filter" : $"Filter{i}")))
                .ToDictionary(
                i => csv.GetField(i == 1 ? "Filter" : $"Filter{i}") ?? "",
                i => csv.GetField(i == 1 ? "FilterValue" : $"Filter{i}Value"));

            filters["breakdown_topic"].Should().Be(topic, property);
            filters["breakdown"].Should().Be(breakdown, property);
            checkedRows++;
        }

        checkedRows.Should().Be(12, "both measures have counts and percentages for three years");
    }
}
