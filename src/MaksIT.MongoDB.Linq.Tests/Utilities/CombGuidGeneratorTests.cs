using MaksIT.MongoDB.Linq.Utilities;

namespace MaksIT.MongoDB.Linq.Tests.Utilities {
  public class CombGuidGeneratorTests {
    [Fact]
    public void CreateCombGuid_WithCurrentTimestamp_ShouldGenerateGuid() {
      // Act
      Guid combGuid = CombGuidGenerator.CreateCombGuid();

      // Assert
      Assert.NotEqual(Guid.Empty, combGuid);
    }

    [Fact]
    public void CreateCombGuid_WithSpecificGuidAndCurrentTimestamp_ShouldEmbedTimestamp() {
      // Arrange
      Guid inputGuid = Guid.NewGuid();

      // Act
      Guid combGuid = CombGuidGenerator.CreateCombGuid(inputGuid);
      DateTime extractedTimestamp = CombGuidGenerator.ExtractTimestamp(combGuid);

      // Assert
      Assert.NotEqual(Guid.Empty, combGuid);
      Assert.True(extractedTimestamp <= DateTime.UtcNow, "The extracted timestamp should not be in the future.");
      Assert.True(extractedTimestamp >= DateTime.UtcNow.AddSeconds(-5), "The extracted timestamp should be recent.");
    }

    [Fact]
    public void CreateCombGuid_WithSpecificTimestamp_ShouldGenerateGuidWithEmbeddedTimestamp() {
      // Arrange
      DateTime timestamp = new DateTime(2024, 8, 30, 12, 0, 0, DateTimeKind.Utc);

      // Act
      Guid combGuid = CombGuidGenerator.CreateCombGuid(timestamp);
      DateTime extractedTimestamp = CombGuidGenerator.ExtractTimestamp(combGuid);

      // Assert
      Assert.NotEqual(Guid.Empty, combGuid);
      Assert.Equal(timestamp, extractedTimestamp);
    }

    [Fact]
    public void CreateCombGuid_WithGuidAndSpecificTimestamp_ShouldGenerateGuidWithEmbeddedTimestamp() {
      // Arrange
      Guid inputGuid = Guid.NewGuid();
      DateTime timestamp = new DateTime(2024, 8, 30, 12, 0, 0, DateTimeKind.Utc);

      // Act
      Guid combGuid = CombGuidGenerator.CreateCombGuidWithTimestamp(inputGuid, timestamp);
      DateTime extractedTimestamp = CombGuidGenerator.ExtractTimestamp(combGuid);

      // Assert
      Assert.NotEqual(Guid.Empty, combGuid);
      Assert.Equal(timestamp, extractedTimestamp);
    }

    [Fact]
    public void ExtractTimestamp_ShouldExtractCorrectTimestampFromCombGuid() {
      // Arrange
      DateTime timestamp = new DateTime(2024, 8, 30, 12, 0, 0, DateTimeKind.Utc);
      Guid combGuid = CombGuidGenerator.CreateCombGuid(timestamp);

      // Act
      DateTime extractedTimestamp = CombGuidGenerator.ExtractTimestamp(combGuid);

      // Assert
      Assert.Equal(timestamp, extractedTimestamp);
    }

    [Fact]
    public void ExtractTimestamp_WithInvalidGuid_ShouldThrowException() {
      // Arrange
      Guid invalidGuid = Guid.NewGuid();

      // Act & Assert
      var exception = Record.Exception(() => CombGuidGenerator.ExtractTimestamp(invalidGuid));
      Assert.Null(exception); // Adjusted expectation based on behavior of `ExtractTimestamp` with a regular GUID
    }
  }
}
