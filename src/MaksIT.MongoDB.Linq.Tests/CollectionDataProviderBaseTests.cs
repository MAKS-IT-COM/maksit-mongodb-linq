using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using MaksIT.Results;
using MaksIT.Core.Abstractions.Dto;
using MaksIT.MongoDB.Linq.Abstractions;
using MaksIT.MongoDB.Linq.Utilities;
using MaksIT.MongoDB.Linq.Tests.Mock;


namespace MaksIT.MongoDB.Tests {

  // Sample DTO class for testing
  public class TestableDocumentDto : DtoDocumentBase<Guid> {
    public required string Name { get; set; }
  }

  // Generalized Testable class to simulate MongoDB operations using an in-memory list
  public class TestableCollectionDataProvider : BaseCollectionDataProviderBase<TestableCollectionDataProvider, TestableDocumentDto, Guid> {
    private readonly List<TestableDocumentDto> _inMemoryCollection;

    public TestableCollectionDataProvider(ILogger<TestableCollectionDataProvider> logger)
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        : base(logger, new MongoClientMock(), "TestDatabase", "TestCollection") {
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
      _inMemoryCollection = new List<TestableDocumentDto>();  // Initialize correctly
    }

    // Override protected methods to use in-memory list instead of a real database
    protected override IQueryable<TestableDocumentDto> GetQuery() => _inMemoryCollection.AsQueryable();

    protected override async Task<Result<Guid>> InsertAsync(TestableDocumentDto document, IClientSessionHandle? session) {
      _inMemoryCollection.Add(document);
      return await Task.FromResult(Result<Guid>.Ok(document.Id));
    }

    protected override async Task<Result<List<Guid>?>> InsertManyAsync(List<TestableDocumentDto> documents, IClientSessionHandle? session) {
      _inMemoryCollection.AddRange(documents);
      return await Task.FromResult(Result<List<Guid>?>.Ok(documents.Select(d => d.Id).ToList()));
    }

    protected override async Task<Result<Guid>> UpsertWithPredicateAsync(TestableDocumentDto document, Expression<Func<TestableDocumentDto, bool>> predicate, IClientSessionHandle? session) {
      var existingDocument = _inMemoryCollection.FirstOrDefault(predicate.Compile());
      if (existingDocument != null) {
        _inMemoryCollection.Remove(existingDocument);
      }
      _inMemoryCollection.Add(document);
      return await Task.FromResult(Result<Guid>.Ok(document.Id));
    }

    protected override async Task<Result<List<Guid>?>> UpsertManyWithPredicateAsync(List<TestableDocumentDto> documents, Expression<Func<TestableDocumentDto, bool>> predicate, IClientSessionHandle? session) {
      var existingDocuments = _inMemoryCollection.Where(predicate.Compile()).ToList();
      foreach (var doc in existingDocuments) {
        _inMemoryCollection.Remove(doc);
      }
      _inMemoryCollection.AddRange(documents);
      return await Task.FromResult(Result<List<Guid>?>.Ok(documents.Select(d => d.Id).ToList()));
    }

    protected override async Task<Result<Guid>> UpdateWithPredicateAsync(TestableDocumentDto document, Expression<Func<TestableDocumentDto, bool>> predicate, IClientSessionHandle? session) {
      var existingDocument = _inMemoryCollection.FirstOrDefault(predicate.Compile());
      if (existingDocument != null) {
        _inMemoryCollection.Remove(existingDocument);
        _inMemoryCollection.Add(document);
        return await Task.FromResult(Result<Guid>.Ok(document.Id));
      }
      return await Task.FromResult(Result<Guid>.InternalServerError(default(Guid), "Update failed"));
    }

    protected override async Task<Result<List<Guid>?>> UpdateManyWithPredicateAsync(List<TestableDocumentDto> documents, Expression<Func<TestableDocumentDto, bool>> predicate, IClientSessionHandle? session) {
      var existingDocuments = _inMemoryCollection.Where(predicate.Compile()).ToList();
      if (existingDocuments.Any()) {
        foreach (var doc in existingDocuments) {
          _inMemoryCollection.Remove(doc);
        }
        _inMemoryCollection.AddRange(documents);
        return await Task.FromResult(Result<List<Guid>?>.Ok(documents.Select(d => d.Id).ToList()));
      }
      return await Task.FromResult(Result<List<Guid>?>.InternalServerError(default, "UpdateMany failed"));
    }

    protected override async Task<Result> DeleteWithPredicateAsync(Expression<Func<TestableDocumentDto, bool>> predicate, IClientSessionHandle? session) {
      var documentToRemove = _inMemoryCollection.FirstOrDefault(predicate.Compile());
      if (documentToRemove != null) {
        _inMemoryCollection.Remove(documentToRemove);
        return await Task.FromResult(Result.Ok());
      }
      return await Task.FromResult(Result.InternalServerError("Delete failed"));
    }

    protected override async Task<Result> DeleteManyWithPredicateAsync(Expression<Func<TestableDocumentDto, bool>> predicate, IClientSessionHandle? session) {
      var documentsToRemove = _inMemoryCollection.Where(predicate.Compile()).ToList();
      if (documentsToRemove.Any()) {
        foreach (var doc in documentsToRemove) {
          _inMemoryCollection.Remove(doc);
        }
        return await Task.FromResult(Result.Ok());
      }
      return await Task.FromResult(Result.InternalServerError("DeleteMany failed"));
    }

    // Expose protected methods as public with different names for testing purposes
    public async Task<Result<Guid>> TestInsertAsync(TestableDocumentDto document, IClientSessionHandle? session = null) {
      return await InsertAsync(document, session);
    }

    public async Task<Result<List<Guid>?>> TestInsertManyAsync(List<TestableDocumentDto> documents, IClientSessionHandle? session = null) {
      return await InsertManyAsync(documents, session);
    }

    public async Task<Result<Guid>> TestUpsertWithPredicateAsync(TestableDocumentDto document, Expression<Func<TestableDocumentDto, bool>> predicate, IClientSessionHandle? session = null) {
      return await UpsertWithPredicateAsync(document, predicate, session);
    }

    public async Task<Result<List<Guid>?>> TestUpsertManyWithPredicateAsync(List<TestableDocumentDto> documents, Expression<Func<TestableDocumentDto, bool>> predicate, IClientSessionHandle? session = null) {
      return await UpsertManyWithPredicateAsync(documents, predicate, session);
    }

    public async Task<Result<Guid>> TestUpdateWithPredicateAsync(TestableDocumentDto document, Expression<Func<TestableDocumentDto, bool>> predicate, IClientSessionHandle? session = null) {
      return await UpdateWithPredicateAsync(document, predicate, session);
    }

    public async Task<Result<List<Guid>?>> TestUpdateManyWithPredicateAsync(List<TestableDocumentDto> documents, Expression<Func<TestableDocumentDto, bool>> predicate, IClientSessionHandle? session = null) {
      return await UpdateManyWithPredicateAsync(documents, predicate, session);
    }

    public async Task<Result> TestDeleteWithPredicateAsync(Expression<Func<TestableDocumentDto, bool>> predicate, IClientSessionHandle? session = null) {
      return await DeleteWithPredicateAsync(predicate, session);
    }

    public async Task<Result> TestDeleteManyWithPredicateAsync(Expression<Func<TestableDocumentDto, bool>> predicate, IClientSessionHandle? session = null) {
      return await DeleteManyWithPredicateAsync(predicate, session);
    }

    // Helper method to access the in-memory collection for validation
    public IQueryable<TestableDocumentDto> GetInMemoryCollection() {
      return _inMemoryCollection.AsQueryable();
    }
  }






  public class TestableCollectionDataProviderTests {
    private readonly TestableCollectionDataProvider _dataProvider;

    public TestableCollectionDataProviderTests() {
      // Set up a mock logger
      var logger = new LoggerFactory().CreateLogger<TestableCollectionDataProvider>();
      _dataProvider = new TestableCollectionDataProvider(logger);
    }

    [Fact]
    public async Task TestInsertAsync_ShouldReturnOkResult_WhenDocumentIsInserted() {
      // Arrange
      var document = new TestableDocumentDto {
        Id = CombGuidGenerator.CreateCombGuid(),
        Name = "Test Document"
      };

      // Act
      var result = await _dataProvider.TestInsertAsync(document, null);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(document.Id, result.Value);
    }

    [Fact]
    public async Task TestInsertManyAsync_ShouldReturnOkResult_WhenDocumentsAreInserted() {
      // Arrange
      var documents = new List<TestableDocumentDto>
      {
            new TestableDocumentDto { Id = CombGuidGenerator.CreateCombGuid(), Name = "Document 1" },
            new TestableDocumentDto { Id = CombGuidGenerator.CreateCombGuid(), Name = "Document 2" }
        };

      // Act
      var result = await _dataProvider.TestInsertManyAsync(documents, null);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(2, result.Value?.Count);
    }

    [Fact]
    public async Task TestUpsertWithPredicateAsync_ShouldInsertNewDocument_WhenNoMatchingDocumentExists() {
      // Arrange
      var document = new TestableDocumentDto { Id = CombGuidGenerator.CreateCombGuid(), Name = "Test Document" };

      // Act
      var result = await _dataProvider.TestUpsertWithPredicateAsync(document, x => x.Id == document.Id, null);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(document.Id, result.Value);
    }

    [Fact]
    public async Task TestUpsertWithPredicateAsync_ShouldUpdateExistingDocument_WhenMatchingDocumentExists() {
      // Arrange
      var document = new TestableDocumentDto { Id = CombGuidGenerator.CreateCombGuid(), Name = "Initial Document" };
      await _dataProvider.TestInsertAsync(document, null);

      var updatedDocument = new TestableDocumentDto { Id = document.Id, Name = "Updated Document" };

      // Act
      var result = await _dataProvider.TestUpsertWithPredicateAsync(updatedDocument, x => x.Id == document.Id, null);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(updatedDocument.Id, result.Value);

      var inMemoryCollection = _dataProvider.GetInMemoryCollection().FirstOrDefault(x => x.Id == updatedDocument.Id);
      Assert.NotNull(inMemoryCollection);
      Assert.Equal("Updated Document", inMemoryCollection.Name);
    }

    [Fact]
    public async Task TestUpsertManyWithPredicateAsync_ShouldInsertNewDocuments_WhenNoMatchingDocumentsExist() {
      // Arrange
      var documents = new List<TestableDocumentDto>
      {
            new TestableDocumentDto { Id = CombGuidGenerator.CreateCombGuid(), Name = "Document 1" },
            new TestableDocumentDto { Id = CombGuidGenerator.CreateCombGuid(), Name = "Document 2" }
        };

      // Act
      var result = await _dataProvider.TestUpsertManyWithPredicateAsync(documents, x => documents.Select(d => d.Id).Contains(x.Id), null);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(2, result.Value?.Count);
    }

    [Fact]
    public async Task TestUpdateWithPredicateAsync_ShouldUpdateDocument_WhenMatchingDocumentExists() {
      // Arrange
      var document = new TestableDocumentDto { Id = CombGuidGenerator.CreateCombGuid(), Name = "Initial Document" };
      await _dataProvider.TestInsertAsync(document, null);

      var updatedDocument = new TestableDocumentDto { Id = document.Id, Name = "Updated Document" };

      // Act
      var result = await _dataProvider.TestUpdateWithPredicateAsync(updatedDocument, x => x.Id == document.Id, null);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(updatedDocument.Id, result.Value);

      var inMemoryCollection = _dataProvider.GetInMemoryCollection().FirstOrDefault(x => x.Id == updatedDocument.Id);
      Assert.NotNull(inMemoryCollection);
      Assert.Equal("Updated Document", inMemoryCollection.Name);
    }

    [Fact]
    public async Task TestUpdateManyWithPredicateAsync_ShouldUpdateDocuments_WhenMatchingDocumentsExist() {
      // Arrange
      var documents = new List<TestableDocumentDto>
      {
            new TestableDocumentDto { Id = CombGuidGenerator.CreateCombGuid(), Name = "Document 1" },
            new TestableDocumentDto { Id = CombGuidGenerator.CreateCombGuid(), Name = "Document 2" }
        };
      await _dataProvider.TestInsertManyAsync(documents, null);

      var updatedDocuments = new List<TestableDocumentDto>
      {
            new TestableDocumentDto { Id = documents[0].Id, Name = "Updated Document 1" },
            new TestableDocumentDto { Id = documents[1].Id, Name = "Updated Document 2" }
        };

      // Act
      var result = await _dataProvider.TestUpdateManyWithPredicateAsync(updatedDocuments, x => updatedDocuments.Select(d => d.Id).Contains(x.Id), null);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(2, result.Value?.Count);

      var inMemoryCollection = _dataProvider.GetInMemoryCollection().ToList();
      Assert.Equal("Updated Document 1", inMemoryCollection[0].Name);
      Assert.Equal("Updated Document 2", inMemoryCollection[1].Name);
    }

    [Fact]
    public async Task TestDeleteWithPredicateAsync_ShouldDeleteDocument_WhenMatchingDocumentExists() {
      // Arrange
      var document = new TestableDocumentDto { Id = CombGuidGenerator.CreateCombGuid(), Name = "Test Document" };
      await _dataProvider.TestInsertAsync(document, null);

      // Act
      var result = await _dataProvider.TestDeleteWithPredicateAsync(x => x.Id == document.Id, null);

      // Assert
      Assert.True(result.IsSuccess);

      var inMemoryCollection = _dataProvider.GetInMemoryCollection().FirstOrDefault(x => x.Id == document.Id);
      Assert.Null(inMemoryCollection);
    }

    [Fact]
    public async Task TestDeleteManyWithPredicateAsync_ShouldDeleteDocuments_WhenMatchingDocumentsExist() {
      // Arrange
      var documents = new List<TestableDocumentDto>
      {
            new TestableDocumentDto { Id = CombGuidGenerator.CreateCombGuid(), Name = "Document 1" },
            new TestableDocumentDto { Id = CombGuidGenerator.CreateCombGuid(), Name = "Document 2" }
        };
      await _dataProvider.TestInsertManyAsync(documents, null);

      // Act
      var result = await _dataProvider.TestDeleteManyWithPredicateAsync(x => documents.Select(d => d.Id).Contains(x.Id), null);

      // Assert
      Assert.True(result.IsSuccess);

      var inMemoryCollection = _dataProvider.GetInMemoryCollection().ToList();
      Assert.Empty(inMemoryCollection);
    }

    [Fact]
    public async Task TestGetQuery_ShouldReturnCorrectDocuments() {
      // Arrange
      var documents = new List<TestableDocumentDto>
      {
            new TestableDocumentDto { Id = CombGuidGenerator.CreateCombGuid(), Name = "Document 1" },
            new TestableDocumentDto { Id = CombGuidGenerator.CreateCombGuid(), Name = "Document 2" }
        };

      // Use 'await' to asynchronously wait for the operation
      await _dataProvider.TestInsertManyAsync(documents, null);

      // Act
      var queryResult = _dataProvider.GetInMemoryCollection().ToList();

      // Assert
      Assert.Equal(2, queryResult.Count);
      Assert.Contains(queryResult, doc => doc.Name == "Document 1");
      Assert.Contains(queryResult, doc => doc.Name == "Document 2");
    }
  }





}