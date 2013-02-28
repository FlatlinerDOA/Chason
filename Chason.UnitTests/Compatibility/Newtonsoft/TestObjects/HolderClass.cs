using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.TestObjects
{
    using System.Runtime.Serialization;

    public class HolderClass
  {
    public HolderClass() { }

    [DataMember] // (TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All)
    public ContentBaseClass TestMember { get; set; }

    [DataMember] // TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All
    public Dictionary<int, IList<ContentBaseClass>> AnotherTestMember { get; set; }

    public ContentBaseClass AThirdTestMember { get; set; }

  }
}
