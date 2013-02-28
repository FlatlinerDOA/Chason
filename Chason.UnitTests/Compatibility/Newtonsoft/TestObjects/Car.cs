using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.TestObjects
{
    using Chason;

    public class Car
  {
    // included in JSON
    public string Model { get; set; }
    public DateTime Year { get; set; }
    public List<string> Features { get; set; }

    // ignored
    [NonDataMember]
    public DateTime LastModified { get; set; }
  }
}
