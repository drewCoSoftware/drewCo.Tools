using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using drewCo.Tools.Logging;
using NUnit.Framework;

namespace drewCo.Tools.Core.Testers;

// ==============================================================================================================================
public class TestLogger : ConsoleLogger
{
  public string LastMessage = null;

  // --------------------------------------------------------------------------------------------------------------------
  public override void WriteToLog(string message)
  {
    LastMessage = message;
  }
}

// ==============================================================================================================================
/// <summary>
/// Makes sure that our logger does WTF we intend it to do.
/// </summary>
public class LoggerTesters
{


  // --------------------------------------------------------------------------------------------------------------------
  /// <summary>
  /// This test case was provided to solve a bug where default logger options effectively
  /// disabled all messages.
  /// </summary>
  [Test]
  public void DefaultLoggerOptionsIncludeAllLevels()
  {
    var logger = new TestLogger();
    Assert.IsTrue(logger.HasLogLevel("whatever"));

    Assert.IsNull(logger.LastMessage);

    const string TEST_STRING = "some message....";
    logger.WriteLine("awugnpawnug", TEST_STRING);

    Assert.That(logger.LastMessage , Is.EqualTo(TEST_STRING + Environment.NewLine));  
  }

  // --------------------------------------------------------------------------------------------------------------------------
  /// <summary>
  /// This test was provided to make sure that we can get the contents of a log file when we need it.
  /// This is mainly used in cases where we need to read-back or post the current state of the log to
  /// some external service.
  /// </summary>
  [Test]
  public void CanGetCurrentLogDataFromFile()
  {

    string testPath = $"log-{nameof(CanGetCurrentLogDataFromFile)}.txt";
    FileTools.DeleteExistingFile(testPath);
    Assert.IsFalse(File.Exists(testPath));

    var ops = new FileLoggerOptions(null, "./", testPath, null, EFileLoggerMode.Append);
    var l = new FileLogger(ops);

    const string TEST_MSG_1 = "abc";
    const string TEST_MSG_2 = "def";
    const string TEST_MSG_3 = "ghi";

    l.Info(TEST_MSG_1);
    l.Info(TEST_MSG_2);

    string curContent = l.GetLogFileContent();
    Assert.IsNotNull(curContent);
    Assert.AreEqual(curContent, TEST_MSG_1 + Environment.NewLine + TEST_MSG_2 + Environment.NewLine);

    l.Info(TEST_MSG_3);
    l.Dispose();

    string fileContent = File.ReadAllText(testPath);

    Assert.AreNotEqual(fileContent, curContent);

  }

}
