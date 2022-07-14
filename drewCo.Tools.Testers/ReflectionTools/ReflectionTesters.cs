// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright (c)2012-2022 Andrew A. Ritz, all rights reserved.
//
// This code is released under the ms-pl (Microsoft Public License)
// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
using ReflectionToolsTesters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

#if !NETFX_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace drewCo.Tools.Testers
{
  // ============================================================================================================================
  [TestClass]
  public class ReflectionTesters
  {
    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test case was provided to show that we can use the various methods to get/set values
    /// on nested data types.
    /// </summary>
    [TestMethod]
    public void CanGetAndSetNestedPropertyValue()
    {
      const string VAL_1 = "Value 1";

      var testInstance = new TypeWithNestedData();
      testInstance.TypeWithNull = new TypeWithNullable()
      {
        SomeName = VAL_1,
      };

      string propPath = $"{nameof(TypeWithNestedData.TypeWithNull)}.{nameof(TypeWithNullable.SomeName)}";
      string check1 = (string)ReflectionTools.GetNestedPropertyValue(testInstance, propPath);
      string check2 = (string)ReflectionTools.GetNestedPropertyValue<TypeWithNestedData>(testInstance, x => x.TypeWithNull.SomeName);
      Assert.AreEqual(VAL_1, check1);
      Assert.AreEqual(VAL_1, check2);

      // Now show that we can set the values as well.
      const string VAL_2 = "Value 2";
      ReflectionTools.SetNestedPropertyValue(testInstance, propPath, VAL_2);
      Assert.AreEqual(VAL_2, testInstance.TypeWithNull.SomeName);

      const string VAL_3 = "Value 3";
      ReflectionTools.SetNestedPropertyValue<TypeWithNestedData>(testInstance, x => x.TypeWithNull.SomeName, VAL_3);
      Assert.AreEqual(VAL_3, testInstance.TypeWithNull.SomeName);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanCreateInstanceWithNonPublicDefaultConstructor()
    {
      var x = ReflectionTools.CreateInstance<TypeWithPrivateDefaultCtor>();
      var y = ReflectionTools.CreateInstance<TypeWithProtectedDefaultCtor>();
      var z = ReflectionTools.CreateInstance<TypeWithInternalDefaultCtor>();

      Assert.IsNotNull(x);
      Assert.IsNotNull(y);
      Assert.IsNotNull(z);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanCreateStructInstance()
    {
      MyStruct m = ReflectionTools.CreateInstance<MyStruct>();
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Some collection types in WinRT don't / can't get resolved because certain assemblies, etc. aren't loaded into the
    /// app domain by default.  However, it doesn't hurt to make sure that the resolutions also work on win32.
    /// </summary>
    [TestMethod]
    public void CanResolveCollectionTypesByName()
    {

      // Add to this list as needed.
      string[] testNames = new[]
      {
        "System.Collections.Generic.SortedDictionary`2",
        "System.Collections.Generic.HashSet`1",
      };
      foreach (var name in testNames)
      {
        Type checkType = ReflectionTools.ResolveType(name, true, true);
      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Show that we can resolve a static function declared on a certain type.
    /// </summary>
    [TestMethod]
    public void CanGetStaticMethodOnType()
    {

      //var o = Activator.CreateInstance<TypeWithNoDefaultConstructor>();

      MethodInfo m1 = ReflectionTools.GetMethod(typeof(TypeWithStaticFunctions), "AddEm", new[] { typeof(int) });
      MethodInfo m2 = ReflectionTools.GetMethod(typeof(TypeWithStaticFunctions), "AddEm", new[] { typeof(int), typeof(int) });

      Assert.IsNotNull(m1, "First method should not be null!");
      Assert.AreEqual(1, m1.GetParameters().Length, "There should only be one parameter on the first method!");

      Assert.IsNotNull(m2, "Second method should not be null!");
      Assert.AreEqual(2, m2.GetParameters().Length, "There should be two parameters on the second method!");
    }


    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanGetNonPublicFieldAndProp()
    {
      Type t = typeof(TypeWithNonPublics);
      PropertyInfo p = ReflectionTools.GetPropertyInfo(t, "Y", true, true);
      Assert.IsNotNull(p, "Should be able to get non public property!");

      // TODO: Include conditions for fields....
      FieldInfo f = ReflectionTools.GetFieldInfo(t, "X", true);
      Assert.IsNotNull(f, "Should be able to get non public field!");
    }



    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test case solves a bug where we weren't able to get proper values for nullable data types.
    /// </summary>
    [TestMethod]
    public void CanGetNullablePropertyValueWithNestedExpression()
    {
      const int TEST_VAL = 12345;
      TypeWithNestedData test = new TypeWithNestedData()
      {
        TypeWithNull = new TypeWithNullable()
        {
          NullOrInt = TEST_VAL,
        }
      };

      var expression = (Expression<Func<TypeWithNestedData, object>>)(x => x.TypeWithNull.NullOrInt);

      object comp = ReflectionTools.GetNestedPropertyValue(test, expression);
      Assert.AreEqual(TEST_VAL, comp, "Incorrect nullable value! [1]");

      test.TypeWithNull.NullOrInt = null;
      comp = ReflectionTools.GetNestedPropertyValue(test, expression);
      Assert.AreEqual(null, comp, "Incorrect nullable value! [2]");


    }




    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanResolveBaseType()
    {
      Type one = typeof(BaseType);
      Type two = typeof(DerivedType);
      Type three = typeof(FarRemoved);

      Type other = typeof(ReflectionContext);

      // Show thatthe inheritance chain is correctly detected.
      foreach (var item in new[] { one, two, three })
      {
        Assert.IsTrue(ReflectionTools.IsOfBaseType(item, one));
      }

      Assert.IsFalse(ReflectionTools.IsOfBaseType(other, one));

    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that we can collect property attributes.
    /// </summary>
    [TestMethod]
    public void CanGetPropertyAttributes()
    {

      var propsWithAttributes = ReflectionTools.GetAttributesOnProperties<DefaultValueAttribute, TypeWithPropsWithAttributes>().ToList();
      Assert.AreEqual(2, propsWithAttributes.Count, "There should be two items listed!");

      Assert.AreEqual("x", propsWithAttributes[0].Attribute.Value, "Bad value for item #1!");
      Assert.AreEqual("SomeString", propsWithAttributes[0].Property.Name);

      Assert.AreEqual(100, propsWithAttributes[1].Attribute.Value, "Bad value for item #2!");
      Assert.AreEqual("SomeInt", propsWithAttributes[1].Property.Name);
    }



    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test case was written to solve a flag bug where 'GetFields' was using bad flags internally and giving bad
    /// results.  While we are at it, we check for properties too.
    /// </summary>
    [TestMethod]
    public void GetFieldsGetsInstanceFields()
    {
      FieldInfo[] fields = ReflectionTools.GetFields<NestedType>().ToArray();
      PropertyInfo[] props = ReflectionTools.GetProperties<NestedType>().ToArray();

      Assert.AreEqual(2, fields.Length, "Invalid field count!");
      Assert.AreEqual(2, props.Length, "Invalid prop count!");
    }

    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanResolveTypeByName()
    {
      const string TEST_NAME = "InvalidOperationException";
      Type exType = Type.GetType(TEST_NAME);
      Assert.IsNull(exType, "The normal resolution should not work for simple type names!");

      // Our enhanced version should do the trick!
      exType = ReflectionTools.ResolveType(TEST_NAME);
      Assert.IsNotNull(exType, "The resolution function should have worked!");

    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Demonstrates that the type converters can indeed change many types to string.
    /// This case was provided to solve a bug where such a conversion was not supported.
    /// </summary>
    [TestMethod]
    public void CanConvertTypesToString()
    {
      bool falseVal = false;
      object converted = ReflectionTools.ConvertEx(typeof(string), falseVal);
      Assert.AreEqual("False", converted, "Invalid value!");

      // We may add additional cases as we see fit...
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Makes sure that we can convert a compatible type to its nullable equivalent, i.e. bool -> bool?
    /// This case was also provided to solve a specific bug / lack of implementation.
    /// </summary>
    [TestMethod]
    public void CanConvertCompatibleValueToNullableType()
    {

      Type expectedType = typeof(bool?);
      bool actualVal = true;
      object converted = ReflectionTools.ConvertEx(expectedType, actualVal);
      Assert.IsNotNull(converted, "Converted value should not be null!");

      // NOTE: Because we have a value that we are trying to convert (instead of null) we will actually get back
      // its type, instead of a nullable version.  This is what the framework will output when we use the nullable constructor
      // with a value arg.  It may seem odd that we don't get the *? that we expect, but that is just how it works.
      Assert.AreEqual(converted.GetType(), actualVal.GetType());
    }


    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanGetNestedPropertyInfoOnType()
    {
      PropertyInfo nestedCountProp = ReflectionTools.GetNestedPropertyInfo<TypeWithNestedProps>("Nest1.Count");
      Assert.IsNotNull(nestedCountProp, "Property should not be null!");
    }

    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanSetNestedPropertyValue()
    {
      const int TEST_COUNT = 152;
      TypeWithNestedProps target = new TypeWithNestedProps();
      Assert.AreNotEqual(TEST_COUNT, target.Nest1.Count);


      ReflectionTools.SetNestedPropertyValue(target, "Nest1.Count", TEST_COUNT);
      Assert.AreEqual(TEST_COUNT, target.Nest1.Count);
    }

  }



  // ============================================================================================================================
  public class TypeWithPropsWithAttributes
  {
    [DefaultValue("x")]
    public string SomeString { get; set; }

    [DefaultValue(100)]
    public int SomeInt { get; set; }
  }

  // ============================================================================================================================
  public class TypeWithNestedProps
  {
    // --------------------------------------------------------------------------------------------------------------------------
    public TypeWithNestedProps()
    {
      Nest1 = new NestedType();
    }
    public NestedType Nest1 { get; set; }
  }

  // ============================================================================================================================
  public class NestedType
  {
    public string Name { get; set; }
    public int Count { get; set; }

    public string FieldName;
    public int FieldCount;
  }

}
