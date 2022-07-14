using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReflectionToolsTesters
{

  // ============================================================================================================================
  public class TypeWithPrivateDefaultCtor
  {
    private TypeWithPrivateDefaultCtor()
    {
      X = 100;
    }

    public int X { get; private set; }
  }


  // ============================================================================================================================
  public class TypeWithProtectedDefaultCtor
  {
    protected TypeWithProtectedDefaultCtor()
    {
      X = 100;
    }

    public int X { get; protected set; }
  }

  // ============================================================================================================================
  public class TypeWithInternalDefaultCtor
  {
    protected TypeWithInternalDefaultCtor()
    {
      X = 100;
    }

    public int X { get; protected set; }
  }


  // ============================================================================================================================
  public struct MyStruct
  {
    int x;
    int y;
    string z;
  }

  // ============================================================================================================================
  public class TypeWithNoDefaultConstructor
  {
    // --------------------------------------------------------------------------------------------------------------------------
    TypeWithNoDefaultConstructor(int x_)
    {
      X = x_;
    }

    public int X { get; private set; }
  }


  // ============================================================================================================================
  public class TypeWithStaticFunctions
  {
    public static int AddEm(int x) { return x + 0; }
    public static int AddEm(int x, int y) { return x + y; }
  }

  // ============================================================================================================================
  public class TypeWithNonPublics
  {
    private int X = 100;
    private int Y { get; set; }
  }

  // ============================================================================================================================
  public class BaseType
  { }

  // ============================================================================================================================
  public class DerivedType : BaseType
  { }

  // ============================================================================================================================
  public class FarRemoved : DerivedType
  { }


  // ============================================================================================================================
  public class TypeWithNestedData
  {
    public TypeWithNullable TypeWithNull { get; set; }
  }

  // ============================================================================================================================
  public class TypeWithNullable
  {
    public string SomeName { get; set; }
    public int? NullOrInt { get; set; }
  }
}
