// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright (c)2010-2014 Andrew A. Ritz, all rights reserved.
// This code is released under the ms-pl (Microsoft Public License)
// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace drewCo.Curations
{

  // ============================================================================================================================
#if IS_TOOLS_LIB
  public static class IEnumHelper
#else
  internal static class IEnumHelper
#endif
  {

    // This won't be ready until ReflectionTools is available.... (nuGet ??)
    //// --------------------------------------------------------------------------------------------------------------------------
    //public static bool AllUnique<T>(IEnumerable<T> list, Expression<Func<T, object>> prop = null)
    //{
    //  int max = list.Count();
    //  int dCount = -1;
    //  if (prop == null)
    //  {
    //    dCount = list.Distinct().Count();
    //  }
    //  else
    //  {
    //    dCount = DistinctBy(list, prop).Count();
    //  }

    //  return max == dCount;
    //}


    //// --------------------------------------------------------------------------------------------------------------------------
    //public static IEnumerable<T> DistinctBy<T>(IEnumerable<T> list, Expression<Func<T, object>> prop)
    //{
    //  List<object> found = new List<object>(list.Count());
    //  foreach (var item in list)
    //  {
    //    object val = ReflectionTools.GetPropertyValue(item, prop);
    //    if (found.Contains(val)) { continue; }

    //    found.Add(item);
    //    yield return item;
    //  }
    //}



    // --------------------------------------------------------------------------------------------------------------------------
    public static bool ContainsAll<T>(IEnumerable<T> target, IEnumerable<T> toFind)
    {
      foreach (var item in toFind)
      {
        if (!target.Contains(item)) { return false; }
      }
      return true;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static Stack<T> ToStack<T>(IEnumerable<T> src)
    {
      int count = src.Count();

      Stack<T> res = new Stack<T>();
      for (int i = 0; i < count; i++)
      {
        res.Push(src.ElementAt(count - 1 - i));
      }
      return res;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <remarks>
    /// It is well known at this time that this algorithm is very inefficient.  It does produce correct
    /// results however, so improvements can be made later in known test cases....
    /// </remarks>
    public static DiffGram<T> ComputeDiff<T>(IEnumerable<T> left, IEnumerable<T> right, Func<T, T, bool> match = null)
    {
      if (match == null)
      {
        match = (x, y) => x.Equals(y);
      }

      List<T> uniqueLeft = new List<T>();
      List<T> uniqueRight = new List<T>();
      List<T> same = new List<T>();

      foreach (var lItem in left)
      {
        bool hasMatch = false;

        foreach (var rItem in right)
        {
          hasMatch = match(lItem, rItem);
          if (hasMatch)
          {
            same.Add(lItem);
            break;
          }
        }

        if (!hasMatch)
        {
          uniqueLeft.Add(lItem);
        }
      }


      // We can now just compare to our list of items that are known to match to figure out what items on the right
      // are actually working....
      foreach (var item in right)
      {
        bool hasMatch = false;
        foreach (var sItem in same)
        {
          if (match(item, sItem))
          {
            hasMatch = true;
            break;
          }
        }
        if (!hasMatch)
        {
          uniqueRight.Add(item);
        }
      }

      return new DiffGram<T>(uniqueLeft, uniqueRight, same);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Determines if any item item within the <see cref="IEnumerable<T>"/> meets the given criteria.
    /// </summary>
    public static bool Contains<T>(IEnumerable<T> searchIn, Func<T, bool> criteria)
    {
      foreach (T item in searchIn)
      {
        if (criteria.Invoke(item)) { return true; }
      }
      return false;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns the first matching index of the item that meets the given criteria.
    /// </summary>
    public static int IndexOf<T>(IList<T> input, Predicate<T> match)
    {
      int count = input.Count;
      for (int i = 0; i < count; i++)
      {
        if (match.Invoke(input[i]))
        {
          return i;
        }
      }

      return -1;
    }
  }


  // ============================================================================================================================
  /// <summary>
  /// Provides useful manipulations for ILists.
  /// </summary>
#if IS_TOOLS_LIB
  public static class IListHelper
#else
  internal static class IListHelper
#endif
  {

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Get the last-n number of items in a list.
    /// </summary>
    public static List<T> Last<T>(IList<T> src, int n)
    {
      if (src == null) { return null; }
      int len = src.Count;
      if (n > len) { n = len; }

      List<T> res = new List<T>();
      int start = len - n;
      for (int i = start; i < len; i++)
      {
        res.Add(src[i]);
      }

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Swaps the given index in the given list, replacing it with the new value.
    /// </summary>
    public static void Replace<T>(IList<T> target, int targetIndex, T newValue)
    {
      target.RemoveAt(targetIndex);
      target.Insert(targetIndex, newValue);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Partitions the given list, splitting each element into lists based on the result of the given predicate.
    /// </summary>
    public static PartitionResult<T> Partition<T>(IList<T> input, Predicate<T> predicate)
    {
      PartitionResult<T> res = new PartitionResult<T>();

      int iCount = input.Count;
      for (int i = 0; i < iCount; i++)
      {
        T item = input[i];
        if (predicate(item))
        {
          res.TrueList.Add(item);
        }
        else
        {
          res.FalseList.Add(item);
        }
      }

      return res;
    }
  }

  // ============================================================================================================================
  public class PartitionResult<T>
  {
    public IList<T> TrueList = new List<T>();
    public IList<T> FalseList = new List<T>();
  }


}
