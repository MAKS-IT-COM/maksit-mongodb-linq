﻿using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

using MaksIT.MongoDB.Linq.Abstractions.Domain;
using MaksIT.Results;
using MongoDB.Bson.Serialization;

namespace MaksIT.MongoDB.Linq.Abstractions {
  public abstract class BaseCollectionDataProviderBase<T, TDtoDocument, TDtoKey> : DataProviderBase<T>
      where TDtoDocument : DtoDocumentBase<TDtoKey> {

    protected readonly IMongoCollection<TDtoDocument> Collection;
    protected readonly string _errorMessage = "MaksIT.MongoDB.Linq - Data provider error";

    protected BaseCollectionDataProviderBase(
        ILogger<T> logger,
        IMongoClient client,
        string databaseName,
        string collectionName
    ) : base(logger, client, databaseName) {
      if (!Database.ListCollectionNames().ToList().Contains(collectionName))
        Database.CreateCollection(collectionName);

      Collection = Database.GetCollection<TDtoDocument>(collectionName);
    }

    #region Insert
    protected virtual async Task<Result<TDtoKey?>> InsertAsync(TDtoDocument document, IClientSessionHandle? session) {
      try {
        if (session != null)
          await Collection.InsertOneAsync(session, document);
        else
          await Collection.InsertOneAsync(document);

        return Result<TDtoKey?>.Ok(document.Id);
      }
      catch (Exception ex) {
        Logger.LogError(ex, _errorMessage);
        return Result<TDtoKey?>.InternalServerError(default(TDtoKey?), _errorMessage);
      }
    }
    #endregion

    #region InsertMany
    protected virtual async Task<Result<List<TDtoKey>?>> InsertManyAsync(List<TDtoDocument> documents, IClientSessionHandle? session) {
      try {
        if (session != null)
          await Collection.InsertManyAsync(session, documents);
        else
          await Collection.InsertManyAsync(documents);

        return Result<List<TDtoKey>?>.Ok(documents.Select(x => x.Id).ToList());
      }
      catch (Exception ex) {
        Logger.LogError(ex, _errorMessage);
        return Result<List<TDtoKey>?>.InternalServerError(default, _errorMessage);
      }
    }
    #endregion

    #region Get
    protected virtual IQueryable<TDtoDocument> GetQuery() => Collection.AsQueryable();
    #endregion

    #region Update
    protected virtual async Task<Result<TDtoKey?>> UpdateWithPredicateAsync(
        TDtoDocument document,
        Expression<Func<TDtoDocument, bool>> predicate,
        IClientSessionHandle? session) {
      try {
        if (session != null)
          await Collection.ReplaceOneAsync(session, predicate, document);
        else
          await Collection.ReplaceOneAsync(predicate, document);

        return Result<TDtoKey?>.Ok(document.Id);
      }
      catch (Exception ex) {
        Logger.LogError(ex, _errorMessage);
        return Result<TDtoKey?>.InternalServerError(default(TDtoKey?), _errorMessage);
      }
    }
    #endregion

    #region UpdateMany
    protected virtual async Task<Result<List<TDtoKey>?>> UpdateManyWithPredicateAsync(
        List<TDtoDocument> documents,
        Expression<Func<TDtoDocument, bool>> predicate,
        IClientSessionHandle? session) {
      try {
        foreach (var document in documents) {
          if (session != null)
            await Collection.ReplaceOneAsync(session, predicate, document);
          else
            await Collection.ReplaceOneAsync(predicate, document);
        }

        var updatedIds = documents.Select(doc => doc.Id).ToList();
        return Result<List<TDtoKey>?>.Ok(updatedIds);
      }
      catch (Exception ex) {
        Logger.LogError(ex, _errorMessage);
        return Result<List<TDtoKey>?>.InternalServerError(default, _errorMessage);
      }
    }
    #endregion

    #region Upsert
    protected virtual async Task<Result<TDtoKey?>> UpsertWithPredicateAsync(
        TDtoDocument document,
        Expression<Func<TDtoDocument, bool>> predicate,
        IClientSessionHandle? session
    ) {
      try {
        if (session != null) {
          await Collection.DeleteOneAsync(session, predicate);
          await Collection.InsertOneAsync(session, document);
        }
        else {
          await Collection.DeleteOneAsync(predicate);
          await Collection.InsertOneAsync(document);
        }

        return Result<TDtoKey?>.Ok(document.Id);
      }
      catch (Exception ex) {
        Logger.LogError(ex, _errorMessage);
        return Result<TDtoKey?>.InternalServerError(default(TDtoKey?), _errorMessage);
      }
    }
    #endregion

    #region UpsertMany
    protected virtual async Task<Result<List<TDtoKey>?>> UpsertManyWithPredicateAsync(
        List<TDtoDocument> documents,
        Expression<Func<TDtoDocument, bool>> predicate,
        IClientSessionHandle? session) {
      try {
        foreach (var document in documents) {
          if (session != null) {
            await Collection.DeleteOneAsync(session, predicate);
            await Collection.InsertOneAsync(session, document);
          }
          else {
            await Collection.DeleteOneAsync(predicate);
            await Collection.InsertOneAsync(document);
          }
        }

        var upsertedIds = documents.Select(doc => doc.Id).ToList();
        return Result<List<TDtoKey>?>.Ok(upsertedIds);
      }
      catch (Exception ex) {
        Logger.LogError(ex, _errorMessage);
        return Result<List<TDtoKey>?>.InternalServerError(default, _errorMessage);
      }
    }
    #endregion

    #region Delete
    protected virtual async Task<Result> DeleteWithPredicateAsync(
        Expression<Func<TDtoDocument, bool>> predicate,
        IClientSessionHandle? session) {
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
    protected virtual async Task<Result> DeleteManyWithPredicateAsync(
        Expression<Func<TDtoDocument, bool>> predicate,
        IClientSessionHandle? session) {
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
