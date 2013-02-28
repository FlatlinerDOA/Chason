using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.TestObjects
{
    using System.Runtime.Serialization;

    public class PublicParametizedConstructorWithPropertyNameConflict
  {
    private readonly int _value;

    public PublicParametizedConstructorWithPropertyNameConflict(string name)
    {
      _value = Convert.ToInt32(name);
    }

    public int Name
    {
      get { return _value; }
    }
  }

  public class PublicParametizedConstructorWithPropertyNameConflictWithAttribute
  {
    private readonly int _value;

    public PublicParametizedConstructorWithPropertyNameConflictWithAttribute(string nameParameter)
    {
      _value = Convert.ToInt32(nameParameter);
    }

    public int Name
    {
      get { return _value; }
    }
  }
}
