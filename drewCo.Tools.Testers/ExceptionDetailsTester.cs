using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace drewCo.Tools.Testers
{
  
  // ============================================================================================================================
  [TestClass]
  public class ExceptionDetailsTester
  {


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that V1 exception detail XML can be read in correctly.
    /// </summary>
    [TestMethod]
    public void CanReadVersion1ExceptionDetail()
    {
      string path = FileTools.GetAppDir() + "\\TestData\\V1ExceptionDetail.xml";
      XDocument doc = XDocument.Parse(File.ReadAllText(path));

      ExceptionDetail v1 = ExceptionDetail.FromXML(doc);
      Assert.IsNotNull(v1.InnerException, "The inner exception should not be null!");
      Assert.IsNotNull(v1.InnerException.InnerException, "The inner-inner exception should not be null!");

    }


    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanGetDetailWithAggregateException()
    {
      Exception ex1 = new Exception("1");
      Exception ex2 = new Exception("2");
      AggregateException ae = new AggregateException(new[] { ex1, ex2 });

      // Translate, back and forth.
      XDocument doc = ExceptionDetail.GetExceptionDetailXML(ae);
      ExceptionDetail detail = ExceptionDetail.FromXML(doc);

      Assert.IsNull(detail.InnerException, "There should not be an inner exception listed!");
      Assert.AreEqual(2, detail.InnerExceptions.Count, "There should be two inner (aggregate) exceptions listed!");


    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Makes sure that the stack trace lines are translated to / from XML correctly.
    /// </summary>
    [TestMethod]
    public void CanGetStackTraceLines()
    {

      ExceptionDetail d = null;
      try
      {
        ExceptionThrower();
      }
      catch (Exception ex)
      {
        d = new ExceptionDetail(ex);
      }
      Assert.IsNotNull(d, "We should have an exception detail!");

      // Translate n stuff.
      XDocument doc = d.ToXML();
      ExceptionDetail check = ExceptionDetail.FromXML(doc);

      Assert.IsNotNull(check.StackTrace, "We should have a stack trace!");
      Assert.IsTrue(check.StackTrace.Count > 1, "There should be more than one line in the stack trace!");


    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Used to get deeper stack traces, etc.
    /// </summary>
    private void ExceptionThrower()
    {
      throw new Exception("Kaboom!");
    }
  }

}
