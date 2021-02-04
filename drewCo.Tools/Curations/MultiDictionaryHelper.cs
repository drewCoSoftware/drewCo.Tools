using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace drewCo.Curations
{
  // ============================================================================================================================
  public static class MultiDictionaryHelper
  {

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns an array of values that correspond to the 'path' of keys through the collection.
    /// For example let's say you have a dictionary like so: (x,y -> 1) | (y,z -> 2)
    /// and you want the path of values from x -> z.  Normally this won't produce any value since the key (x,z) is technically
    /// not in place.  However, this function will find the path, returning (1,2) because (x,y (1)) and (y, z (2)) [x->y->z = (1,2)]
    /// </summary>
    public static TValue[] GetValuePath<TKey, TValue>(MultiDictionary<TKey, TKey, TValue> searchIn, TKey startKey, TKey stopKey)
    {
      List<TValue> res = new List<TValue>();

      if (searchIn.ContainsKey(startKey, stopKey))
      {
        return new[] { searchIn[startKey, stopKey] };
      }

      var allKeys = searchIn.Keys;
      var matchingFirst = (from x in allKeys where x.Item1.Equals(startKey) select x).ToList();
      var matchingLast = (from x in allKeys where x.Item2.Equals(stopKey) select x).ToList();

      // There is no path at all!
      if (matchingFirst.Count == 0 || matchingLast.Count == 0)
      {
        return null;
      }

      foreach (var item in matchingFirst)
      {

        TValue[] chunk = GetValuePath(searchIn, item.Item2, stopKey);
        if (chunk != null)
        {
          res.Add(searchIn[startKey, item.Item2]);
          return res.Concat(chunk).ToArray();
        }
      }

      // There is no path to be resolved....
      return null;
    }



  }
}
