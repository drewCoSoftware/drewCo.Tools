using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace drewCo.Tools.Testers
{
  // ============================================================================================================================
  [TestClass]
  public class ArrayHelpersTesters
  {

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that we can insert items into an array, at whatever index.
    /// </summary>
    [TestMethod]
    public void CanInsertIntoArray()
    {

      string[] myString = new string[10];
      for (int i = 0; i < myString.Length; i++)
      {
        myString[i] = string.Empty;
      }

      const int TEST_INDEX = 5;
      const string TEST_STRING = "whatever...";
      string[] comp = (string[])ArrayHelpers.Insert(myString, TEST_INDEX, TEST_STRING);

      Assert.AreEqual(myString.Length + 1, comp.Length, "Invalid length!");
      Assert.AreEqual(TEST_STRING, comp[TEST_INDEX], "Inserted value is not correct!");

      for (int i = 0; i < comp.Length; i++)
      {
        if (i != TEST_INDEX)
        {
          Assert.AreEqual(string.Empty, comp[i], "The rest of the strings should be empty!");
        }
      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanAddToArray()
    {
      const int COUNT = 10;
      double[] test = new double[0];

      for (int i = 0; i < COUNT; i++)
      {
        test = (double[])ArrayHelpers.Add(test, i);
      }

      Assert.AreEqual(COUNT, test.Length);
      for (int i = 0; i < COUNT; i++)
      {
        Assert.AreEqual(test[i], i, "The value is wrong at index#{0}", i);
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that we can remove items from an array at the given index.
    /// </summary>
    [TestMethod]
    public void CanRemoveFromArray()
    {
      const int COUNT = 10;

      int[] test = new int[COUNT];
      for (int i = 0; i < COUNT; i++)
      {
        test[i] = i;
      }


      // Now we will remove the item at the given index....
      const int REMOVE_INDEX = 4;
      int[] res = (int[])ArrayHelpers.RemoveAt(test, REMOVE_INDEX);

      // And make sure it all comes out...
      Assert.AreEqual(COUNT - 1, res.Length, "Invalid length for array after removal!");
      for (int i = 0; i < res.Length; i++)
      {
        int val = res[i];
        Assert.AreNotEqual(REMOVE_INDEX, val, "Value can't be the same as the removed item!");
        if (i < REMOVE_INDEX)
        {
          Assert.AreEqual(i, val, "Invalid value at index #{0}", i);
        }
        else
        {
          Assert.AreEqual(i + 1, val, "Invalid value at index #{0}", i);
        }
      }

    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that we can redimension an array to make it smaller.
    /// </summary>
    [TestMethod]
    public void CanReduceArray()
    {
      const int START_COUNT = 10;

      float[] source = new float[START_COUNT];
      for (int i = 0; i < START_COUNT; i++)
      {
        source[i] = (float)(i * System.Math.PI);
      }


      // Now we trim it down..
      const int NEW_COUNT = 5;
      float[] comp = (float[])ArrayHelpers.Redim(source, NEW_COUNT);


      Assert.IsNotNull(comp);
      Assert.AreEqual(1, comp.Rank);
      Assert.AreEqual(NEW_COUNT, comp.Length, "invalid length!");

      for (int i = 0; i < comp.Length; i++)
      {
        Assert.AreEqual(source[i], comp[i], "Array values don't match at index {0}", i);
      }

    }

    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanEnlargeArray()
    {
      const int START_COUNT = 10;

      int[] source = new int[START_COUNT];
      for (int i = 0; i < START_COUNT; i++)
      {
        source[i] = i;
      }

      const int NEW_COUNT = 15;
      int[] comp = (int[])ArrayHelpers.Redim(source, NEW_COUNT);

      Assert.IsNotNull(comp);
      Assert.AreEqual(1, comp.Rank);
      Assert.AreEqual(NEW_COUNT, comp.Length, "invalid length!");

      for (int i = 0; i < source.Length; i++)
      {
        Assert.AreEqual(source[i], comp[i], "Array values don't match at index {0}", i);
      }
    }




  }
}
