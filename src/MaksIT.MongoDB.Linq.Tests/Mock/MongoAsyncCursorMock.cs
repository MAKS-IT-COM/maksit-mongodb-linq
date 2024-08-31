using MongoDB.Driver;

namespace MaksIT.MongoDB.Linq.Tests.Mock {
  internal class MongoAsyncCursorMock<T>(List<T> documents) : IAsyncCursor<T> {

    public IEnumerable<T> Current => documents;

    public bool MoveNext(CancellationToken cancellationToken = default) {
      return false; // Only one batch of data
    }

    public Task<bool> MoveNextAsync(CancellationToken cancellationToken = default) {
      return Task.FromResult(false);
    }

    public void Dispose() {

    }
  }
}
