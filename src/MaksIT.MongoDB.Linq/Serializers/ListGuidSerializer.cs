using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;


namespace MaksIT.MaksIT.MongoDB.Linq.Serializers;

public class ListGuidSerializer : SerializerBase<List<Guid>> {
  private readonly GuidSerializer _guidSerializer = new GuidSerializer();

  public override List<Guid> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) {
    var bsonType = context.Reader.CurrentBsonType;
    if (bsonType == BsonType.Array) {
      var guidList = new List<Guid>();
      context.Reader.ReadStartArray();
      while (context.Reader.ReadBsonType() != BsonType.EndOfDocument) {
        var guid = _guidSerializer.Deserialize(context, args);
        guidList.Add(guid);
      }
      context.Reader.ReadEndArray();
      return guidList;
    }
    throw new FormatException($"Cannot deserialize BsonType '{bsonType}' to List<Guid>.");
  }

  public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, List<Guid> value) {
    context.Writer.WriteStartArray();
    foreach (var guid in value) {
      _guidSerializer.Serialize(context, args, guid);
    }
    context.Writer.WriteEndArray();
  }
}
