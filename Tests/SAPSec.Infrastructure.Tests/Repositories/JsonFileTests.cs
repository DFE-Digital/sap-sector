using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SAPSec.Infrastructure.Json;

namespace SAPSec.Infrastructure.Tests.Repositories
{
    public class TestEntity
    {
        public string Id { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class JsonFileTests : IDisposable
    {
        private readonly string _dataDir;
        private readonly string _filesDir;

        public JsonFileTests()
        {
            // JSONRepository uses AppContext.BaseDirectory + "Data/Files"
            _dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
            _filesDir = Path.Combine(_dataDir, "Files");
            Directory.CreateDirectory(_filesDir);
        }

        public void Dispose()
        {
            // Cleanup any test files we might have created (do not remove existing unrelated files)
            try
            {
                foreach (var file in Directory.EnumerateFiles(_filesDir, "TestEntity*.json"))
                {
                    File.Delete(file);
                }
            }
            catch
            {
                // best-effort cleanup, swallow errors to avoid hiding test results
            }
        }

        [Fact]
        public async Task ReadAll_ReturnsEntities_WhenFileExists_AndContainsValidJson()
        {
            // Arrange
            var items = new[]
            {
                new TestEntity { Id = "111", Value = "One" },
                new TestEntity { Id = "222", Value = "Two" }
            };

            var filePath = Path.Combine(_filesDir, "TestEntity.json");
            File.WriteAllText(filePath, JsonConvert.SerializeObject(items));

            var logger = new Mock<ILogger<JsonFile<TestEntity>>>();
            var repo = new JsonFile<TestEntity>(logger.Object);

            // Act
            var result = await repo.ReadAllAsync();

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, l => l.Id == "111");
            Assert.Contains(list, l => l.Id == "222");
        }

        [Fact]
        public async Task ReadAll_ReturnsEmpty_WhenFileDoesNotExist()
        {
            // Arrange
            var filePath = Path.Combine(_filesDir, "TestEntity.json");
            if (File.Exists(filePath)) File.Delete(filePath);

            var logger = new Mock<ILogger<JsonFile<TestEntity>>>();
            var repo = new JsonFile<TestEntity>(logger.Object);

            // Act
            var result = await repo.ReadAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ReadAll_ReturnsEmpty_WhenFileContainsInvalidJson()
        {
            // Arrange
            var filePath = Path.Combine(_filesDir, "TestEntity.json");
            File.WriteAllText(filePath, "{ invalid-json ");

            var logger = new Mock<ILogger<JsonFile<TestEntity>>>();
            var repo = new JsonFile<TestEntity>(logger.Object);

            // Act
            var result = await repo.ReadAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void Ctor_ThrowsArgumentNullException_WhenLoggerIsNull()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new JsonFile<TestEntity>(null!));
        }
    }


}
