// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright (c)2010-2014 Andrew A. Ritz, all rights reserved.
// 
// This code is released under the ms-pl (Microsoft Public License)
// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace drewCo.Curations
{

  // ============================================================================================================================
  /// <summary>
  /// Like a normal observable collection, but with features that make them easier to work with.
  /// </summary>
  public class SuperObserver<T> : ObservableCollection<T>
  {


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Allows you to add a range of items to the collection, alternately avoiding notification calls on a per item basis.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="suspendNotification">Optional parameter to suspend notification of each item added.  If true, a single
    /// notification will be fired upon completion.</param>
    public void AddRange(IEnumerable<T> input, bool suspendNotification = false)
    {
      throw new NotImplementedException("Please complete this feature!");
    }

  }
}
