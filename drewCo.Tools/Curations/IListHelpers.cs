﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace drewCo.Tools.Curations
{
  public static class IListHelpers
  {

    // --------------------------------------------------------------------------------------------------------------------------
    // Thanks internet!
    //https://stackoverflow.com/questions/273313/randomize-a-listt
    public static void Shuffle<T>(IList<T> list, Random rng)
    {
      int n = list.Count;
      while (n > 1)
      {
        n--;
        int k = rng.Next(n + 1);
        T value = list[k];
        list[k] = list[n];
        list[n] = value;
      }
    }

  }
}
