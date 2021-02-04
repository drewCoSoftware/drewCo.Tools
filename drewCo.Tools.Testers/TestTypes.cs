using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drewCo.Tools.Testers
{

  public class TypeWithFields
  {
    public string Description;
    public int Number;
  }

  public class NestingType
  {
    public int Number { get; set; }
    public NestingType Nested { get; set; }
  }



  public class ListType1
  {
    public string Name { get; set; }
    public int Number { get; set; }
  }

  public class ListType2
  {
    public string Name { get; set; }
    public int Number { get; set; }
  }

  public class ListHost_T1
  {
    public List<ListType1> List { get; set; } = new List<ListType1>();
  }

  public class ListHost_T2
  {
    public List<ListType2> List { get; set; } = new List<ListType2>();
  }

}


namespace Structo
{
  public struct MyType
  {
    public string Name;
    public int Number;
    public double Weight;
  }
}

namespace Classo
{
  public class MyType
  {
    public string Name { get; set; }
    public int Number { get; set; }
    public double Weight { get; set; }
  }
}

