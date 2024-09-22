using System.Linq.Expressions;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;
using MongoDB.Bson.Serialization;

using MaksIT.MongoDB.Linq.Abstractions.Domain;

using MaksIT.Results;

namespace MaksIT.MongoDB.Linq.Abstractions {

  public abstract class CollectionDataProviderBase<T, TDtoDocument, TDtoKey> : BaseCollectionDataProviderBase<T, TDtoDocument, TDtoKey>
    where TDtoDocument : DtoDocumentBase<TDtoKey> {

    protected CollectionDataProviderBase(
        ILogger<T> logger,
        IMongoClient client,
        string databaseName,
        string collectionName
    ) : base(logger, client, databaseName, collectionName) { }

    #region Insert
    public Result<TDtoKey?> Insert(TDtoDocument obj, IClientSessionHandle? session) =>
        InsertAsync(obj, session).Result;
    #endregion

    #region InsertMany
    public Result<List<TDtoKey>?> InsertMany(List<TDtoDocument> objList, IClientSessionHandle? session) =>
        InsertManyAsync(objList, session).Result;
    #endregion

    #region Count
    protected Result<int?> CountWithPredicate(Expression<Func<TDtoDocument, bool>> predicate) =>
        CountWithPredicate(new List<Expression<Func<TDtoDocument, bool>>> { predicate });

    private protected Result<int?> CountWithPredicate(List<Expression<Func<TDtoDocument, bool>>> predicates) {
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
    protected Result<List<TResult>?> GetWithPredicate<TResult>(Expression<Func<TDtoDocument, bool>> predicate, Expression<Func<TDtoDocument, TResult>> selector) =>
        GetWithPredicate(new List<Expression<Func<TDtoDocument, bool>>> { predicate }, selector, null, null);

    protected Result<List<TResult>?> GetWithPredicate<TResult>(Expression<Func<TDtoDocument, bool>> predicate, Expression<Func<TDtoDocument, TResult>> selector, int? skip, int? limit) =>
        GetWithPredicate(new List<Expression<Func<TDtoDocument, bool>>> { predicate }, selector, skip, limit);

    protected Result<List<TResult>?> GetWithPredicate<TResult>(
      List<Expression<Func<TDtoDocument, bool>>> predicates,
      Expression<Func<TDtoDocument, TResult>> selector,
      int? skip,
      int? limit
    ) {

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

    protected IQueryable<TDtoDocument> GetWithPredicate(List<Expression<Func<TDtoDocument, bool>>> predicates) {
      var query = GetQuery();

      foreach (var predicate in predicates)
        query = query.Where(predicate);

      return query;
    }
    #endregion

    #region Update
    protected Result<TDtoKey?> UpdateWithPredicate(TDtoDocument obj, Expression<Func<TDtoDocument, bool>> predicate, IClientSessionHandle? session) =>
        UpdateWithPredicateAsync(obj, predicate, session).Result;
    #endregion

    #region UpdateMany
    public Result<List<TDtoKey>?> UpdateManyWithPredicate(Expression<Func<TDtoDocument, bool>> predicate, List<TDtoDocument> objList, IClientSessionHandle? session) =>
        UpdateManyWithPredicateAsync(objList, predicate, session).Result;
    #endregion

    #region Upsert
    protected Result<TDtoKey?> UpsertWithPredicate(TDtoDocument obj, Expression<Func<TDtoDocument, bool>> predicate, IClientSessionHandle? session) =>
        UpsertWithPredicateAsync(obj, predicate, session).Result;
    #endregion

    #region UpsertMany
    public Result<List<TDtoKey>?> UpsertManyWithPredicate(List<TDtoDocument> objList, Expression<Func<TDtoDocument, bool>> predicate, IClientSessionHandle? session) =>
        UpsertManyWithPredicateAsync(objList, predicate, session).Result;
    #endregion

    #region Delete
    protected Result DeleteWithPredicate(Expression<Func<TDtoDocument, bool>> predicate, IClientSessionHandle? session) =>
        DeleteWithPredicateAsync(predicate, session).Result;
    #endregion

    #region DeleteMany
    protected Result DeleteManyWithPredicate(Expression<Func<TDtoDocument, bool>> predicate, IClientSessionHandle? session) =>
        DeleteManyWithPredicateAsync(predicate, session).Result;
    #endregion
  }
}
