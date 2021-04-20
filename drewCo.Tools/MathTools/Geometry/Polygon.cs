using drewCo.Curations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace drewCo.MathTools.Geometry
{

  // ============================================================================================================================
  /// <summary>
  /// Allows the size of a shape to be changed by dynamically scaling it.
  /// </summary>
  /// <remarks>
  /// This interface may change to a generic version to create a scaled copy vs. changing the existing instance.
  /// </remarks>
  interface ICanScale
  {
    void Scale(double scale);
    void Scale(double scaleX, double scaleY);
  }

  // ============================================================================================================================
  interface IBoundingBox2D
  {
    Rectangle GetAABB();
  }

  // ============================================================================================================================
  public class Polygon : IBoundingBox2D
  {
    // NOTE: I think that I can just make points / segments the readonly collections we see below.
    // --> true.  There is no need to modify the vertices / segments collections.
    private List<Vector2> _Vertices = null;
    private List<LineSegment> _Segments = null;

    public ReadOnlyCollection<Vector2> Vertices { get; private set; }
    public ReadOnlyCollection<LineSegment> Sides { get; private set; }

    // --------------------------------------------------------------------------------------------------------------------------
    public Polygon(IList<Vector2> vertices_)
    {
      _Vertices = new List<Vector2>();
      _Vertices.AddRange(vertices_);

      // Make sure that verts are always in clockwise order.
      // This allows us to compute vertex angles and do other math consistently.
      if (!IsClockwise(_Vertices))
      {
        _Vertices.Reverse();
      }
      Vertices = new ReadOnlyCollection<Vector2>(_Vertices);

      RecomputeSegments();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private void RecomputeSegments()
    {
      _Segments = new List<LineSegment>();
      int len = _Vertices.Count;
      for (int i = 0; i < len; i++)
      {
        int nextIndex = (i + 1) % len;
        _Segments.Add(new LineSegment(_Vertices[i], _Vertices[nextIndex]));
      }
      Sides = new ReadOnlyCollection<LineSegment>(_Segments);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public Rectangle GetAABB()
    {
      double minX = double.MaxValue;
      double maxX = double.MinValue;
      double minY = double.MaxValue;
      double maxY = double.MinValue;

      foreach (var v in Vertices)
      {
        minX = Math.Min(minX, v.X);
        maxX = Math.Max(maxX, v.X);
        minY = Math.Min(minY, v.Y);
        maxY = Math.Max(maxY, v.Y);
      }

      return new Rectangle(minX, minY, maxX - minX, maxY - minY);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Scale(double scale)
    {
       Scale(scale, scale);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Scale(double scaleX, double scaleY)
    {
      int len = Vertices.Count;
      for (int i = 0; i < len; i++)
      {
        _Vertices[i] = new Vector2(_Vertices[i].X * scaleX, _Vertices[i].Y * scaleY);
      }
      Vertices = new ReadOnlyCollection<Vector2>(_Vertices);
      RecomputeSegments();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Tells us if this polygon entirely contains the given rectangle.
    /// </summary>
    public bool Contains(Rectangle rect)
    {
      var p = rect.ToPolygon();
      return Contains(p);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Tells us if this polygon entirely contains the given polygon.
    /// </summary>
    public bool Contains(Polygon p)
    {
      foreach (var vert in p.Vertices)
      {
        if (PolygonContainsPoint(this.Vertices, vert))
        {
          return true;
        }
      }

      return false;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Given a polygon, as represented by a set of points, this will tell us if <paramref name="point"/> is contained within it.
    /// </summary>
    /// <remarks>
    /// This code was originally by Bob Stein.  I lifted it from a C-Listing online at;
    /// http://www.linuxjournal.com/article/2029
    /// I have modified to use floating points and stuff like that.  Thanks Bob!
    /// </remarks>
    public static bool PolygonContainsPoint(IList<Vector2> poly, Vector2 point)
    {
      double xnew, ynew;
      double xold, yold;
      double x1, y1;
      double x2, y2;
      int npoints = poly.Count;
      bool inside = false;

      if (npoints < 3)
      {
        return (false);
      }

      xold = poly[npoints - 1].X;
      yold = poly[npoints - 1].Y;

      for (int i = 0; i < npoints; i++)
      {
        xnew = poly[i].X;
        ynew = poly[i].Y;
        if (xnew > xold)
        {
          x1 = xold;
          x2 = xnew;
          y1 = yold;
          y2 = ynew;
        }
        else
        {
          x1 = xnew;
          x2 = xold;
          y1 = ynew;
          y2 = yold;
        }
        /* edge "open" at left end */
        if ((xnew < point.X) == (point.X <= xold) && (point.Y - y1) * (x2 - x1) < (y2 - y1) * (point.X - x1))
        {
          inside = !inside;
        }
        xold = xnew;
        yold = ynew;
      }
      return (inside);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Tells us if the ordering of the points is clockwise or not.
    /// </summary>
    public static bool IsClockwise(IList<Vector2> points)
    {
      // This was taken from some code on the internet....
      double sum = 0;
      for (int i = 0; i < points.Count - 1; i++)
      {
        double part = ComputeAreaPart(points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y);
        sum += part;
      }

      // And the final piece.
      sum += ComputeAreaPart(points[points.Count - 1].X, points[points.Count - 1].Y, points[0].X, points[0].Y);
      return sum < 0;

    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void MakeClockwise(List<Vector2> points)
    {
      if (!IsClockwise(points))
      {
        points.Reverse();
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private static double ComputeAreaPart(double x1, double y1, double x2, double y2)
    {
      return (x2 - x1) * (y1 + y2);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public bool Intersects(LineSegment segment)
    {
      foreach (var s in _Segments)
      {
        if (s.Intersects(segment))
        {
          return true;
        }
      }
      return false;
    }

  }

}
