using System;

namespace drewCo.MathTools
{
  // ============================================================================================================================
  public static class Vector2Extensions
  {
    private const double EPSILON = 1e-10;
    public static bool IsZero(this double d)
    {
      return Math.Abs(d) < EPSILON;
    }
    public static bool IsZero(this float d)
    {
      return Math.Abs(d) < EPSILON;
    }
  }

}
