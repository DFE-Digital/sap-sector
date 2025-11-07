using SAPSec.Infrastructure.Repositories;

namespace SAPSec.Infrastructure.Tests.Repositories;

public class EstablishmentCsvFileRepositoryTests : IDisposable
{
    private readonly string _testDirectory;

    public EstablishmentCsvFileRepositoryTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"EstablishmentTests_{Guid.NewGuid()}");
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

    // [Fact]
    // public void Constructor_WithNonNullPath_ShouldCreateInstance()
    // {
    //     var csvPath = "valid/path/to/file.csv";
    //
    //     var repository = new EstablishmentCsvFileRepository(csvPath);
    //
    //     repository.Should().NotBeNull();
    // }
    //
    // [Fact]
    // public void Constructor_WithNullPath_ShouldThrowArgumentNullException()
    // {
    //     string csvPath = null!;
    //
    //     var act = () => new EstablishmentCsvFileRepository(csvPath);
    //
    //     act.Should().Throw<ArgumentNullException>()
    //         .WithParameterName("csvPath");
    // }

    [Fact]
    public void GetAll_WithValidCsvFile_ShouldReturnEstablishments()
    {
        var csvContent = @"EstablishmentNumber,EstablishmentName
                           123,Test School
                           456,Another School";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new EstablishmentCsvFileRepository(csvPath);

        var results = repository.GetAll().ToList();

        results.Should().HaveCount(2);
        results[0].EstablishmentNumber.Should().Be(123);
        results[0].EstablishmentName.Should().Be("Test School");
        results[1].EstablishmentNumber.Should().Be(456);
        results[1].EstablishmentName.Should().Be("Another School");
    }

    [Fact]
    public void GetAll_WithEstablishmentNameHavingWhitespace_ShouldTrimWhitespace()
    {
        var csvContent = @"EstablishmentNumber,EstablishmentName
                           100,  Whitespace School  ";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new EstablishmentCsvFileRepository(csvPath);

        var results = repository.GetAll().ToList();

        results.Should().ContainSingle();
        results[0].EstablishmentName.Should().Be("Whitespace School");
    }

    [Fact]
    public void GetAll_WithMultipleValidAndInvalidRows_ShouldReturnOnlyValidEstablishments()
    {
        var csvContent = @"EstablishmentNumber,EstablishmentName
                           100,Valid School 1
                           invalid,Invalid Number
                           200,Valid School 2
                           300,";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new EstablishmentCsvFileRepository(csvPath);

        var results = repository.GetAll().ToList();

        results.Should().HaveCount(2);
        results[0].EstablishmentNumber.Should().Be(100);
        results[0].EstablishmentName.Should().Be("Valid School 1");
        results[1].EstablishmentNumber.Should().Be(200);
        results[1].EstablishmentName.Should().Be("Valid School 2");
    }

    [Fact]
    public void GetAll_WithEmptyPath_ShouldReturnEmptyEnumerable()
    {
        var repository = new EstablishmentCsvFileRepository(string.Empty);

        var results = repository.GetAll().ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void GetAll_WithWhitespacePath_ShouldReturnEmptyEnumerable()
    {
        var repository = new EstablishmentCsvFileRepository("   ");

        var results = repository.GetAll().ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void GetAll_WithNonExistentFile_ShouldReturnEmptyEnumerable()
    {
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.csv");
        var repository = new EstablishmentCsvFileRepository(nonExistentPath);

        var results = repository.GetAll().ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void GetAll_WithEmptyFile_ShouldReturnEmptyEnumerable()
    {
        var csvPath = CreateTestCsvFile(string.Empty);
        var repository = new EstablishmentCsvFileRepository(csvPath);

        var results = repository.GetAll().ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void GetAll_WithOnlyHeadersAndNewline_ShouldReturnEmptyEnumerable()
    {
        var csvContent = "EstablishmentNumber,EstablishmentName\n";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new EstablishmentCsvFileRepository(csvPath);

        var results = repository.GetAll().ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void GetAll_WithNegativeEstablishmentNumber_ShouldIncludeEstablishment()
    {
        var csvContent = @"EstablishmentNumber,EstablishmentName
                           -100,Negative Number School";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new EstablishmentCsvFileRepository(csvPath);

        var results = repository.GetAll().ToList();

        results.Should().ContainSingle();
        results[0].EstablishmentNumber.Should().Be(-100);
        results[0].EstablishmentName.Should().Be("Negative Number School");
    }

    [Fact]
    public void GetAll_WithSpecialCharactersInName_ShouldIncludeEstablishment()
    {
        var csvContent = @"EstablishmentNumber,EstablishmentName
                           100,St. Mary's School & College!";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new EstablishmentCsvFileRepository(csvPath);

        var results = repository.GetAll().ToList();

        results.Should().ContainSingle();
        results[0].EstablishmentName.Should().Be("St. Mary's School & College!");
    }

    [Fact]
    public void GetAll_WithQuotedFields_ShouldHandleCorrectly()
    {
        var csvContent = @"EstablishmentNumber,EstablishmentName
                           100,""School with, comma""
                           200,School with ""quotes""";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new EstablishmentCsvFileRepository(csvPath);

        var results = repository.GetAll().ToList();

        results.Should().HaveCount(2);
        results[0].EstablishmentName.Should().Be("\"School with, comma\"");
        results[1].EstablishmentName.Should().Be("School with \"quotes\"");
    }

    [Fact]
    public void GetAll_WithExtraColumns_ShouldIgnoreExtraColumns()
    {
        var csvContent = @"EstablishmentNumber,EstablishmentName,ExtraColumn
                           100,Test School,Extra Data";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new EstablishmentCsvFileRepository(csvPath);

        var results = repository.GetAll().ToList();

        results.Should().ContainSingle();
        results[0].EstablishmentNumber.Should().Be(100);
        results[0].EstablishmentName.Should().Be("Test School");
    }

    [Fact]
    public void GetAll_WithFileDeletedAfterConstruction_ShouldReturnEmpty()
    {
        var csvContent = @"EstablishmentNumber,EstablishmentName
                           100,Test School";
        var csvPath = CreateTestCsvFile(csvContent);
        var repository = new EstablishmentCsvFileRepository(csvPath);
        File.Delete(csvPath);

        var results = repository.GetAll().ToList();

        results.Should().BeEmpty();
    }
}
