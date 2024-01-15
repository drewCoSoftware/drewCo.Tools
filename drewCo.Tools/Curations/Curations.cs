using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drewCo.Tools.Curations
{
  // ============================================================================================================================
  public static class Curations
  {
    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This is like the Max() linq extension method, but will return a fallback value if <paramref name="src"/> is null or empty.
    /// </summary>
    public static int SafeMax(this IEnumerable<int> src, int fallback = -1)
    {
      if (src == null || src.Count() == 0) { return fallback; }
      int res = src.Max();
      return res;
    }
  }


}
