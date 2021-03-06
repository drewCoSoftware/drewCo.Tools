﻿//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright ©2013-2015 Andrew A. Ritz, All Rights Reserved
//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace drewCo.Tools
{
  // ============================================================================================================================
  public interface IDeepCopy<T> where T : class
  {
    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// Create a deep copy of the class.
    /// </summary>
    /// <returns></returns>
    T DeepCopy();
  }
}
