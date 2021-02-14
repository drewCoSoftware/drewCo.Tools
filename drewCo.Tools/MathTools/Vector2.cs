using System;
using System.Diagnostics;
using System.Windows;

namespace drewCo.MathTools
{
  // TODO: The canvas needs something done to it so that the shapes can't fall outside of its drawing area when we move them about.
  // --> Clipping area is the solution I think....
  // http://social.msdn.microsoft.com/forums/en-US/wpf/thread/e060205e-5bfc-4f21-bf80-dfa55c44eb8a should complete the process.

  // TODO: I am thinking that the canvas is going to need some type of scrollbar action for the shapes that are defined outside of
  // the area.  Maybe we have a completely bound region? --> I do like the idea of just having it adjust to itself though.

  // ============================================================================================================================
  public static class Trigonometry
  {
    public const double TO_RAD = System.Math.PI / 180.0d;
    public const double TO_DEG = 180.0d / System.Math.PI;
  }


  // ============================================================================================================================
  [DebuggerDisplay("{X},{Y}")]
  public struct Vector2
  {
    // --------------------------------------------------------------------------------------------------------------------------
    public Vector2(double angle)
    {
      X = Math.Cos(angle);
      Y = Math.Sin(angle);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public Vector2(float angle)
    {
      X = Math.Cos(angle);
      Y = Math.Sin(angle);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public Vector2(double x, double y) { X = x; Y = y; }

    public static Vector2 Zero { get { return new Vector2(0.0d, 0.0d); } }

    // --------------------------------------------------------------------------------------------------------------------------
    public Vector2(Point p)
    {
      X = p.X;
      Y = p.Y;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Set(double x, double y)
    {
      X = x;
      Y = y;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void SetZero()
    {
      X = 0.0d;
      Y = 0.0d;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public bool IsZero()
    {
      return X == 0.0d && Y == 0.0d;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public override int GetHashCode()
    {
      return X.GetHashCode() ^ Y.GetHashCode();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public override bool Equals(object obj)
    {
      if (obj != null)
      {
        Vector2 other = (Vector2)obj;
        return other == this;
      }
      return false;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static bool operator ==(Vector2 p1, Vector2 p2)
    {
      return p1.X == p1.X && p1.Y == p2.Y;
    }
    // --------------------------------------------------------------------------------------------------------------------------
    public static bool operator !=(Vector2 p1, Vector2 p2)
    {
      return p1.X != p1.X || p1.Y != p2.Y;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static Vector2 operator +(Vector2 input, Point p)
    {
      Vector2 res = new Vector2(input.X + p.X, input.Y + p.Y);
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static Vector2 operator +(Vector2 p1, Vector2 p2)
    {
      Vector2 res = new Vector2(p1.X + p2.X, p1.Y + p2.Y);
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static Vector2 operator -(Vector2 p1, Vector2 p2)
    {
      Vector2 res = new Vector2(p1.X - p2.X, p1.Y - p2.Y);
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static double operator *(Vector2 l, Vector2 r)
    {
      return l.Dot(r);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static Vector2 operator *(Vector2 input, double scale)
    {
      Vector2 res = new Vector2(input.X * scale, input.Y * scale);
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static Vector2 operator -(Vector2 input)
    {
      return new Vector2(-input.X, -input.Y);
    }

    public double X { get; set; }
    public double Y { get; set; }

    public double Length() { return Math.Sqrt(X * X + Y * Y); }
    public double LengthSquared() { return X * X + Y * Y; }

    // --------------------------------------------------------------------------------------------------------------------------
    public static double Dot(Vector2 p1, Vector2 p2)
    {
      return p1.Dot(p2);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public double Dot(Vector2 other)
    {
      double res = X * other.X + Y * other.Y;
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static double Distance(Vector2 v1, Vector2 v2)
    {
      double x = v1.X - v2.X;
      double y = v1.Y - v2.Y;
      double res = Math.Sqrt(x * x + y * y);
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public double Angle()
    {
      // NOTE: This isn't correct.....
      // We need it normalized....
      var v = Norm();
      double res = Math.Acos(v.X);

      return Math.Atan(v.Y / v.X);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    // Get a normalized copy of this vector.
    public Vector2 Norm()
    {
      // There is probably a better way to compute this...
      double len = Length();
      if (len == 0.0d)
      {
        return new Vector2();
      }

      Vector2 res = new Vector2(X / len, Y / len);
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public Vector2 ScaleTo(double newLength)
    {
      double len = Length();
      if (len == 0.0d)
      {
        return new Vector2();
      }

      Vector2 res = new Vector2(X / len, Y / len) * newLength;
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static Vector2 Average(Vector2 v1, Vector2 v2)
    {
      var x = (v2.X + v1.X) / 2.0f;
      var y = (v2.Y + v1.Y) / 2.0f;

      return new Vector2(x, y);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public Point ToPoint()
    {
      return new Point(X, Y);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static Vector2 Rotate(double angle, Vector2 point, Vector2 center)
    {
      double px = point.X - center.X;
      double py = point.Y - center.Y;

      double rx = px * Math.Cos(angle) - py * Math.Sin(angle);
      double ry = py * Math.Cos(angle) + px * Math.Sin(angle);

      // Put it all back together with the offset.
      Vector2 res = new Vector2(rx, ry) + center;
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Rotate this vector around the origin.
    /// </summary>
    public Vector2 Rotate(double angle)
    {
      return Rotate(angle, this, new Vector2());
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Get the cross product of two vectors.
    /// </summary>
    public static double Cross(Vector2 v1, Vector2 v2)
    {
      double res = v1.X * v2.Y - v1.Y * v2.X;
      return res;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public double Cross(Vector2 v)
    {
      return X * v.Y - Y * v.X;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Compute a vector, perpendicular to this one, rotated by the given direction.
    /// </summary>
    /// <param name="dir">
    /// The direction that the perpendicular vector should be, relative to this one.  1 = LEFT, -1 = RIGHT.
    /// This corresponds to rotation angles on the cartesian plane.
    /// </param>
    /// <remarks>
    /// Weird stuff will happen with zero vectors, or if the directions isn't 1.0 or -1.0
    /// </remarks>
    public Vector2 Perpendicular(double dir)
    {
      // We can just do a rotation, +/- 90degress since we want the perpendicular.
      // This allows us all kinds of shortcuts.
      //double rx = px * Math.Cos(angle) - py * Math.Sin(angle);
      //double ry = py * Math.Cos(angle) + px * Math.Sin(angle);

      double sinA = dir; // * -1.0d;        // This is always +/- sin(pi/2) or +/- 1
      double cosA = 0;                  // This is always zero. 
      //double rx = px * cosA - py * sinA;
      //double ry = py * cosA + px * sinA;

      // Even simpler....
      double rx = -Y * sinA;
      double ry = X * sinA;

      Vector2 res = new Vector2(rx, ry);
      return res;

    }

    // --------------------------------------------------------------------------------------------------------------------------
    public override string ToString()
    {
      return $"{X}, {Y}";
    }

  }

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
