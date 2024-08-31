# MaksIT.MongoDB.Linq

## Description

`MaksIT.MongoDB.Linq` is a .NET library designed to facilitate working with MongoDB using LINQ queries, providing a seamless and intuitive interface for developers to interact with MongoDB databases. The library abstracts common data access patterns, allowing for more efficient and readable code when performing CRUD operations, managing sessions, and handling transactions.

## Purpose

The primary goal of `MaksIT.MongoDB.Linq` is to simplify MongoDB integration in .NET applications by offering a robust, easy-to-use API. Whether you're managing complex queries, handling transactions, or implementing custom data providers, this library enables developers to focus on business logic while abstracting the intricacies of MongoDB operations.

## Key Features

- **LINQ Integration:** Query MongoDB collections using familiar LINQ syntax.
- **CRUD Operations:** Simplified methods for creating, reading, updating, and deleting documents.
- **Session and Transaction Management:** Built-in support for managing MongoDB sessions and transactions.
- **Custom Data Providers:** Extendable base classes to create your own data providers.
- **Error Handling:** Robust error handling with detailed logging using `Microsoft.Extensions.Logging`.
- **Support for Comb GUIDs:** Generate sortable GUIDs with timestamps for improved query performance.

## Installation

To include `MaksIT.MongoDB.Linq` in your .NET project, you can add the package via NuGet:

```shell
dotnet add package MaksIT.MongoDB.Linq
```

> Note: This library utilizes the `MaksIT.Results` library to standardize result handling and error management. `MaksIT.Results` provides a robust framework for handling operation outcomes with appropriate HTTP status codes and seamless conversion to `IActionResult` for ASP.NET Core applications.
>
>For more information about `MaksIT.Results`, visit the [GitHub repository](https://github.com/MAKS-IT-COM/maksit-results).

## Usage Examples

### Creating a Custom Data Provider

Below is an example of a custom data provider that demonstrates CRUD operations using `MaksIT.MongoDB.Linq`.

```csharp
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MaksIT.Vault.Abstractions;
using MaksIT.Core.Extensions; // Assuming this namespace contains the extension method ToNullable

public class OrganizationDataProvider : CollectionDataProviderBase<OrganizationDataProvider, OrganizationDto, Guid>, IOrganizationDataProvider
{
    public OrganizationDataProvider(
        ILogger<OrganizationDataProvider> logger,
        IMongoClient client,
        IIdGenerator idGenerator
    ) : base(logger, client, idGenerator, "maksit-vault", "organizations") { }

    // **Read** operation: Get a document by ID
    public Result<OrganizationDto?> GetById(Guid id) =>
        GetWithPredicate(x => x.Id == id, x => x, null, null)
            .WithNewValue(_ => _?.FirstOrDefault());

    // **Insert** operation: Insert a new document
    public Result<Guid?> Insert(OrganizationDto document, IClientSessionHandle? session = null) =>
        InsertAsync(document, session).Result
            .WithNewValue(_ => _.ToNullable());

    // **InsertMany** operation: Insert multiple documents
    public Result<List<Guid>?> InsertMany(List<OrganizationDto> documents, IClientSessionHandle? session = null) =>
        InsertManyAsync(documents, session).Result
            .WithNewValue(_ => _?.Select(id => id.ToNullable()).ToList());

    // **Update** operation: Update a document by a predicate
    public Result<Guid?> UpdateById(OrganizationDto document, IClientSessionHandle? session = null) =>
        UpdateWithPredicate(document, x => x.Id == document.Id, session)
            .WithNewValue(_ => _.ToNullable());

    // **UpdateMany** operation: Update multiple documents by a predicate
    public Result<List<Guid>?> UpdateManyById(List<OrganizationDto> documents, IClientSessionHandle? session = null) =>
        UpdateManyWithPredicate(x => documents.Select(y => y.Id).Contains(x.Id), documents, session)
            .WithNewValue(_ => _?.Select(id => id.ToNullable()).ToList());

    // **Upsert** operation: Insert or update a document by ID
    public Result<Guid?> UpsertById(OrganizationDto document, IClientSessionHandle? session = null) =>
        UpsertWithPredicate(document, x => x.Id == document.Id, session)
            .WithNewValue(_ => _.ToNullable());

    // **UpsertMany** operation: Insert or update multiple documents
    public Result<List<Guid>?> UpsertManyById(List<OrganizationDto> documents, IClientSessionHandle? session = null) =>
        UpsertManyWithPredicate(documents, x => documents.Select(y => y.Id).Contains(x.Id), session)
            .WithNewValue(_ => _?.Select(id => id.ToNullable()).ToList());

    // **Delete** operation: Delete a document by ID
    public Result DeleteById(Guid id, IClientSessionHandle? session = null) =>
        DeleteWithPredicate(x => x.Id == id, session);

    // **DeleteMany** operation: Delete multiple documents by ID
    public Result DeleteManyById(List<Guid> ids, IClientSessionHandle? session = null) =>
        DeleteManyWithPredicate(x => ids.Contains(x.Id), session);
}
```

### Performing CRUD Operations

#### Inserting a Document

```csharp
var document = new OrganizationDto
{
    Id = Guid.NewGuid(),
    Name = "My Organization"
};

var insertResult = organizationDataProvider.Insert(document);
if (insertResult.IsSuccess)
{
    Console.WriteLine($"Document inserted with ID: {insertResult.Value}");
}
```

#### Getting a Document by ID

```csharp
var id = Guid.Parse("your-document-id-here");
var getResult = organizationDataProvider.GetById(id);

if (getResult.IsSuccess)
{
    Console.WriteLine($"Document retrieved: {getResult.Value?.Name}");
}
else
{
    Console.WriteLine("Document not found.");
}
```

#### Updating a Document

```csharp
var documentToUpdate = new OrganizationDto
{
    Id = existingId,
    Name = "Updated Organization Name"
};

var updateResult = organizationDataProvider.UpdateById(documentToUpdate);
if (updateResult.IsSuccess)
{
    Console.WriteLine($"Document updated with ID: {updateResult.Value}");
}
```

#### Deleting a Document

```csharp
var deleteResult = organizationDataProvider.DeleteById(idToDelete);
if (deleteResult.IsSuccess)
{
    Console.WriteLine("Document deleted successfully.");
}
else
{
    Console.WriteLine("Failed to delete the document.");
}
```

Here is the section listing the available methods in `BaseCollectionDataProviderBase` and `CollectionDataProviderBase`:

### Available Methods

#### `BaseCollectionDataProviderBase<T, TDtoDocument, TDtoKey>`

- **Insert Operations**
  - `Task<Result<TDtoKey?>> InsertAsync(TDtoDocument document, IClientSessionHandle? session = null)`: Asynchronously inserts a new document.
  - `Task<Result<List<TDtoKey>?>> InsertManyAsync(List<TDtoDocument> documents, IClientSessionHandle? session = null)`: Asynchronously inserts multiple documents.

- **Update Operations**
  - `Task<Result<TDtoKey?>> UpdateWithPredicateAsync(TDtoDocument document, Expression<Func<TDtoDocument, bool>> predicate, IClientSessionHandle? session = null)`: Asynchronously updates a document matching the specified predicate.
  - `Task<Result<List<TDtoKey>?>> UpdateManyWithPredicateAsync(List<TDtoDocument> documents, Expression<Func<TDtoDocument, bool>> predicate, IClientSessionHandle? session = null)`: Asynchronously updates multiple documents matching the specified predicate.

- **Upsert Operations**
  - `Task<Result<TDtoKey?>> UpsertWithPredicateAsync(TDtoDocument document, Expression<Func<TDtoDocument, bool>> predicate, IClientSessionHandle? session = null)`: Asynchronously inserts or updates a document matching the specified predicate.
  - `Task<Result<List<TDtoKey>?>> UpsertManyWithPredicateAsync(List<TDtoDocument> documents, Expression<Func<TDtoDocument, bool>> predicate, IClientSessionHandle? session = null)`: Asynchronously inserts or updates multiple documents matching the specified predicate.

- **Delete Operations**
  - `Task<Result> DeleteWithPredicateAsync(Expression<Func<TDtoDocument, bool>> predicate, IClientSessionHandle? session = null)`: Asynchronously deletes documents matching the specified predicate.
  - `Task<Result> DeleteManyWithPredicateAsync(Expression<Func<TDtoDocument, bool>> predicate, IClientSessionHandle? session = null)`: Asynchronously deletes multiple documents matching the specified predicate.

#### `CollectionDataProviderBase<T, TDtoDocument, TDtoKey>`

- **Insert Operations**
  - `Result<TDtoKey?> Insert(TDtoDocument document, IClientSessionHandle? session = null)`: Inserts a new document.
  - `Result<List<TDtoKey>?> InsertMany(List<TDtoDocument> documents, IClientSessionHandle? session = null)`: Inserts multiple documents.

- **Read Operations**
  - `Result<List<TResult>?> GetWithPredicate<TResult>(Expression<Func<TDtoDocument, bool>> predicate, Expression<Func<TDtoDocument, TResult>> selector)`: Retrieves documents matching a predicate and projects them to a specified result type.
  - `Result<List<TResult>?> GetWithPredicate<TResult>(Expression<Func<TDtoDocument, bool>> predicate, Expression<Func<TDtoDocument, TResult>> selector, int? skip, int? limit)`: Retrieves documents matching a predicate, with optional skip and limit, and projects them to a specified result type.

- **Update Operations**
  - `Result<TDtoKey?> UpdateWithPredicate(TDtoDocument document, Expression<Func<TDtoDocument, bool>> predicate, IClientSessionHandle? session = null)`: Updates a document matching the specified predicate.
  - `Result<List<TDtoKey>?> UpdateManyWithPredicate(Expression<Func<TDtoDocument, bool>> predicate, List<TDtoDocument> documents, IClientSessionHandle? session = null)`: Updates multiple documents matching the specified predicate.

- **Upsert Operations**
  - `Result<TDtoKey?> UpsertWithPredicate(TDtoDocument document, Expression<Func<TDtoDocument, bool>> predicate, IClientSessionHandle? session = null)`: Inserts or updates a document matching the specified predicate.
  - `Result<List<TDtoKey>?> UpsertManyWithPredicate(List<TDtoDocument> documents, Expression<Func<TDtoDocument, bool>> predicate, IClientSessionHandle? session = null)`: Inserts or updates multiple documents matching the specified predicate.

- **Delete Operations**
  - `Result DeleteWithPredicate(Expression<Func<TDtoDocument, bool>> predicate, IClientSessionHandle? session = null)`: Deletes documents matching the specified predicate.
  - `Result DeleteManyWithPredicate(Expression<Func<TDtoDocument, bool>> predicate, IClientSessionHandle? session = null)`: Deletes multiple documents matching the specified predicate.

- **Count Operations**
  - `Result<int?> CountWithPredicate(Expression<Func<TDtoDocument, bool>> predicate)`: Counts documents matching a single predicate.
  - `Result<int?> CountWithPredicate(List<Expression<Func<TDtoDocument, bool>>> predicates)`: Counts documents matching multiple predicates.

These methods provide a comprehensive set of CRUD operations, allowing for flexible and powerful interaction with MongoDB databases through a LINQ-compatible interface.

No, the provided document did not mention the `CombGuidGenerator` utility from the `MaksIT.MongoDB.Linq.Utilities` namespace, which helps generate COMB GUIDs for collections. 

To improve the documentation, here's an additional section that introduces the `CombGuidGenerator` utility:

---

## Additional Utilities

### `CombGuidGenerator`

The `MaksIT.MongoDB.Linq` library also includes a utility class `CombGuidGenerator` for generating COMB GUIDs (COMBined Globally Unique Identifiers) that incorporate both randomness and a timestamp. This is particularly useful for databases where sorting by the GUID is necessary, as it improves index efficiency and query performance.

#### Key Features

- **Combines GUID with Timestamp:** Provides better sortability in databases by embedding a timestamp into the GUID.
- **Multiple Methods for Flexibility:** Supports generating COMB GUIDs with the current UTC timestamp, a specified timestamp, or a base GUID combined with a timestamp.

#### Usage Examples

```csharp
using MaksIT.MongoDB.Linq.Utilities;

// Generate a COMB GUID using the current UTC timestamp
Guid combGuid = CombGuidGenerator.CreateCombGuid();
Console.WriteLine($"Generated COMB GUID: {combGuid}");

// Generate a COMB GUID from an existing GUID with the current UTC timestamp
Guid baseGuid = Guid.NewGuid();
Guid combGuidFromBase = CombGuidGenerator.CreateCombGuid(baseGuid);
Console.WriteLine($"Generated COMB GUID from base GUID: {combGuidFromBase}");

// Generate a COMB GUID with a specific timestamp
DateTime specificTimestamp = new DateTime(2024, 8, 31, 12, 0, 0, DateTimeKind.Utc);
Guid combGuidWithTimestamp = CombGuidGenerator.CreateCombGuid(specificTimestamp);
Console.WriteLine($"Generated COMB GUID with specific timestamp: {combGuidWithTimestamp}");

// Extract the embedded timestamp from a COMB GUID
DateTime extractedTimestamp = CombGuidGenerator.ExtractTimestamp(combGuidWithTimestamp);
Console.WriteLine($"Extracted Timestamp from COMB GUID: {extractedTimestamp}");
```

### Benefits of Using COMB GUIDs

- **Improved Indexing Performance:** Embedding a timestamp in the GUID improves the indexing and retrieval performance of documents in databases, especially when using GUIDs as primary keys.
- **Maintains Uniqueness with Sortability:** COMB GUIDs retain the uniqueness properties of traditional GUIDs while adding a sortable timestamp component, making them ideal for scenarios where both are needed.

By utilizing `CombGuidGenerator`, developers can ensure more efficient database operations when dealing with GUID-based identifiers in MongoDB collections.

## Contribution

Contributions to this project are welcome! Please fork the repository and submit a pull request with your changes. If you encounter any issues or have feature requests, feel free to open an issue on GitHub.

## License

This project is licensed under the MIT License. See the full license text below.

---

### MIT License

```
MIT License

Copyright (c) 2024 Maksym Sadovnychyy (MAKS-IT)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## Contact

For any questions or inquiries, please reach out via GitHub or [email](mailto:maksym.sadovnychyy@gmail.com).