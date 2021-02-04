//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright ©2017-2019 Andrew A. Ritz, All Rights Reserved
//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Collections.Generic;
using System.Diagnostics;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace drewCo.Tools
{

  // ============================================================================================================================
  public class GopherRegisteredEventArgs : EventArgs
  {
    // --------------------------------------------------------------------------------------------------------------------------
    public GopherRegisteredEventArgs(object registered_)
    {
      Registered = registered_;
    }

    public object Registered { get; private set; }
  }

  // ============================================================================================================================
  /// <summary>
  /// This is a simple service locator that we use for singletons.
  /// </summary>
  public static class Gopher
  {
    public static event EventHandler<GopherRegisteredEventArgs> Registered;

    // Some defined log codes.  You can map these to your own categorization stuff. 
    // (which is why we don't have an enum).
    public const int LOG_CATEGORY_MSG = 0;
    public const int LOG_CATEGORY_DEFAULT = LOG_CATEGORY_MSG;
    public const int LOG_CATEGORY_ERROR = 1;
    public const int LOG_CATEGORY_SUCCESS = 2;
    public const int LOG_CATEGORY_WARNING = 3;

    private static Dictionary<Type, object> Items = new Dictionary<Type, object>();

    /// <summary>
    /// Pointer to a generic logging function that takes a message, and message category (as an int) code.
    /// </summary>
    private static Action<string, int> LogFunc = null;

    // --------------------------------------------------------------------------------------------------------------------------
    public static void Register<T>(T instance, bool overwriteExisting = false)
    {
      if (instance == null) { throw new ArgumentNullException("instance"); }
      Type t = typeof(T);

      if (overwriteExisting)
      {
        Items[t] = instance;
      }
      else
      {
        Items.Add(t, instance);
      }

      Registered?.Invoke(null, new GopherRegisteredEventArgs(instance));
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static T Get<T>()
    {
      Type t = typeof(T);

      object res = null;
      Items.TryGetValue(t, out res);
      return (T)res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void SetLogFunc(Action<string, int> func_)
    {
      if (func_ == null) { throw new ArgumentNullException("func_"); }
      LogFunc = func_;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void Log(string msg, int msgCateogry = LOG_CATEGORY_DEFAULT)
    {
      if (LogFunc != null)
      {
        LogFunc.Invoke(msg, msgCateogry);
      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public static void LoggedTask(string startMsg, string completeMsg, Action task)
    {
      LoggedTask(startMsg, completeMsg, "There was an error!", task);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void LoggedTask(string startMsg, string completeMsg, string errMsg, Action task, Action<Exception> errHandler = null, bool rethrow = false)
    {
      LoggedTaskOptions ops = new LoggedTaskOptions()
      {
        StartMessage = startMsg,
        CompleteMessage = completeMsg,
        ErrorMessage = errMsg,
        ErrorHandler = errHandler,
        RethrowExceptions = rethrow,
      };

      LoggedTask(task, ops);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void LoggedTask(Action task, LoggedTaskOptions options)
    {
      var sw = Stopwatch.StartNew();

      Gopher.Log(options.StartMessage);
      try
      {
        task.Invoke();
      }
      catch (Exception ex)
      {
        Gopher.Log(options.ErrorMessage, Gopher.LOG_CATEGORY_ERROR);
        Gopher.Log(ex.Message, Gopher.LOG_CATEGORY_ERROR);

        options.ErrorHandler?.Invoke(ex);

        if (options.RethrowExceptions)
        {
          throw;
        }

        return;
      }

      Gopher.Log(options.CompleteMessage + (options.ReportTime ? $"\t({sw.Elapsed.TotalSeconds:f3}s)" : ""));

     const bool INCLUE_BREAK = true;
     if (INCLUE_BREAK)
     {
      Gopher.Log("");
     }
    }


  }

  // ============================================================================================================================
  public class LoggedTaskOptions
  {
    public string StartMessage { get; set; }
    public string CompleteMessage { get; set; }
    public string ErrorMessage { get; set; }

    public Action<Exception> ErrorHandler { get; set; } = null;
    public bool RethrowExceptions { get; set; } = false;
    public bool ReportTime { get; set; } = true;
  }

}
