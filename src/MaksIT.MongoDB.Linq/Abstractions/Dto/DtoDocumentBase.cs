namespace MaksIT.MongoDB.Linq.Abstractions.Domain;

public abstract class DtoDocumentBase<T> : DtoObjectBase {
  public required T Id { get; set; }
}
