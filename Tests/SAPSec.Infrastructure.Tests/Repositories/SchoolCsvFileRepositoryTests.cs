using SAPSec.Infrastructure.Repositories;

namespace SAPSec.Infrastructure.Tests.Repositories;

public class SchoolCsvFileRepositoryTests : IDisposable
{
    private readonly string _testDirectory;

    public SchoolCsvFileRepositoryTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SchoolTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        // Cleanup test files and directory
        if (Directory.Exists(_testDirectory))
        {
            try { Directory.Delete(_testDirectory, true); } catch { /* ignored */ }
        }
    }

    private string CreateTestCsvFile(string content, string? fileName = null)
    {
        var filePath = Path.Combine(_testDirectory, fileName ?? $"test_{Guid.NewGuid()}.csv");
        File.WriteAllText(filePath, content);
        return filePath;
    }

    [Fact]
    public void GetAll_WithValidCsvFile_ShouldReturnSchools()
    {
        var csvContent = @"URN,UKPRN,LA (code),EstablishmentNumber,EstablishmentName
                           1,2,3,123,Test School
                           4,5,6,456,Another School";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new SchoolCsvFileRepository(csvPath);

        var results = repository.GetAll().ToList();

        results.Should().HaveCount(2);

        results[0].Urn.Should().Be(1);
        results[0].Ukprn.Should().Be(2);
        results[0].DfENumber.Should().Be("3/123");
        results[0].SearchAbleDfENumber.Should().Be(3123);
        results[0].EstablishmentName.Should().Be("Test School");

        results[1].Urn.Should().Be(4);
        results[1].Ukprn.Should().Be(5);
        results[1].DfENumber.Should().Be("6/456");
        results[1].SearchAbleDfENumber.Should().Be(6456);
        results[1].EstablishmentName.Should().Be("Another School");
    }

    [Fact]
    public void GetAll_WithSchoolNameHavingWhitespace_ShouldTrimWhitespace()
    {
        var csvContent = @"URN,UKPRN,LA (code),EstablishmentNumber,EstablishmentName
                           1, 2, 3, 100,  Whitespace School  ";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new SchoolCsvFileRepository(csvPath);

        var results = repository.GetAll().ToList();

        results.Should().ContainSingle();
        results[0].EstablishmentName.Should().Be("Whitespace School");
    }

    [Fact]
    public void GetAll_WithMultipleValidAndInvalidRows_ShouldReturnOnlyValidSchools()
    {
        var csvContent = @"URN,UKPRN,LA (code),EstablishmentNumber,EstablishmentName
                           1,2,3,100,Valid School 1
                           invalid,Invalid Number
                           4,5,6,200,Valid School 2
                           300,";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new SchoolCsvFileRepository(csvPath);

        var results = repository.GetAll().ToList();

        results.Should().HaveCount(2);
        results[0].Urn.Should().Be(1);
        results[0].EstablishmentName.Should().Be("Valid School 1");
        results[1].Urn.Should().Be(4);
        results[1].EstablishmentName.Should().Be("Valid School 2");
    }

    [Fact]
    public void GetAll_WithEmptyPath_ShouldReturnEmptyEnumerable()
    {
        var repository = new SchoolCsvFileRepository(string.Empty);

        var results = repository.GetAll().ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void GetAll_WithWhitespacePath_ShouldReturnEmptyEnumerable()
    {
        var repository = new SchoolCsvFileRepository("   ");

        var results = repository.GetAll().ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void GetAll_WithNonExistentFile_ShouldReturnEmptyEnumerable()
    {
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.csv");
        var repository = new SchoolCsvFileRepository(nonExistentPath);

        var results = repository.GetAll().ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void GetAll_WithEmptyFile_ShouldReturnEmptyEnumerable()
    {
        var csvPath = CreateTestCsvFile(string.Empty);
        var repository = new SchoolCsvFileRepository(csvPath);

        var results = repository.GetAll().ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void GetAll_WithOnlyHeadersAndNewline_ShouldReturnEmptyEnumerable()
    {
        var csvContent = "URN,UKPRN,LA (code),EstablishmentNumber,EstablishmentName\n";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new SchoolCsvFileRepository(csvPath);

        var results = repository.GetAll().ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void GetAll_WithSpecialCharactersInName_ShouldIncludeSchool()
    {
        var csvContent = @"URN,UKPRN,LA (code),EstablishmentNumber,EstablishmentName
                           1,2,3,100,St. Mary's School & College!";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new SchoolCsvFileRepository(csvPath);

        var results = repository.GetAll().ToList();

        results.Should().ContainSingle();
        results[0].EstablishmentName.Should().Be("St. Mary's School & College!");
    }

    [Fact]
    public void GetAll_WithQuotedFields_ShouldHandleCorrectly()
    {
        var csvContent = @"URN,UKPRN,LA (code),EstablishmentNumber,EstablishmentName
                           1,2,3,100,""School with, comma""
                           4,5,6,200,School with ""quotes""";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new SchoolCsvFileRepository(csvPath);

        var results = repository.GetAll().ToList();

        results.Should().HaveCount(2);
        results[0].EstablishmentName.Should().Be("School with, comma");
        results[1].EstablishmentName.Should().Be("School with \"quotes\"");
    }

    [Fact]
    public void GetAll_WithExtraColumns_ShouldIgnoreExtraColumns()
    {
        var csvContent = @"URN,UKPRN,LA (code),EstablishmentNumber,EstablishmentName,ExtraColumn
                           1,2,3,100,Test School,Extra Data";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new SchoolCsvFileRepository(csvPath);

        var results = repository.GetAll().ToList();

        results.Should().ContainSingle();
        results[0].Urn.Should().Be(1);
        results[0].EstablishmentName.Should().Be("Test School");
    }

    [Fact]
    public void GetAll_WithFileDeletedAfterConstruction_ShouldReturnEmpty()
    {
        var csvContent = @"URN,UKPRN,LA (code),EstablishmentNumber,EstablishmentName
                           100,Test School";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new SchoolCsvFileRepository(csvPath);
        File.Delete(csvPath);

        var results = repository.GetAll().ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void GetAll_WhenCalledMultipleTimes_ShouldUseCachedResults()
    {
        var csvContent = @"URN,UKPRN,LA (code),EstablishmentNumber,EstablishmentName
                               1,2,3,100,Test School";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new SchoolCsvFileRepository(csvPath);

        var firstCall = repository.GetAll();
        var secondCall = repository.GetAll();

        secondCall.Should().BeEquivalentTo(firstCall);
        ReferenceEquals(firstCall, secondCall).Should().BeTrue("results should be cached in memory");
    }

    [Fact]
    public void GetSchoolByUrn_WithExistingUrn_ShouldReturnMatchingSchool()
    {
        var csvContent = @"URN,UKPRN,LA (code),EstablishmentNumber,EstablishmentName
                               1,10,100,1000,First School
                               2,20,200,2000,Second School";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new SchoolCsvFileRepository(csvPath);

        var school = repository.GetSchoolByUrn(2);

        school.Should().NotBeNull();
        school.Urn.Should().Be(2);
        school.EstablishmentName.Should().Be("Second School");
    }

    [Fact]
    public void GetSchoolByUrn_WithNonExistingUrn_ShouldThrowInvalidOperationException()
    {
        var csvContent = @"URN,UKPRN,LA (code),EstablishmentNumber,EstablishmentName
                               1,10,100,1000,First School";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new SchoolCsvFileRepository(csvPath);

        Action act = () => repository.GetSchoolByUrn(999);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetSchoolByNumber_WhenNumberMatchesUrn_ShouldReturnSchool()
    {
        var csvContent = @"URN,UKPRN,LA (code),EstablishmentNumber,EstablishmentName
                               5,50,500,5000,Urn Match School";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new SchoolCsvFileRepository(csvPath);

        var school = repository.GetSchoolByNumber(5);

        school.Should().NotBeNull();
        school.Urn.Should().Be(5);
        school.EstablishmentName.Should().Be("Urn Match School");
    }

    [Fact]
    public void GetSchoolByNumber_WhenNumberMatchesUkprn_ShouldReturnSchool()
    {
        var csvContent = @"URN,UKPRN,LA (code),EstablishmentNumber,EstablishmentName
                               1,12345,10,100,Ukprn Match School";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new SchoolCsvFileRepository(csvPath);

        var school = repository.GetSchoolByNumber(12345);

        school.Should().NotBeNull();
        school.Ukprn.Should().Be(12345);
        school.EstablishmentName.Should().Be("Ukprn Match School");
    }

    [Fact]
    public void GetSchoolByNumber_WhenNumberDoesNotMatchAnySchool_ShouldReturnNull()
    {
        var csvContent = @"URN,UKPRN,LA (code),EstablishmentNumber,EstablishmentName
                               1,10,100,1000,First School
                               2,20,200,2000,Second School";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new SchoolCsvFileRepository(csvPath);

        var school = repository.GetSchoolByNumber(99999);

        school.Should().BeNull();
    }

    [Fact]
    public void GetSchoolByUrn_AfterCallingGetAll_ShouldUseCachedSchools()
    {
        var csvContent = @"URN,UKPRN,LA (code),EstablishmentNumber,EstablishmentName
                               10,100,1,1,Cached School";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new SchoolCsvFileRepository(csvPath);

        // Prime the cache
        var all = repository.GetAll();
        all.Should().HaveCount(1);

        // Delete the underlying file to ensure data is coming from cache
        File.Delete(csvPath);

        var school = repository.GetSchoolByUrn(10);

        school.Should().NotBeNull();
        school.Urn.Should().Be(10);
        school.EstablishmentName.Should().Be("Cached School");
    }

    [Fact]
    public void GetSchoolByNumber_AfterCallingGetAll_ShouldUseCachedSchools()
    {
        var csvContent = @"URN,UKPRN,LA (code),EstablishmentNumber,EstablishmentName
                               11,22222,3,4,Cached Number School";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new SchoolCsvFileRepository(csvPath);

        // Prime the cache
        var all = repository.GetAll();
        all.Should().HaveCount(1);

        // Delete the underlying file to ensure data is coming from cache
        File.Delete(csvPath);

        var school = repository.GetSchoolByNumber(22222);

        school.Should().NotBeNull();
        school.Ukprn.Should().Be(22222);
        school.EstablishmentName.Should().Be("Cached Number School");
    }
}
