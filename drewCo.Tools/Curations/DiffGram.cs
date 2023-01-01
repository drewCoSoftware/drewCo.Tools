// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright (c)2010-2014 Andrew A. Ritz, all rights reserved.
// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace drewCo.Curations
{

  // ============================================================================================================================
  /// <summary>
  /// Describes the differences and intersections between two IEnumerable<typeparamref name="T"/> instances.
  /// </summary>
  public class DiffGram<T>
  {
    public ReadOnlyCollection<T> UniqueLeft;
    public ReadOnlyCollection<T> UniqueRight;
    public ReadOnlyCollection<T> Same;




    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Construct and compute a diffgram from the given left/right enumerables and optional comparison function.
    /// </summary>
    public DiffGram(IEnumerable<T> left, IEnumerable<T> right, IEqualityComparer<T> comparer = null)
    {
      var uniqueLeft = new List<T>();
      var uniqueRight = new List<T>();
      var sameItems = new List<T>();

      var usedItems = new HashSet<T>(comparer);
      foreach (var item in left)
      {
        // Try to find it in the right one....
        if (!right.Contains(item, comparer))
        {
          uniqueLeft.Add(item);
        }
        else
        {
          // We have a match!
          sameItems.Add(item);
        }
        usedItems.Add(item);
      }

      // Everything on the right that hasn't been used is unique!
      foreach (var item in right)
      {
        if (usedItems.Contains(item)) { continue; }
        uniqueRight.Add(item);
      }

      UniqueLeft =  new ReadOnlyCollection<T>(uniqueLeft);
      UniqueRight = new ReadOnlyCollection<T>(uniqueRight);
      Same = new ReadOnlyCollection<T>(sameItems);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public DiffGram(List<T> uniqueLeft, List<T> uniqueRight, List<T> same)
    {
      UniqueLeft = new ReadOnlyCollection<T>(uniqueLeft);
      UniqueRight = new ReadOnlyCollection<T>(uniqueRight);
      Same = new ReadOnlyCollection<T>(same);
    }

    public bool AllAreSame
    {
      get { return UniqueLeft.Count == 0 && UniqueRight.Count == 0; }
    }

  }


}
