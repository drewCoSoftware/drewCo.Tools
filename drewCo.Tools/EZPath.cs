
namespace drewCo.Tools;

    // TODO: This and its associated test case should go live with drewCo.Tools.  Hell, it deserves a
    // writeup on that projects readme.md file!
    // ============================================================================================================================
    /// <summary>
    /// This is like the 'Path' object from python's 'pathlib' library.  It is meant to be as easy to use, and
    /// allows us to use operators vs. constant 'Path.Combine' calls when computing cross-platform paths.
    /// </summary>
    public class EZPath
    {
      private string _Path;

      // --------------------------------------------------------------------------------------------------------------------------
      public EZPath(string path_)
      {
        _Path = path_;
      }
      public override string ToString() { return _Path; }

      // --------------------------------------------------------------------------------------------------------------------------
      public static EZPath operator /(EZPath parent, EZPath part)
      {
        string res = parent.ToString() + Path.DirectorySeparatorChar + part;
        return new EZPath(res);
      }

      // --------------------------------------------------------------------------------------------------------------------------
      public static EZPath operator /(EZPath parent, string part)
      {
        string res = parent.ToString() + Path.DirectorySeparatorChar + part;
        return new EZPath(res);
      }

      // --------------------------------------------------------------------------------------------------------------------------
      public static implicit operator string(EZPath p)
      {
        string res = p.ToString();
        return res;
      }
    }
