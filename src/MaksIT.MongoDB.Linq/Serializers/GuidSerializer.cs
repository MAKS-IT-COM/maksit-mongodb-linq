using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;


namespace MaksIT.MaksIT.MongoDB.Linq.Serializers;

public class GuidSerializer : SerializerBase<Guid> {
  public override Guid Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) {
    var bsonType = context.Reader.CurrentBsonType;
    if (bsonType == BsonType.Binary) {
      var binaryData = context.Reader.ReadBinaryData();
      return new Guid(binaryData.Bytes);
    }
    throw new FormatException($"Cannot deserialize BsonType '{bsonType}' to Guid.");
  }

  public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Guid value) {
    var guidBytes = value.ToByteArray();
    var binaryData = new BsonBinaryData(guidBytes, BsonBinarySubType.UuidStandard);
    context.Writer.WriteBinaryData(binaryData);
  }
}
