using System.Linq.Expressions;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;
using MongoDB.Bson.Serialization;

using MaksIT.MongoDBLinq.Abstractions.Domain;

using MaksIT.Results;

namespace MaksIT.Vault.Abstractions {

  public abstract class CollectionDataProviderBase<T, TDomainDocument> : BaseCollectionDataProviderBase<T, TDomainDocument> where TDomainDocument : DtoDocumentBase {

    protected CollectionDataProviderBase(
        ILogger<T> logger,
        IMongoClient client,
        IIdGenerator idGenerator,
        string databaseName,
        string collectionName
    ) : base(logger, client, idGenerator, databaseName, collectionName) { }

    #region Insert
    public Result<Guid?> Insert(TDomainDocument obj, IClientSessionHandle? session) =>
        InsertAsync(obj, session).Result;
    #endregion

    #region InsertMany
    public Result<List<Guid>?> InsertMany(List<TDomainDocument> objList, IClientSessionHandle? session) =>
        InsertManyAsync(objList, session).Result;
    #endregion

    #region Count
    protected Result<int?> CountWithPredicate(Expression<Func<TDomainDocument, bool>> predicate) =>
        CountWithPredicate(new List<Expression<Func<TDomainDocument, bool>>> { predicate });

    private protected Result<int?> CountWithPredicate(List<Expression<Func<TDomainDocument, bool>>> predicates) {
      try {
        var query = GetWithPredicate(predicates);

        var result = query.Count();

        return Result<int?>.Ok(result);
      }
      catch (Exception ex) {
        Logger.LogError(ex, _errorMessage);
        return Result<int?>.InternalServerError(null, _errorMessage);
      }
    }
    #endregion

    #region Get
    protected Result<List<TResult>?> GetWithPredicate<TResult>(Expression<Func<TDomainDocument, bool>> predicate, Expression<Func<TDomainDocument, TResult>> selector) =>
        GetWithPredicate(new List<Expression<Func<TDomainDocument, bool>>> { predicate }, selector, null, null);

    protected Result<List<TResult>?> GetWithPredicate<TResult>(Expression<Func<TDomainDocument, bool>> predicate, Expression<Func<TDomainDocument, TResult>> selector, int? skip, int? limit) =>
        GetWithPredicate(new List<Expression<Func<TDomainDocument, bool>>> { predicate }, selector, skip, limit);

    protected Result<List<TResult>?> GetWithPredicate<TResult>(List<Expression<Func<TDomainDocument, bool>>> predicates, Expression<Func<TDomainDocument, TResult>> selector, int? skip, int? limit) {
      try {
        var query = GetWithPredicate(predicates).Select(selector);

        if (skip != null)
          query = query.Skip(skip.Value);

        if (limit != null)
          query = query.Take(limit.Value);

        var result = query.ToList();

        return result.Count > 0
            ? Result<List<TResult>?>.Ok(result)
            : Result<List<TResult>?>.NotFound(null);
      }
      catch (Exception ex) {
        Logger.LogError(ex, _errorMessage);
        return Result<List<TResult>?>.InternalServerError(null, _errorMessage);
      }
    }

    protected IQueryable<TDomainDocument> GetWithPredicate(List<Expression<Func<TDomainDocument, bool>>> predicates) {
      var query = GetQuery();

      foreach (var predicate in predicates)
        query = query.Where(predicate);

      return query;
    }
    #endregion

    #region Update
    protected Result<Guid?> UpdateWithPredicate(TDomainDocument obj, Expression<Func<TDomainDocument, bool>> predicate, IClientSessionHandle? session) =>
        UpdateWithPredicateAsync(obj, predicate, session).Result;
    #endregion

    #region UpdateMany
    public Result<List<Guid>?> UpdateManyWithPredicate(Expression<Func<TDomainDocument, bool>> predicate, List<TDomainDocument> objList, IClientSessionHandle? session) =>
        UpdateManyWithPredicateAsync(objList, predicate, session).Result;
    #endregion

    #region Upsert
    protected Result<Guid?> UpsertWithPredicate(TDomainDocument obj, Expression<Func<TDomainDocument, bool>> predicate, IClientSessionHandle? session) =>
        UpsertWithPredicateAsync(obj, predicate, session).Result;
    #endregion

    #region UpsertMany
    public Result<List<Guid>?> UpsertManyWithPredicate(List<TDomainDocument> objList, Expression<Func<TDomainDocument, bool>> predicate, IClientSessionHandle? session) =>
        UpsertManyWithPredicateAsync(objList, predicate, session).Result;
    #endregion

    #region Delete
    protected Result DeleteWithPredicate(Expression<Func<TDomainDocument, bool>> predicate, IClientSessionHandle? session) =>
        DeleteWithPredicateAsync(predicate, session).Result;
    #endregion

    #region DeleteMany
    protected Result DeleteManyWithPredicate(Expression<Func<TDomainDocument, bool>> predicate, IClientSessionHandle? session) =>
        DeleteManyWithPredicateAsync(predicate, session).Result;
    #endregion
  }
}
