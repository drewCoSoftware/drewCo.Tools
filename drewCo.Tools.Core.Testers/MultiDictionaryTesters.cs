#if NETCOREAPP
using drewCo.Curations;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TestMethod = NUnit.Framework.TestAttribute;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace drewCo.Tools.Core.Testers
{
  // ============================================================================================================================
  public class MultiDictionaryTesters
  {

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test case was provided to show that multi dictionary can use custom key comparers.
    /// </summary>
    [TestMethod]
    public void CanUseCustomKeyComparers()
    {

      // Note that c1 and c2 have the same value for 'Number'
      var c1 = new TestClass() { Number = 1 };
      var c2 = new TestClass() { Number = 1 };
      var c3 = new TestClass() { Number = 2 };


      {
        // Using the custom comparer will cause c1 and c2 to be treated as the same key.
        // So we should only end up with 4 added items...
        const int EXPECTED_COUNT = 4;
        var comparer = new TestClassComparer();
        var d1 = new MultiDictionary<TestClass, TestClass, int>(comparer, comparer);

        int addedItems = 0;
        var testKeys = new[] { c1, c2, c3 };
        for (int i = 0; i < testKeys.Length; i++)
        {
          for (int j = 0; j < testKeys.Length; j++)
          {
            d1[testKeys[i], testKeys[j]] = addedItems;
            ++addedItems;
          }
        }
        Assert.AreEqual(EXPECTED_COUNT, d1.Count(), "Incorrect number of added items in dictionary #1");
      }

      {
        // Using the default comparer will cause each pair to be unique.
        var d1 = new MultiDictionary<TestClass, TestClass, int>();
        int addedItems = 0;
        var testKeys = new[] { c1, c2, c3 };
        for (int i = 0; i < testKeys.Length; i++)
        {
          for (int j = 0; j < testKeys.Length; j++)
          {
            d1[testKeys[i], testKeys[j]] = addedItems;
            ++addedItems;
          }
        }
        Assert.AreEqual(d1.Count(), addedItems, "Incorrect number of added items in dictionary #1");
      }


    }

    // --------------------------------------------------------------------------------------------------------------------------
    class TestClass
    {
      public int Number { get; set; }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    class TestClassComparer : IEqualityComparer<TestClass>
    {
      // --------------------------------------------------------------------------------------------------------------------------
      public bool Equals(TestClass x, TestClass y)
      {
        if (x == null) { return y == null; }
        if (y == null) { return x == null; }
        return x.Number.Equals(y.Number);
      }

      // --------------------------------------------------------------------------------------------------------------------------
      public int GetHashCode([DisallowNull] TestClass obj)
      {
        return obj.Number;
      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test case shows that the 'TryGetValue' function of MultiDictionary works as expected.
    /// </summary>
    [TestMethod]
    public void CanUseTryGetValue()
    {

      const int KEY_1 = 100;
      const string KEY_2 = "One Hundred";
      const string TEST_VALUE = "I am a test value!";

      var dict = new MultiDictionary<int, string, string>();
      dict.Add(KEY_1, KEY_2, TEST_VALUE);

      {
        bool hasVal = dict.TryGetValue(KEY_1, KEY_2, out string checkVal);
        Assert.IsTrue(hasVal);
        Assert.AreEqual(checkVal, TEST_VALUE, "The test value does not match!");
      }


      // Show that it will come up empty for keys that don't exist.
      {
        const string KEY_3 = KEY_2 + "_notakey";
        bool hasVal = dict.TryGetValue(KEY_1, KEY_3, out string checkVal);
        Assert.IsFalse(hasVal);
      }

    }


  }
}
