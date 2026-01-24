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
    Weekly,
    Monthly,
    Yearly,
    Custom         // Custom time interval...
  }


  // ============================================================================================================================
  public static class DateTools
  {

    // ------------------------------------------------------------------------------------------------
    /// <summary>
    /// Align the given DateTimeOffset to the current hour.
    /// </summary>
    public static DateTimeOffset AlignToHour(DateTimeOffset input)
    {
      var res = new DateTimeOffset(input.Year, input.Month, input.Day, input.Hour, 0, 0, input.Offset);
      return res;
    }

    // ------------------------------------------------------------------------------------------------
    /// <summary>
    /// Align the given DateTimeOffset to the current day.
    /// </summary>
    public static DateTimeOffset AlignToDay(DateTimeOffset input)
    {
      var res = new DateTimeOffset(input.Year, input.Month, input.Day, 0, 0, 0, input.Offset);
      return res;
    }

    // ------------------------------------------------------------------------------------------------
    /// <summary>
    /// Align the given DateTimeOffset to the current day of week.
    /// </summary>
    public static DateTimeOffset AlignToDayOfWeek(DateTimeOffset input, DayOfWeek day)
    {
      int dayDiff = (7 + (input.DayOfWeek - day)) % 7;

      input = input - TimeSpan.FromDays(dayDiff);

      // Align to start of day.
      var res = AlignToDay(input);
      return res;
    }




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
