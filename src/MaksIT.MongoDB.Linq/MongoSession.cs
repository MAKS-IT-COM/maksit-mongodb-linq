using MongoDB.Driver;


namespace MaksIT.MongoDB.Linq {
  public class DisposableMongoSession : IDisposable {
    private readonly IClientSessionHandle _session;

    public DisposableMongoSession(IClientSessionHandle session, string sessionId) {
      _session = session;
      SessionId = sessionId;
    }

    public IClientSessionHandle Session => _session;

    public string SessionId { get; }

    public void Dispose() {
      _session?.Dispose();
    }
  }
}
