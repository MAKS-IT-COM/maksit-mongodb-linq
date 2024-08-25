using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MaksIT.MongoDB.Linq {
  public interface IMongoSessionManager {
    Task<DisposableMongoSession> GetOrCreateSessionAsync();
    void ReleaseSession(DisposableMongoSession session);
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

    public async Task<DisposableMongoSession> GetOrCreateSessionAsync() {
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

    public void ReleaseSession(DisposableMongoSession session) {
      if (_sessions.TryRemove(session.SessionId, out var _)) {
        _logger.LogInformation("Releasing and disposing session with ID: {SessionId}", session.SessionId);
        session.Dispose();
      }
      else {
        _logger.LogWarning("Failed to find session with ID: {SessionId} for release.", session.SessionId);
      }
    }
  }
}
