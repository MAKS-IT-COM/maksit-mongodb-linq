using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using MaksIT.Core.Abstractions.Dto;
using MaksIT.Results;


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
        // Step 1: Find the documents that already exist based on the predicate
        List<TDtoDocument> existingDocuments;
        if (session != null) {
          existingDocuments = await Collection.Find(session, predicate).ToListAsync();
        }
        else {
          existingDocuments = await Collection.Find(predicate).ToListAsync();
        }

        // Step 2: Get the existing document IDs
        var existingIds = existingDocuments.Select(doc => doc.Id).ToHashSet();

        // Step 3: Filter the documents to update only those that exist in the collection
        var documentsToUpdate = documents.Where(doc => existingIds.Contains(doc.Id)).ToList();

        // Step 4: Update each of the existing documents
        foreach (var document in documentsToUpdate) {
          // Handling nullable Id by checking both document.Id and x.Id for null
          var documentPredicate = (Expression<Func<TDtoDocument, bool>>)(x =>
              (x.Id == null && document.Id == null) ||
              (x.Id != null && x.Id.Equals(document.Id)));

          if (session != null) {
            await Collection.ReplaceOneAsync(session, documentPredicate, document);
          }
          else {
            await Collection.ReplaceOneAsync(documentPredicate, document);
          }
        }

        var updatedIds = documentsToUpdate.Select(doc => doc.Id).ToList();
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
        // Deletion
        if (session != null)
          await Collection.DeleteManyAsync(session, predicate);
        else
          await Collection.DeleteManyAsync(predicate);

        // Creation
        if (session != null)
          await Collection.InsertManyAsync(session, documents);
        else
          await Collection.InsertManyAsync(documents);
 

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
