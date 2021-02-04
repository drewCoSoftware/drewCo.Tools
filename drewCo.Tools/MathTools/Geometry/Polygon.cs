using drewCo.Curations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace drewCo.MathTools.Geometry
{


  // ============================================================================================================================
  public class Polygon
  {
    // NOTE: I think that I can just make points / segments the readonly collections we see below.
    private List<Vector2> _Points = null;
    private List<LineSegment> _Segments = null;

    public ReadOnlyCollection<Vector2> Vertices { get; private set; }
    public ReadOnlyCollection<LineSegment> Sides { get; private set; }

    // --------------------------------------------------------------------------------------------------------------------------
    public Polygon(IList<Vector2> points_)
    {
      _Points = new List<Vector2>();
      _Points.AddRange(points_);
      Vertices = new ReadOnlyCollection<Vector2>(_Points);

      // TODO: Make sure that our vertex order is always counter-clock (the same direction of 2d rotation angles).
      // If not, sort them!
      // Compose the segments.
      _Segments = new List<LineSegment>();
      int len = _Points.Count;
      for (int i = 0; i < len; i++)
      {
        int nextIndex = (i + 1) % len;
        _Segments.Add(new LineSegment(_Points[i], _Points[nextIndex]));
      }

      Sides = new ReadOnlyCollection<LineSegment>(_Segments);
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


  // ============================================================================================================================
  [Obsolete("Use type 'Polygon'.  These functions will be removed in an upcoming verison!")]
  public static class PolygonTools
  {
    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Given a polygon, as represented by a set of points, this will tell us if <paramref name="point"/> is contained within it.
    /// </summary>
    /// <remarks>
    /// This code was originally by Bob Stein.  I lifted it from a C-Listing online at;
    /// http://www.linuxjournal.com/article/2029
    /// I have modified to use floating points and stuff like that.  Thanks Bob!
    /// </remarks>
    public static bool IsPointInPolygon(List<Vector2> poly, Vector2 point)
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
    /// Tells us if the given line segment overlaps (intersects with) the polygon, which is represented by a list
    /// of line segments.
    /// </summary>
    public static bool Overlaps(LineSegment segment, List<LineSegment> polygon)
    {
      throw new NotSupportedException();
    }

  }
}
