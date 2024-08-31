using MongoDB.Bson.Serialization;

namespace MaksIT.MongoDB.Linq.Tests.Mock {
  internal class MongoIdGeneratorMock : IIdGenerator {
    public Guid Generate() {
      return Guid.NewGuid();
    }

    #region not implemented
    public object GenerateId(object container, object document) {
      throw new NotImplementedException();
    }

    public bool IsEmpty(object id) {
      throw new NotImplementedException();
    }
    #endregion
  }
}
