//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright ©2024 Andrew A. Ritz, All Rights Reserved
//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
using System;

namespace drewCo.Tools
{

  // ============================================================================================================================
  /// <summary>
  /// This partial class is used to provide some 'mnemonic' type functions to wrap certain functionality, mostly when finding files.
  /// </summary>
  public partial class FileTools
  {
    
    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Find all files in the given directory whos last write time is before the given date.
    /// </summary>
    /// <param name="fromDir"></param>
    /// <param name="date"></param>
    /// <param name="dosNameFilter"></param>
    /// <returns></returns>
    public static string[] FindFilesBefore(string fromDir, DateTime date, string dosNameFilter = FindFilesOptions.DEFAULT_DOS_NAME_FILTER)
    {
      FindFilesOptions ops = ComposeOptions(dosNameFilter, date);

      string[] res = FileTools.FindFiles(fromDir, ops);
      return res;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Find all files in the given directory whos last write time is after the given date.
    /// </summary>
    public static string[] FindFilesAfter(string fromDir, DateTime date, string dosNameFilter = FindFilesOptions.DEFAULT_DOS_NAME_FILTER)
    {
      FindFilesOptions ops = ComposeOptions(dosNameFilter, date);

      string[] res = FileTools.FindFiles(fromDir, ops);
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private static FindFilesOptions ComposeOptions(string dosNameFilter, DateTime date)
    {
      return new FindFilesOptions()
      {
        DOSNameFilter = dosNameFilter,
        Cutoff = date,
        DateCompareType = EDateComparisonType.Before,
      };
    }

  }
}
