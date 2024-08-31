using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MaksIT.MongoDB.Linq.Abstractions {
  public abstract class DataProviderBase<T> {
    protected readonly ILogger<T> Logger;
    protected readonly IMongoDatabase Database;
    private readonly IMongoClient _client;

    protected DataProviderBase(
        ILogger<T> logger,
        IMongoClient client,
        string databaseName) {
      Logger = logger;
      _client = client;
      Database = _client.GetDatabase(databaseName);
    }
  }
}
