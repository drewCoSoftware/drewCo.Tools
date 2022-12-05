using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// NOTE: This should go into the tools lib.
namespace drewCo.Tools
{

  // ============================================================================================================================
  /// <summary>
  /// Describes generic time intervals.
  /// </summary>
  public enum ETimeInterval
  {
    Invalid = 0,
    Always,           // Indicates that time intervals don't apply in this case.
    Once,
    Hourly,
    Daily,
    Monthly,
    Weekly,
    Custom         // Custom time interval...
  }


  // ============================================================================================================================
  public static class DateTools
  {

    // --------------------------------------------------------------------------------------------------------------------------
    public static DateTimeOffset IncrementInterval(DateTimeOffset current, ETimeInterval interval)
    {
      switch (interval)
      {
        case ETimeInterval.Monthly:
          current = current.AddMonths(1);
          return current;

        default:
          throw new ArgumentOutOfRangeException();
      }
      throw new NotImplementedException();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static DateTimeOffset AlignDateToInterval(DateTimeOffset start, ETimeInterval interval, int offset = 1)
    {
      switch (interval)
      {
        case ETimeInterval.Monthly:
          start = new DateTimeOffset(start.Year, start.Month, offset, 0, 0, 0, TimeSpan.Zero);
          return start;

        default:
          throw new ArgumentOutOfRangeException();
      }
    }
  }
}
