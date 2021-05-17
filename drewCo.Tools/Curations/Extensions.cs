// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright (c)2010-2014 Andrew A. Ritz, all rights reserved.
// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace drewCo.Curations.Extensions
{

  // ============================================================================================================================
  /// <summary>
  /// Exposes the helper functions as extension methods for those who prefer this technique.
  /// </summary>
  public static class IEnumHelperExtensions
  {
    // --------------------------------------------------------------------------------------------------------------------------
    public static Stack<T> ToStack<T>(this IEnumerable<T> src)
    {
      return IEnumHelper.ToStack(src);
    }
  }

}
