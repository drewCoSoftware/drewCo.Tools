using System.Diagnostics;

namespace drewCo.MathTools.Geometry
{

  // ============================================================================================================================
  [DebuggerDisplay("({X},{Y}) : {Radius}")]
  public class Circle : IIntersectsRect
  {
    public double X { get; set; }
    public double Y { get; set; }
    public double Radius { get; set; }

    // --------------------------------------------------------------------------------------------------------------------------
    public Circle(Vector2 pos, double radius)
      : this(pos.X, pos.Y, radius)
    { }

    // --------------------------------------------------------------------------------------------------------------------------
    public Circle(double x, double y, double radius)
    {
      X = x;
      Y = y;
      Radius = radius;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public Vector2 Center
    {
      get { return new Vector2(X, Y); }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public override bool Equals(object obj)
    {
      Circle other = obj as Circle;
      if (other == null) { return false; }

      bool res = this.X == other.X && this.Y == other.Y && this.Radius == other.Radius;
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public bool Intersects(Rectangle rect)
    {
      return rect.Intersects(this);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

  }




}
