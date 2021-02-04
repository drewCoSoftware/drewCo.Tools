//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright ©2009-2015 Andrew A. Ritz, All Rights Reserved
//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace drewCo.Tools
{
  // ============================================================================================================================
  /// <summary>
  /// Helps out with array related tasks.  Computations, element, and size manipulations.
  /// </summary>
  public static class ArrayHelpers
  {

    // --------------------------------------------------------------------------------------------------------------------------
    public static T[] RemoveAt<T>(T[] input, int index)
    {
      T[] res = new T[input.Length - 1];
      int curIndex = 0;
      for (int i = 0; i < index; i++)
      {
        res[curIndex] = input[i];
        curIndex++;
      }
      for (int i = index + 1; i < input.Length; i++)
      {
        res[curIndex] = input[i];
        curIndex++;
      }

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This works like 'redim preserve' from VB.
    /// </summary>
    public static Array Redim(Array input, int newSize)
    {
      if (input.Rank > 1) { throw new NotSupportedException(); }

      Array res = Array.CreateInstance(input.GetType().GetElementType(), newSize);

      for (int i = 0; i < newSize; i++)
      {
        if (i > input.Length - 1) { break; }
        res.SetValue(input.GetValue(i), i);
      }

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static Array Insert(Array input, int index, object value)
    {
      if (input.Rank > 1) { throw new NotSupportedException(); }
      if (index < 0) { throw new IndexOutOfRangeException(); }

      if (index == input.Length) { return Add(input, value); }

      if (index > input.Length - 1) { throw new IndexOutOfRangeException(); }

      // We will redim the array, and then shift all elements after 'index' up one notch.
      Array res = Redim(input, input.Length + 1);
      for (int i = index; i < res.Length - 1; i++)
      {
        object copy = res.GetValue(i);
        res.SetValue(copy, i + 1);
      }
      res.SetValue(value, index);

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static Array Add(Array input, object value)
    {
      if (input.Rank > 1) { throw new NotSupportedException(); }

      Array res = Redim(input, input.Length + 1);
      res.SetValue(value, res.Length - 1);
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static Array RemoveAt(Array input, int index)
    {
      if (input.Rank > 1) { throw new NotSupportedException(); }
      if (index > input.Length - 1) { throw new IndexOutOfRangeException(); }
      if (index < 0) { throw new IndexOutOfRangeException(); }


      // Here we shuffle all of the elements upwards.
      for (int i = index; i < input.Length - 1; i++)
      {
        object copy = input.GetValue(i + 1);
        input.SetValue(copy, i);
      }

      return Redim(input, input.Length - 1);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns an array of integers specifying the size of each dimension of an array.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public static int[] GetDims(Array target)
    {
      int rank = target.Rank;
      int[] res = new int[rank];
      for (int i = 0; i < rank; i++)
      {
        res[i] = target.GetLength(i);
      }

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns the composite sizes of the array dimensions.  Useful for computing the location of a given element number...
    /// </summary>
    /// <param name="dims"></param>
    public static int[] GetSizes(int[] dims)
    {
      int rank = dims.Length;

      int[] res = new int[rank - 1];
      for (int i = 0; i < res.Length; i++)
      {
        res[i] = 1;

        for (int j = i + 1; j < rank; j++)
        {
          res[i] *= dims[j];
        }
      }

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static int[] GetSizes(Array target)
    {
      return GetSizes(GetDims(target));
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public static int GetTotalElements(int[] dims)
    {
      int sum = 1;

      for (int i = 0; i < dims.Length; i++)
      {
        sum *= dims[i];
      }

      return sum;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static int[] ComputeIndicies(int[] dims, int[] sizes, int index)
    {
      int[] res = new int[dims.Length];

      for (int i = 0; i < dims.Length - 1; i++)
      {
        int val = index / sizes[i];
        res[i] = val;

        index -= (val * sizes[i]);
      }
      res[dims.Length - 1] = index;

      return res;
    }

  }


}
