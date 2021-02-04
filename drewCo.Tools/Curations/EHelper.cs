using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace DrewCo.Curations
{

  // ============================================================================================================================
  // TODO: Figure out why I picked this name.... what does it mean??
  public static class EHelper
  {

    // --------------------------------------------------------------------------------------------------------------------------
    public static Stack<T> CreateStack<T>(IEnumerable<T> src)
    {
      int count = src.Count();

      Stack<T> res = new Stack<T>();
      for (int i = 0; i < count; i++)
      {
        res.Push(src.ElementAt(count - 1 - i));
      }
      return res;
    }
  }
}



