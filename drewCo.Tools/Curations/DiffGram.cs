// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright (c)2010-2014 Andrew A. Ritz, all rights reserved.
//
// This code is released under the ms-pl (Microsoft Public License)
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
