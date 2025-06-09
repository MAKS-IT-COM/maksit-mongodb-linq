using Microsoft.Extensions.Logging;

using MaksIT.Results;
using MaksIT.MongoDB.Linq.Tests.Mock;


namespace MaksIT.MongoDB.Linq.Tests;

public class MongoSessionManagerTests {

  private readonly MongoSessionManager _sessionManager;
  private readonly ILogger<MongoSessionManager> _logger;

  public MongoSessionManagerTests() {
    _logger = new LoggerFactory().CreateLogger<MongoSessionManager>();
    var mockClient = new MongoClientMock();
    _sessionManager = new MongoSessionManager(_logger, mockClient);
  }

  [Fact]
  public void ExecuteInSession_ShouldCommitTransaction_WhenActionSucceeds() {
    // Act
    var result = _sessionManager.ExecuteInSession(session => {
      // Simulate successful operation
      return Result.Ok();
    });

    // Assert
    Assert.True(result.IsSuccess);
  }

  [Fact]
  public void ExecuteInSession_ShouldAbortTransaction_WhenActionFails() {
    // Act
    var result = _sessionManager.ExecuteInSession(session => {
      // Simulate failed operation
      return Result.InternalServerError("Simulated failure");
    });

    // Assert
    Assert.False(result.IsSuccess);
  }

  [Fact]
  public async Task ExecuteInSessionAsync_ShouldCommitTransaction_WhenActionSucceeds() {
    // Act
    var result = await _sessionManager.ExecuteInSessionAsync(async session => {
      // Simulate successful operation
      return await Task.FromResult(Result.Ok());
    });

    // Assert
    Assert.True(result.IsSuccess);
  }

  [Fact]
  public async Task ExecuteInSessionAsync_ShouldAbortTransaction_WhenActionFails() {
    // Act
    var result = await _sessionManager.ExecuteInSessionAsync(async session => {
      // Simulate failed operation
      return await Task.FromResult(Result.InternalServerError("Simulated failure"));
    });

    // Assert
    Assert.False(result.IsSuccess);
  }
}
