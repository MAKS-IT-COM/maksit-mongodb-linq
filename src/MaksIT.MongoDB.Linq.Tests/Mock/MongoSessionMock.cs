using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Clusters;

namespace MaksIT.MongoDB.Linq.Tests {
  internal class MongoSessionMock : IClientSessionHandle {

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

    public bool IsInTransaction { get; private set; } = false;


    public void StartTransaction(TransactionOptions transactionOptions = null) {
      IsInTransaction = true;
    }

    public Task StartTransactionAsync(TransactionOptions transactionOptions = null, CancellationToken cancellationToken = default) {
      IsInTransaction = true;
      return Task.CompletedTask;
    }

    public void AbortTransaction(CancellationToken cancellationToken = default) {
      IsInTransaction = false;
    }

    public Task AbortTransactionAsync(CancellationToken cancellationToken = default) {
      IsInTransaction = false;
      return Task.CompletedTask;
    }

    public void CommitTransaction(CancellationToken cancellationToken = default) {
      IsInTransaction = false;
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default) {
      IsInTransaction = false;
      return Task.CompletedTask;
    }

    #region not implemented

    public ClientSessionOptions Options => new ClientSessionOptions();

    public IMongoClient Client => throw new NotImplementedException();

    public ICluster Cluster => throw new NotImplementedException();

    public CoreSessionHandle WrappedCoreSession => throw new NotImplementedException();

    public BsonDocument ClusterTime => throw new NotImplementedException();

    public BsonDocument OperationTime => throw new NotImplementedException();

    public bool IsImplicit => throw new NotImplementedException();

    BsonTimestamp IClientSession.OperationTime => throw new NotImplementedException();

    public IServerSession ServerSession => throw new NotImplementedException();

    ICoreSessionHandle IClientSession.WrappedCoreSession => throw new NotImplementedException();

    public void Dispose() {
      // Simulate disposing of the session
    }

    public IClientSessionHandle Fork() {
      throw new NotImplementedException();
    }

    public void AdvanceClusterTime(BsonDocument newClusterTime) {
      throw new NotImplementedException();
    }

    public void AdvanceOperationTime(BsonTimestamp newOperationTime) {
      throw new NotImplementedException();
    }


    public TResult WithTransaction<TResult>(Func<IClientSessionHandle, CancellationToken, TResult> callback, TransactionOptions transactionOptions = null, CancellationToken cancellationToken = default) {
      throw new NotImplementedException();
    }


    public Task<TResult> WithTransactionAsync<TResult>(Func<IClientSessionHandle, CancellationToken, Task<TResult>> callbackAsync, TransactionOptions transactionOptions = null, CancellationToken cancellationToken = default) {
      throw new NotImplementedException();
    }

    #endregion

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
  }
}