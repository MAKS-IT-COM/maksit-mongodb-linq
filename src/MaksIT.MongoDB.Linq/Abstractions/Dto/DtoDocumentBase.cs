using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaksIT.MongoDBLinq.Abstractions.Domain {
  public abstract class DtoDocumentBase : DtoObjectBase {
    public Guid Id { get; set; }
  }
}
