// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright (c)2010-2014 Andrew A. Ritz, all rights reserved.
//
// This code is released under the ms-pl (Microsoft Public License)
// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using drewCo.Curations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectSchemaTesters
{
  // ============================================================================================================================
  [TestClass]
  public class IEnumTesters
  {

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Makes sure that the 'Contains' function works correctly.
    /// </summary>
    [TestMethod]
    public void CanUseContains()
    {
      List<int> TestList = new List<int>();
      for (int i = 0; i < 10; i++)
      {
        TestList.Add(i);
      }

      Assert.IsTrue(IEnumHelper.Contains(TestList, x => x == 5));
      Assert.IsFalse(IEnumHelper.Contains(TestList, x => x == 11));
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that we can compute a diffgram for lists that are the same...
    /// </summary>
    [TestMethod]
    public void CanComputeSameDiffgram()
    {
      List<string> left;
      List<string> right;
      CreateStringLists(out left, out right);

      DiffGram<string> diff = IEnumHelper.ComputeDiff(left, right);

      TestDiffGram(diff, true, left.Count, 0, 0);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Show that we can find when there are differences only on the left side.
    /// </summary>
    [TestMethod]
    public void CanFindLeftDiffs()
    {
      List<int> left;
      List<int> right;
      CreateIntLists(out left, out right);

      left.AddRange(new[] { 5, 6, 7 });
      DiffGram<int> diff = IEnumHelper.ComputeDiff(left, right);

      TestDiffGram(diff, false, 3, 3, 0);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Show that we can find when there are differences only on the right side.
    /// </summary>
    [TestMethod]
    public void CanFindRightDiffs()
    {
      List<int> left;
      List<int> right;
      CreateIntLists(out left, out right);

      right.AddRange(new[] { 5, 6, 7 });

      DiffGram<int> diff = IEnumHelper.ComputeDiff(left, right);
      TestDiffGram(diff, false, 3, 0, 3);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that we can find differences between lists when all of their contents are different.
    /// </summary>
    [TestMethod]
    public void CanFindAllDiffs()
    {
      List<string> left = new List<string>() { "a", "b", "c" };
      List<string> right = new List<string>() { "d", "e", "f" };

      DiffGram<string> diff = IEnumHelper.ComputeDiff(left, right);
      TestDiffGram(diff, false, 0, 3, 3);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that we can also detect mixed results and what not.
    /// </summary>
    [TestMethod]
    public void CanFindMixedDiffs()
    {
      List<string> left;
      List<string> right;
      CreateStringLists(out left, out right);

      left.AddRange(new[] { "l1", "l2", "l3" });
      right.AddRange(new[] { "r1", "r2", "r3" });

      DiffGram<string> diff = IEnumHelper.ComputeDiff(left, right);
      TestDiffGram(diff, false, 3, 3, 3);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    private static void TestDiffGram<T>(DiffGram<T> diff, bool expectedSame, int expectedSameCount, int expectedUniqueLeft,
                                        int expectedUniqueRight)
    {
      Assert.AreEqual(expectedSame, diff.AllAreSame, "The 'AllMatching' flag should be set to '{0}'", expectedSame);
      Assert.AreEqual(expectedSameCount, diff.Same.Count, "All items should be the same!");
      Assert.AreEqual(expectedUniqueLeft, diff.UniqueLeft.Count, "There should be no unique items on the left!");
      Assert.AreEqual(expectedUniqueRight, diff.UniqueRight.Count, "There should be no unique items on the right!");
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Creates lists of strings that each contain "1,2,3" (by default)
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    private static void CreateIntLists(out List<int> left, out List<int> right, IEnumerable<int> useList = null)
    {
      left = new List<int>();
      right = new List<int>();

      if (useList == null)
      {
        useList = new[] { 1, 2, 3 };
      }

      foreach (var item in useList)
      {
        left.Add(item);
        right.Add(item);
      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Creates lists of strings that each contain "abc,def,ghi" (by default)
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    private static void CreateStringLists(out List<string> left, out List<string> right, IEnumerable<string> useList = null)
    {
      left = new List<string>();
      right = new List<string>();

      if (useList == null)
      {
        useList = new[] { "abc", "def", "ghi", };
      }

      foreach (var item in useList)
      {
        left.Add(item);
        right.Add(item);
      }
    }

  }
}
