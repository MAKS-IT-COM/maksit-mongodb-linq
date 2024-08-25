using System.Linq.Expressions;

using Microsoft.Extensions.Logging;

using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MaksIT.MongoDBLinq.Abstractions;
using MaksIT.MongoDBLinq.Abstractions.Domain;

using MaksIT.Results;


namespace MaksIT.Vault.Abstractions {

  public abstract class BaseCollectionDataProviderBase<T, TDomainDocument> : DataProviderBase<T> where TDomainDocument : DtoDocumentBase {

    protected readonly IIdGenerator IdGenerator;
    protected readonly IMongoCollection<TDomainDocument> Collection;

    protected readonly string _errorMessage = "MaksIT.MongoDB.Linq - Data provider error";

    protected BaseCollectionDataProviderBase(
        ILogger<T> logger,
        IMongoClient client,
        IIdGenerator idGenerator,
        string databaseName,
        string collectionName
    ) : base(logger, client, databaseName) {
      IdGenerator = idGenerator;

      if (!Database.ListCollectionNames().ToList().Contains(collectionName))
        Database.CreateCollection(collectionName);

      Collection = Database.GetCollection<TDomainDocument>(collectionName);
    }

    #region Insert
    protected virtual async Task<Result<Guid?>> InsertAsync(TDomainDocument document, IClientSessionHandle? session) {
      try {
        if (session != null)
          await Collection.InsertOneAsync(session, document);
        else
          await Collection.InsertOneAsync(document);

        return Result<Guid?>.Ok(document.Id);
      }
      catch (Exception ex) {
        Logger.LogError(ex, _errorMessage);
        return Result<Guid?>.InternalServerError(null, _errorMessage);
      }
    }
    #endregion

    #region InsertMany
    protected virtual async Task<Result<List<Guid>?>> InsertManyAsync(List<TDomainDocument> documents, IClientSessionHandle? session) {
      try {
        if (session != null)
          await Collection.InsertManyAsync(session, documents);
        else
          await Collection.InsertManyAsync(documents);

        return Result<List<Guid>?>.Ok(documents.Select(x => x.Id).ToList());
      }
      catch (Exception ex) {
        Logger.LogError(ex, _errorMessage);
        return Result<List<Guid>?>.InternalServerError(null, _errorMessage);
      }
    }
    #endregion

    #region Get
    protected virtual IQueryable<TDomainDocument> GetQuery() => Collection.AsQueryable();
    #endregion

    #region Update
    protected virtual async Task<Result<Guid?>> UpdateWithPredicateAsync(TDomainDocument document, Expression<Func<TDomainDocument, bool>> predicate, IClientSessionHandle? session) {
      try {
        if (session != null)
          await Collection.ReplaceOneAsync(session, predicate, document);
        else
          await Collection.ReplaceOneAsync(predicate, document);

        return Result<Guid?>.Ok(document.Id);
      }
      catch (Exception ex) {
        Logger.LogError(ex, _errorMessage);
        return Result<Guid?>.InternalServerError(null, _errorMessage);
      }
    }
    #endregion

    #region UdateMany
    protected virtual async Task<Result<List<Guid>?>> UpdateManyWithPredicateAsync(List<TDomainDocument> documents, Expression<Func<TDomainDocument, bool>> predicate, IClientSessionHandle? session) {
      try {
        var tasks = new List<Task<ReplaceOneResult>>();

        foreach (var document in documents) {
          var filter = Builders<TDomainDocument>.Filter.Where(predicate);
          var updateOptions = new ReplaceOptions { IsUpsert = false };

          if (session != null)
            tasks.Add(Collection.ReplaceOneAsync(session, filter, document, updateOptions));
          else
            tasks.Add(Collection.ReplaceOneAsync(filter, document, updateOptions));
        }

        await Task.WhenAll(tasks);

        var upsertedIds = documents.Select(doc => doc.Id).ToList();
        return Result<List<Guid>?>.Ok(upsertedIds);
      }
      catch (Exception ex) {
        Logger.LogError(ex, _errorMessage);
        return Result<List<Guid>?>.InternalServerError(null, _errorMessage);
      }
    }
    #endregion

    #region Upsert
    protected virtual async Task<Result<Guid?>> UpsertWithPredicateAsync(TDomainDocument documents, Expression<Func<TDomainDocument, bool>> predicate, IClientSessionHandle? session) {
      try {
        var updateOptions = new ReplaceOptions {
          IsUpsert = true
        };

        if (session != null)
          await Collection.ReplaceOneAsync(session, predicate, documents, updateOptions);
        else
          await Collection.ReplaceOneAsync(predicate, documents, updateOptions);

        return Result<Guid?>.Ok(documents.Id);
      }
      catch (Exception ex) {
        Logger.LogError(ex, _errorMessage);
        return Result<Guid?>.InternalServerError(null, _errorMessage);
      }
    }
    #endregion

    #region UpsertMany
    protected virtual async Task<Result<List<Guid>?>> UpsertManyWithPredicateAsync(List<TDomainDocument> documents, Expression<Func<TDomainDocument, bool>> predicate, IClientSessionHandle? session) {
      try {
        var tasks = new List<Task<ReplaceOneResult>>();

        foreach (var document in documents) {
          var filter = Builders<TDomainDocument>.Filter.Where(predicate);
          var updateOptions = new ReplaceOptions { IsUpsert = true };

          if (session != null)
            tasks.Add(Collection.ReplaceOneAsync(session, filter, document, updateOptions));
          else
            tasks.Add(Collection.ReplaceOneAsync(filter, document, updateOptions));
        }

        await Task.WhenAll(tasks);

        var upsertedIds = documents.Select(doc => doc.Id).ToList();
        return Result<List<Guid>?>.Ok(upsertedIds);
      }
      catch (Exception ex) {
        Logger.LogError(ex, _errorMessage);
        return Result<List<Guid>?>.InternalServerError(null, _errorMessage);
      }
    }
    #endregion

    #region Delete
    protected virtual async Task<Result> DeleteWithPredicateAsync(Expression<Func<TDomainDocument, bool>> predicate, IClientSessionHandle? session) {
      try {
        if (session != null)
          await Collection.DeleteOneAsync(session, predicate);
        else
          await Collection.DeleteOneAsync(predicate);

        return Result.Ok();
      }
      catch (Exception ex) {
        Logger.LogError(ex, _errorMessage);
        return Result.InternalServerError(_errorMessage);
      }
    }
    #endregion

    #region DeleteMany
    protected virtual async Task<Result> DeleteManyWithPredicateAsync(Expression<Func<TDomainDocument, bool>> predicate, IClientSessionHandle? session) {
      try {
        if (session != null)
          await Collection.DeleteManyAsync(session, predicate);
        else
          await Collection.DeleteManyAsync(predicate);

        return Result.Ok();
      }
      catch (Exception ex) {
        Logger.LogError(ex, _errorMessage);
        return Result.InternalServerError(_errorMessage);
      }
    }
    #endregion
  }
}
