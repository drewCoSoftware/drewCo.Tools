using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace drewCo.MathTools
{
  public static class Quadratic
  {

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Solves the quadratic formula, returning the roots (one root for same value) or null if there is no solution.
    /// </summary>
    /// <remarks>
    /// We return a null, because we don't support complex numbers when the discriminant is less than zero.
    /// </remarks>
    public static Tuple<double, double> SolveQuadratic(double a, double b, double c)
    {

      double discrimnant = Math.Sqrt(b * b - 4.0d * a * c);
      if (discrimnant < 0.0d)
      {
        return null;
      }

      double solution1 = (-b + discrimnant) / (2.0d * a);
      double solution2 = (-b - discrimnant) / (2.0d * a);

      var res = new Tuple<double, double>(solution1, solution2);
      return res;

    }

  }
}
