// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright (c)2010-2014 Andrew A. Ritz, all rights reserved.
//
// This code is released under the ms-pl (Microsoft Public License)
// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using drewCo.Curations;

namespace CurationsTester
{
  // ============================================================================================================================
  [TestClass]
  public class MultiDictionaryTests
  {

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Demonstrates the usage and functionality of the multidictionary helper class.
    /// Getting value paths is an interesting feature, but rather esoteric.
    /// However, this test case will also demonstrate a possible usage of it.
    /// </summary>
    [TestMethod]
    public void CanGetValuePath()
    {
      // Here we will emulate a series of version update operations.
      // Let's say you have an established program with many different legacy versions floating around.
      // Over the years, you have written various functions to upgrade old versions to new versions.
      // You want to be as backwards compatible as possible, being able to update even your oldest
      // sets of data.

      // In this example, we can see the different versions, and their associated 'update' operation.
      // For clarity, those operations as simple string functions.
      MultiDictionary<int, int, Func<string, string>> updateOps = new MultiDictionary<int,int,Func<string,string>>();
      updateOps.Add(1,2, (s) => s + " two");
      updateOps.Add(2, 3, (s) => s + " three");
      updateOps.Add(3, 4, (s) => s + " four");


      // Using the helper, we can determine the order and functions that need to be run to take
      // our data from version one to version four.
      var funcs = MultiDictionaryHelper.GetValuePath<int, Func<string, string>>(updateOps, 1, 4);
      string test = "one";
      foreach (var f in funcs)
      {
        test = f(test);
      }
      Assert.AreEqual(test, "one two three four");
    }



    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that we get proper exceptions for when we try to add duplicate keys to the system.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CanCatchDuplicateKey()
    {
      MultiDictionary<string, int, int> numberNames = new MultiDictionary<string, int, int>();

      numberNames.Add("ten times", 10, 100);
      numberNames.Add("two times", 10, 100);
      Assert.IsTrue(true);

      numberNames.Add("ten times", 10, 100);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that the values collection is ready and available to us....
    /// </summary>
    [TestMethod]
    public void CanGetAllValues()
    {
      MultiDictionary<int, int, int> products = new MultiDictionary<int, int, int>();

      const int MAX  =10;
      int total = MAX * MAX;
      for (int i = 0; i < MAX; i++)
      {
        for (int j = 0; j < MAX; j++)
        {
          products.Add(i, j, i * j);
        }
      }

      Assert.AreEqual(total, products.Values.Count);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Demonstrates that we can get our hands on the keys as needed.
    /// Also shows that the counts of keys, etc. remains correct as we go.
    /// </summary>
    [TestMethod]
    public void CanGetKeyCounts()
    {
      
      MultiDictionary<int, int, int> sums = new MultiDictionary<int,int,int>();

      const int MAX_X = 10;
      const int MAX_Y =10;
      for (int i = 0; i < MAX_X; i++)
      {
        for (int j = 0; j < MAX_Y; j++)
        {
          sums.Add(i,j, (i*100) + j);
        }
      }

      int total = MAX_X * MAX_Y;
      Assert.AreEqual(total, sums.Keys.Count);


      // Now we will drop all of the items in the Dictionary...
      int removeCount = 0;

      for (int i = 0; i < MAX_X; i++)
      {
        for (int j = 0; j < MAX_Y; j++)
        {
          sums.Remove(i, j);
          removeCount++;
          Assert.AreEqual(total - removeCount, sums.Keys.Count);
        }
      }

    }



    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Show that values can be removed form the system as well.
    /// </summary>
    [TestMethod]
    public void CanRemoveValues()
    {
      MultiDictionary<int, int, int> sums = new MultiDictionary<int,int,int>();
      
      const int MAX  = 10;
      int half = MAX / 2;
      for (int i = 0; i < MAX; i++)
      {
        sums.Add(i,i,i);
      }
      Assert.AreEqual(MAX, sums.Values.Count);

      // Remove the first half of the items.  The actual contents of the values list will be compared when we are done.
      for (int i = 0; i < half; i++)
      {
        sums.Remove(i,i);
      }

      Assert.AreEqual(half, sums.Values.Count);
      for (int i = 0; i < half ; i++)
      {
        Assert.AreEqual(i+half, sums[i+half, i+half]);
      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that we can enumerate over all of the values using normal IEnumerable access style.
    /// </summary>
    [TestMethod]
    public void CanEnumerateValues()
    {

      const int MAX = 10;
      MultiDictionary<object, object, int> values = new MultiDictionary<object, object, int>();
      bool[] itemMask = new bool[MAX];

      for (int i = 0; i < MAX; i++)
      {
        values.Add(new object(), new object(), i);
        itemMask[i] = false;
      }

      foreach (var val in values)
      {
        itemMask[val] = true;
      }

      for (int i = 0; i < MAX; i++)
      {
        Assert.IsTrue(itemMask[i], "The value of '{0}' was never located", i);
      }

    }

  }
}
