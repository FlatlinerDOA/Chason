using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.TestObjects
{
    using System.Runtime.Serialization;

    public class WagePerson : Person
  {
    [DataMember]
    public decimal HourlyWage { get; set; }
  }
}