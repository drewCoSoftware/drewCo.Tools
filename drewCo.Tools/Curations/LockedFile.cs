using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace drewCo.Tools.Curations
{
  // ============================================================================================================================
  public class LockedFile
  {
    private string FilePath = null;
    private int MaxTries = 0;
    private int Interval = 0;

    // --------------------------------------------------------------------------------------------------------------------------
    public LockedFile(string path_)
      : this(path_, 50, 100)          // This is about five seconds of trying....
    { }

    // --------------------------------------------------------------------------------------------------------------------------
    public LockedFile(string path_, int maxTries_, int interval_)
    {
      FilePath = path_;
      MaxTries = maxTries_;
      Interval = interval_;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public Stream GetStream(FileMode mode, FileAccess fileAccess)
    {
      int tryCount = 0;
      while (true)
      {
        try
        {
          Stream res = File.Open(FilePath, mode, fileAccess, FileShare.None);
          return res;
        }
        catch (IOException e)
        {
          if (!IsFileLocked(e))
          {
            throw;
          }

          ++tryCount;
          if (tryCount > MaxTries)
          {
            throw new InvalidOperationException("Operation timed out while trying to access file: {_fileName}!", e);
          }

          Thread.Sleep(Interval);
        }
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private static bool IsFileLocked(IOException exception)
    {
      int errorCode = Marshal.GetHRForException(exception) & ((1 << 16) - 1);
      return errorCode == 32 || errorCode == 33;
    }

  }


}
