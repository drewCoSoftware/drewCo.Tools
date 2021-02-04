using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace drewCo.MathTools.Geometry
{

  // ============================================================================================================================
  [DebuggerDisplay("({P1.X},{P1.Y}) - ({P2.X},{P2.Y})")]
  public struct LineSegment
  {
    public Vector2 P1 { get; set; }
    public Vector2 P2 { get; set; }

    // --------------------------------------------------------------------------------------------------------------------------
    public LineSegment(Vector2 p1, Vector2 p2)
    {
      P1 = p1;
      P2 = p2;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public LineSegment(double p1x, double p1y, double p2x, double p2y)
    {
      P1 = new Vector2(p1x, p1y);
      P2 = new Vector2(p2x, p2y);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public bool Intersects(LineSegment other)
    {
      // NOTE: Try this too....
      // https://gamedev.stackexchange.com/questions/111100/intersection-of-a-line-and-a-rectangle

      Vector2 p = this.P1;
      Vector2 p2 = this.P2;
      Vector2 q = other.P1;
      Vector2 q2 = other.P2;

      var r = p2 - p;
      var s = q2 - q;
      var rxs = r.Cross(s);
      var qpxr = (q - p).Cross(r);

      // If r x s = 0 and (q - p) x r = 0, then the two lines are collinear.
      if (rxs.IsZero() && qpxr.IsZero())
      {
        // 1. If either  0 <= (q - p) * r <= r * r or 0 <= (p - q) * s <= * s
        // then the two lines are overlapping,
        if ((0 <= (q - p) * r && (q - p) * r <= r * r) || (0 <= (p - q) * s && (p - q) * s <= s * s))
        {
          return true;
        }

        // 2. If neither 0 <= (q - p) * r = r * r nor 0 <= (p - q) * s <= s * s
        // then the two lines are collinear but disjoint.
        // No need to implement this expression, as it follows from the expression above.
        return false;
      }

      // 3. If r x s = 0 and (q - p) x r != 0, then the two lines are parallel and non-intersecting.
      if (rxs.IsZero() && !qpxr.IsZero())
      {
        return false;
      }

      // t = (q - p) x s / (r x s)
      var t = (q - p).Cross(s) / rxs;

      // u = (q - p) x r / (r x s)

      var u = (q - p).Cross(r) / rxs;

      // 4. If r x s != 0 and 0 <= t <= 1 and 0 <= u <= 1
      // the two line segments meet at the point p + t r = q + u s.
      if (!rxs.IsZero() && (0 <= t && t <= 1) && (0 <= u && u <= 1))
      {
        // We can calculate the intersection point using either t or u.
        // intersection = p + t * r;

        // An intersection was found.
        return true;
      }

      // 5. Otherwise, the two line segments are not parallel but do not intersect.
      return false;

    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Does this line segment intersect with the given circle?
    /// </summary>
    public bool Intersects(Circle circle)
    {
      // Lovingly copied and adapted from:
      // https://math.stackexchange.com/questions/275529/check-if-line-intersects-with-circles-perimeter

      // Translate everything to origin.
      double ax = P1.X - circle.X;
      double ay = P1.Y - circle.Y;
      double bx = P2.X - circle.X;
      double by = P2.Y - circle.Y;

      double xPart = bx - ax;
      double yPart = by - ay;
      double a = xPart * xPart + yPart * yPart;
      double b = 2 * (ax * (xPart) + ay * (yPart));
      double c = ax * ax + ay * ay - circle.Radius * circle.Radius;

      double disc = b * b - 4 * a * c;
      if (disc <= 0) { return false; }

      double sqrtdisc = Math.Sqrt(disc);
      double t1 = (-b + sqrtdisc) / (2 * a);
      double t2 = (-b - sqrtdisc) / (2 * a);

      if ((0 < t1 && t1 < 1) || (0 < t2 && t2 < 1)) { return true; }
      return false;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public bool Intersects(Polygon polygon)
    {
      return polygon.Intersects(this);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static LineSegment operator -(LineSegment input, Vector2 offset)
    {
      var res = new LineSegment()
      {
        P1 = new Vector2(input.P1.X - offset.X, input.P1.Y - offset.Y),
        P2 = new Vector2(input.P2.X - offset.X, input.P2.Y - offset.Y),
      };
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static LineSegment operator +(LineSegment input, Vector2 offset)
    {
      var res = new LineSegment()
      {
        P1 = new Vector2(input.P1.X + offset.X, input.P1.Y + offset.Y),
        P2 = new Vector2(input.P2.X + offset.X, input.P2.Y + offset.Y),
      };
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static LineSegment operator *(LineSegment input, float scale)
    {
      var res = new LineSegment()
      {
        P1 = new Vector2(input.P1.X * scale, input.P1.Y * scale),
        P2 = new Vector2(input.P2.X * scale, input.P2.Y * scale),
      };
      return res;
    }

  }

}
