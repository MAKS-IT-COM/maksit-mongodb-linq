using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;


namespace MaksIT.MongoDB.Linq.Serializers;

public class GuidKeyDictionarySerializer<TValue> : SerializerBase<Dictionary<Guid, TValue>> {
  public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Dictionary<Guid, TValue> value) {
    context.Writer.WriteStartDocument();
    foreach (var kvp in value) {
      context.Writer.WriteName(kvp.Key.ToString()); // Convert Guid key to string
      BsonSerializer.Serialize(context.Writer, kvp.Value);
    }
    context.Writer.WriteEndDocument();
  }

  public override Dictionary<Guid, TValue> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) {
    var dictionary = new Dictionary<Guid, TValue>();
    context.Reader.ReadStartDocument();
    while (context.Reader.ReadBsonType() != BsonType.EndOfDocument) {
      var key = Guid.Parse(context.Reader.ReadName());
      var value = BsonSerializer.Deserialize<TValue>(context.Reader);
      dictionary.Add(key, value);
    }
    context.Reader.ReadEndDocument();
    return dictionary;
  }
}
