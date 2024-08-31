using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MaksIT.Results;

namespace MaksIT.MongoDB.Linq {
  public interface IMongoSessionManager {
    Result ExecuteInSession(Func<IClientSessionHandle, Result> action);
    Task<Result> ExecuteInSessionAsync(Func<IClientSessionHandle, Task<Result>> action);
    Result<T> ExecuteInSession<T>(Func<IClientSessionHandle, Result<T>> action);
    Task<Result<T>> ExecuteInSessionAsync<T>(Func<IClientSessionHandle, Task<Result<T>>> action);
  }

  public class MongoSessionManager : IMongoSessionManager {
    private readonly IMongoClient _client;
    private readonly ILogger<MongoSessionManager> _logger;
    private readonly ConcurrentDictionary<string, DisposableMongoSession> _sessions;

    public MongoSessionManager(ILogger<MongoSessionManager> logger, IMongoClient client) {
      _logger = logger;
      _client = client;
      _sessions = new ConcurrentDictionary<string, DisposableMongoSession>();
    }

    private async Task<DisposableMongoSession> GetOrCreateSessionAsync() {
      // Generate a unique session ID
      var sessionId = Guid.NewGuid().ToString();

      if (_sessions.TryGetValue(sessionId, out var existingSession)) {
        _logger.LogInformation("Reusing existing session with ID: {SessionId}", sessionId);
        return existingSession;
      }

      _logger.LogInformation("Creating a new session with ID: {SessionId}", sessionId);
      var sessionHandle = await _client.StartSessionAsync();
      var newSession = new DisposableMongoSession(sessionHandle, sessionId);

      if (_sessions.TryAdd(sessionId, newSession)) {
        return newSession;
      }

      _logger.LogError("Failed to add session with ID: {SessionId}", sessionId);
      newSession.Dispose();
      throw new InvalidOperationException("Failed to create or retrieve session.");
    }

    private void ReleaseSession(DisposableMongoSession session) {
      if (_sessions.TryRemove(session.SessionId, out var _)) {
        _logger.LogInformation("Releasing and disposing session with ID: {SessionId}", session.SessionId);
        session.Dispose();
      }
      else {
        _logger.LogWarning("Failed to find session with ID: {SessionId} for release.", session.SessionId);
      }
    }

    public Result ExecuteInSession(Func<IClientSessionHandle, Result> action) {
      using var mongoSession = GetOrCreateSessionAsync().Result;

      try {
        mongoSession.Session.StartTransaction();
        var result = action(mongoSession.Session);

        if (!result.IsSuccess) {
          mongoSession.Session.AbortTransaction();
        }
        else {
          mongoSession.Session.CommitTransaction();
        }

        return result;
      }
      catch (Exception ex) {
        _logger.LogError(ex, "Error during session operation.");
        return Result.InternalServerError("An error occurred during the operation.");
      }
      finally {
        ReleaseSession(mongoSession);
        _logger.LogInformation("Session released for session ID: {SessionId}", mongoSession.SessionId);
      }
    }

    public async Task<Result> ExecuteInSessionAsync(Func<IClientSessionHandle, Task<Result>> action) {
      using var mongoSession = await GetOrCreateSessionAsync();

      try {
        mongoSession.Session.StartTransaction();
        var result = await action(mongoSession.Session);

        if (!result.IsSuccess) {
          await mongoSession.Session.AbortTransactionAsync();
        }
        else {
          await mongoSession.Session.CommitTransactionAsync();
        }

        return result;
      }
      catch (Exception ex) {
        _logger.LogError(ex, "Error during async session operation.");
        return Result.InternalServerError("An error occurred during the async operation.");
      }
      finally {
        ReleaseSession(mongoSession);
        _logger.LogInformation("Session released for session ID: {SessionId}", mongoSession.SessionId);
      }
    }

    public Result<T> ExecuteInSession<T>(Func<IClientSessionHandle, Result<T>> action) {
      using var mongoSession = GetOrCreateSessionAsync().Result;

      try {
        // Start the transaction
        mongoSession.Session.StartTransaction();

        var result = action(mongoSession.Session);

        if (!result.IsSuccess)
          // Abort the transaction if the action failed
          mongoSession.Session.AbortTransaction();

        // Commit the transaction if the action was successful
        mongoSession.Session.CommitTransaction();

        return result;
      }
      catch (Exception ex) {
        _logger.LogError(ex, "Error during session operation.");
        return Result<T>.InternalServerError(default, "An error occurred during the operation.");
      }
      finally {
        ReleaseSession(mongoSession);
        _logger.LogInformation("Session released for session ID: {SessionId}", mongoSession.SessionId);
      }
    }

    public async Task<Result<T>> ExecuteInSessionAsync<T>(Func<IClientSessionHandle, Task<Result<T>>> action) {
      using var mongoSession = await GetOrCreateSessionAsync();

      try {
        // Start the transaction
        mongoSession.Session.StartTransaction();

        // Execute the action within the session
        var result = await action(mongoSession.Session);

        if (!result.IsSuccess)
          // Abort the transaction if the action failed
          await mongoSession.Session.AbortTransactionAsync();

        // Commit the transaction if the action was successful
        await mongoSession.Session.CommitTransactionAsync();

        return result;
      }
      catch (Exception ex) {
        _logger.LogError(ex, "Error during async session operation.");
        return Result<T>.InternalServerError(default, "An error occurred during the async operation.");
      }
      finally {
        ReleaseSession(mongoSession);
        _logger.LogInformation("Session released for session ID: {SessionId}", mongoSession.SessionId);
      }
    }
  }
}
