using SAPData.Models;
using SAPSec.Data.Common.Catalogue.Validation;

namespace SAPSec.Data.Common.Tests.Catalogue.Validation;

public sealed class SourceProfilesTests : IDisposable
{
    private readonly string _dir = Directory.CreateTempSubdirectory("source-profiles-").FullName;

    public void Dispose() => Directory.Delete(_dir, recursive: true);

    private static DataMapRow Row(string file) => new()
    {
        PropertyName = "M_Tot_Est_Current_Num",
        FileName = file,
        Field = "Value",
        Filter = "Break Down",
        FilterValue = "Total",
    };

    [Fact]
    public void Collects_normalised_columns_and_filter_column_values()
    {
        File.WriteAllText(Path.Combine(_dir, "schools.csv"),
            "﻿School URN,Break Down,Value\n1, Total ,5\n2,\"Boys, all\",6\n3,Total,7\n");

        var profile = SourceProfiles.Build([Row("schools")], _dir).Files["schools"];

        profile.Columns.Should().Equal("break_down", "school_urn", "value");
        profile.Values.Keys.Should().Equal("break_down");
        profile.Values["break_down"].Should().Equal("Boys, all", "Total");
    }

    [Fact]
    public void Prefers_the_manual_copy_like_the_pipeline()
    {
        File.WriteAllText(Path.Combine(_dir, "schools.csv"), "Break Down,Value\nFrom download,1\n");
        File.WriteAllText(Path.Combine(_dir, "manual_schools.csv"), "Break Down,Value\nFrom manual,1\n");

        SourceProfiles.Build([Row("schools")], _dir).Files["schools"].Values["break_down"]
            .Should().Equal("From manual");
    }

    [Fact]
    public void Resolves_files_with_the_given_resolver()
    {
        // e.g. the pipeline maps "schools" to the downloaded, versioned file it will load.
        var versioned = Path.Combine(_dir, "schools_v2.0.clean.csv");
        File.WriteAllText(versioned, "Break Down,Value\nRenamed,1\n");

        var profiles = SourceProfiles.Build([Row("schools"), Row("missing")], file => file == "schools" ? versioned : null);

        profiles.Files.Keys.Should().Equal("schools");
        profiles.Files["schools"].Values["break_down"].Should().Equal("Renamed");
    }

    [Fact]
    public void Skips_files_that_are_not_present() =>
        SourceProfiles.Build([Row("missing")], _dir).Files.Should().BeEmpty();

    [Fact]
    public void Round_trips_through_json()
    {
        File.WriteAllText(Path.Combine(_dir, "schools.csv"), "Break Down,Value\nTotal,1\n");
        var path = Path.Combine(_dir, "profiles.json");

        SourceProfiles.Build([Row("schools")], _dir).Save(path);
        var loaded = SourceProfiles.Load(path);

        loaded.Files.Should().ContainKey("SCHOOLS", "file names are matched case-insensitively");
        loaded.Files["schools"].Values["break_down"].Should().Equal("Total");
    }
}
