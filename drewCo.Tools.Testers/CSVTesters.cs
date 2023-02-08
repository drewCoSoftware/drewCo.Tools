using drewCo.Tools.CSV;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drewCo.Tools.Testers
{


  // ============================================================================================================================
  [TestClass]
  public partial class CSVTesters
  {
    private static string TestFolder = FileTools.GetAppDir() + "\\TestData\\CSV";


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test case was provided to solve a bug where sometimes extra quotes would find their way into
    /// output files.
    /// </summary>
    [TestMethod]
    public void CSVOutputDoesntContainExtraQuotes()
    {

      const int MAX = 1;
      var allDatas = new List<SomeOtherData>();
      for (int i = 0; i < MAX; i++)
      {
        allDatas.Add(new SomeOtherData()
        {
          Content = "xyz",
          Number = 123,
          OtherNumber = 456
        });
      }


      var colMapping = CSVColumnMapping.CreateFromType<SomeOtherData>();
      CSVFile f = new CSVFile(colMapping);
      foreach (var item in allDatas)
      {
        f.AddLineFromData(item);
      }

      const string TEST_FILE_NAME = nameof(CSVOutputDoesntContainExtraQuotes);
      FileTools.DeleteExistingFile(TEST_FILE_NAME);
      f.Save(TEST_FILE_NAME);

      var csvCheck = new CSVFile(TEST_FILE_NAME);

      int lineCount = csvCheck.Lines.Count;
      Assert.AreEqual(1, lineCount);

      for (int i = 0; i < lineCount; i++)
      {
        var srcData = allDatas[i];
        var checkData = csvCheck.Lines[i];

        string srcContent = srcData.Content;
        string checkContent = checkData[1];

        Assert.AreEqual(srcContent, checkContent, "Check content doesn't match source content!");


        // Show that we can deserialize the numbers correctly.....
        var number = int.Parse(checkData[0]);
        Assert.AreEqual(srcData.Number, number);

        var otherNumber = double.Parse(checkData[2]);
        Assert.AreEqual(srcData.OtherNumber, otherNumber);
      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This shows that CSV file will properly escape values so that they can be read back in and preserve the data integrity.
    /// This test case was provided to solve a problem where this wasn't the case.
    /// </summary>
    [TestMethod]
    public void CanReadAndWriteCSVWithQuotedFields()
    {
      string HTML = "<html>" + Environment.NewLine + "\t<p>Dear Diary,</p>" + Environment.NewLine + "</html>";
      var testValues = new[] {
        "Column1",
        "Something, Else!",
        HTML
      };

      var file = new CSVFile(new[] { "Col1", "Other Data", "HTML" });
      file.AddLine(testValues);


      const string TEST_PATH = nameof(CanReadAndWriteCSVWithQuotedFields) + ".csv";
      FileTools.DeleteExistingFile(TEST_PATH);
      file.Save(TEST_PATH);


      // Now we will read it back in....
      // All of the data should be the same.
      var checkFile = new CSVFile(TEST_PATH);
      Assert.AreEqual(1, checkFile.Lines.Count, "There should be a single line in the input file!");

      CSVLine line1 = checkFile.Lines[0];
      for (int i = 0; i < line1.Values.Count; i++)
      {
        Assert.AreEqual(testValues[i], line1[i], $"Values at index: {i} do not match!");
      }

    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test case was provided to solve a specific bug where column values of "" (two quotes in a row) were being parsed out
    /// as " (single quote) instead of an empty string.
    /// </summary>
    [TestMethod]
    public void CanParseColumnsWithOnlyQuotes()
    {
      const string TEST_DATA = "EMPTY,NULL\r\n\"\",";
      string testPath = FileTools.GetAppDir() + $"\\{nameof(CanParseColumnsWithOnlyQuotes)}.csv";
      File.WriteAllText(testPath, TEST_DATA);

      CSVFile f = new CSVFile(testPath);
      var line = f.Lines[0];
      Assert.AreEqual(string.Empty, line[0], "Incorrect value for column data!");

      // Make sure that the second value can still be interpreted as a null.
      Assert.IsNull(line[1], "The second column should be interpreted as a null!");
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test case was provided to fix a bug where quoted values weren't being parsed correctly in the
    /// case of double quoting at the end, i.e  "MyStuff"""
    /// </summary>
    [TestMethod]
    public void CanParseLineWithQuotedValues()
    {
      string testPath = Path.Combine(TestFolder, "ManyQuotes.csv");

      CSVFile f = new CSVFile(testPath);
      var line = f.Lines[0];

      Assert.AreEqual("22.00", line[0], "Incorrect value at index 0!");
      Assert.AreEqual("27.50", line[1], "Incorrect value at index 1!");
      Assert.AreEqual("14\" x 17\"", line[2], "Incorrect value at index 2!");
      Assert.AreEqual("1", line[3], "Incorrect value at index 3!");
      Assert.AreEqual("1", line[4], "Incorrect value at index 4!");

    }



    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanHandleUTF8CharsInCSV()
    {
      string testPath = Path.Combine(TestFolder, "UTF8-Chars.csv");
      CSVFile f = new CSVFile(testPath);

      var line = f.Lines[0];
      Assert.AreEqual(line[0], "Capel Rugs’", "Incorrect value for the line!");

    }


    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanParseCSVDataWithEmbeddedLineBreaks()
    {
      var map = CSVColumnMapping.CreateFromType<SomeOtherData>();

      string testPath = Path.Combine(TestFolder, "EmbeddedBreaks.csv");
      CSVFile f = new CSVFile(testPath);

      var line = f.Lines[0];
      Assert.AreEqual(3, line.Values.Count, "There should only be three values for this line!");

      Assert.AreEqual("123", line[0]);

      string expectedContent = "My Data" + Environment.NewLine + "looks great \"in quotes!\", so let's" + Environment.NewLine + "see what we can do with it";
      Assert.AreEqual(expectedContent, line[1], "Invalid value for 'Content'!");
      Assert.AreEqual("3.14159", line[2]);

    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test case was provided to solve a bug where the first line of CSV files were being omitted in some cases.
    /// </summary>
    [TestMethod]
    public void CanReadCorrectNumberOfLinesFromCSVFile()
    {
      string testPath = FileTools.GetLocalDir("\\TestData") + $"\\{nameof(CanReadCorrectNumberOfLinesFromCSVFile)}.csv";
      string fileData = "TEST,HEADER,STUFF" + Environment.NewLine + "A,B,C";
      File.WriteAllText(testPath, fileData);

      CSVFile f = new CSVFile(testPath);
      Assert.AreEqual(1, f.Lines.Count, "There should be one line in the file!");
    }


    //// --------------------------------------------------------------------------------------------------------------------------
    ///// <summary>
    ///// Not really a test case, but a practical way to convert some data for Dad's wine cellar.
    ///// The first step is that we wanted to detect those entries that had multiple rows of data for each particular wine.
    ///// It is kind of hard to say what the data is meant to represent, but that is OK.
    ///// </summary>
    //[TestMethod]
    //public void CanConvertOldWineDataToNew()
    //{
    //  string srcPath = TestFolder + "\\wine-backup-again.csv";
    //  CSVFile file = new CSVFile(srcPath, "\t");

    //  List<LegacyWineEntry> oldEntries = ExtractWines(file);


    //  // We need to suss out all of the racks + bins.
    //  //List<WineRack> racks = ExtractRacks(oldEntries);


    //  // Now we want to save the data to some other CSV file.....
    //  List<NewWineEntry> newEntries = new List<NewWineEntry>();
    //  int index = 0;

    //  foreach (var entry in oldEntries)
    //  {
    //    newEntries.Add(new NewWineEntry(entry));
    //    ++index;
    //  }

    //  //List<NewWineEntry> newEntries = (from x in oldEntries select new NewWineEntry(x)).ToList();

    //  // Now we will write the contents to disk.
    //  // First the column mappings....
    //  List<CSVColumnMapping> newMaps = CreateColumnMappings(new[]
    //  {
    //    //"Record ID"
    //    //,"Ref Yr"
    //     "Vintage"
    //    , "Winery"
    //    , "Label"
    //    , "Variety"
    //    , "Region"
    //    , "Country"
    //    , "Wine Type"
    //    , "Merit"
    //    , "Value"
    //    , "Best"
    //    , "%Alcohol"
    //    , "Qty"
    //    , "Qty/Case"
    //    , "Bottle Size"
    //    , "Price"
    //    , "User Notes 1"
    //    , "User Notes 2"
    //    //, "Rack Names"
    //    //, "Rack Cols"
    //    //, "Rack Rows"
    //    , "Location"
    //    , "UserField3"
    //  });

    //  CSVFile output = new CSVFile(newMaps);

    //  IEnumerable<NewWineEntry> useEntries = (from x in newEntries where x.Qty != "0" select x); ///.Take(10);
    //  foreach (var e in useEntries)
    //  {
    //    output.AddLine(e);
    //  }

    //  string outputPath = FileTools.GetAppDir() + "\\TestOutput";
    //  FileTools.CreateDirectory(outputPath);
    //  outputPath += "\\NewFormat.csv";
    //  output.Save(outputPath);


    //  //      Assert.AreEqual(15, entries.Count, "Incorrect number of entries!");

    //  // TODO: Check one for correct address info / notes....

    //}

    // --------------------------------------------------------------------------------------------------------------------------
    private static Dictionary<string, string> LocationTranslators = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
    };

    private static Dictionary<string, string> PositionTranslators = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
      { "Top Row", "Row 1" },
      { "Bottom Row", "Row 5" },
      { "On Left", "" },
      { "From Left", "" },
      { "From Top", "" },
      { "From Bottom", "" },

      { "Right Row", "Row 3" },

      { "Middle Bin", "Bin 2" },
      { "Bottom Bin", "Bin 3" },

      { "1st row", "Row 1" },
      { "2nd row", "Row 2" },
      { "3rd row", "Row 3" },
      { "4th row", "Row 4" },
      { "5th row", "Row 5" },
      { "1st bin", "Bin 1" },
      { "2nd bin", "Bin 2" },
      { "3rd bin", "Bin 3" },
      { "4th bin", "Bin 4" },
      { "5th bin", "Bin 5" },
      { "left bin", "Bin 1" },
      { "right bin", "Bin 2" }

      ,{ "Left", "Bin 1" }
      ,{ "Center", "Bin 2"}
      ,{ "Right", "Bin 3" }
    };


    // --------------------------------------------------------------------------------------------------------------------------
    private List<WineRack> ExtractRacks(List<LegacyWineEntry> oldEntries)
    {

      HashSet<string> uniqueNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      foreach (var e in oldEntries)
      {
        string loc = e.Location.Trim();
        if (LocationTranslators.TryGetValue(loc, out string newLoc))
        {
          loc = newLoc;
        }

        string pos = e.Position.Trim().ToLower();
        pos = pos.Replace(",", "/");
        pos = pos.Replace("/", " / ");
        pos = StringTools.RemoveExtraSpaces(pos);
        foreach (var item in PositionTranslators)
        {
          pos = pos.Replace(item.Key.ToLower(), item.Value.ToLower());
        }
        pos = StringTools.UppercaseFirstLetterOfEachWord(pos);
        pos = pos.Trim();

        //if (PositionTranslators.TryGetValue(pos, out string newPos))
        //{
        //  pos = newPos;
        //}

        string rackName = $"{loc}-{pos}";
        if (!uniqueNames.Contains(rackName))
        {
          uniqueNames.Add(rackName);
        }
      }


      var res = (from x in uniqueNames
                 select new WineRack()
                 {
                   Name = x,
                 }).ToList();

      return res;
    }

    /// <summary>
    /// Create a list of column mappings from a list of strings.
    /// </summary>
    private List<CSVColumnMapping> CreateColumnMappings(string[] colNames)
    {
      var res = new List<CSVColumnMapping>();
      int index = 0;
      foreach (var name in colNames)
      {
        var map = new CSVColumnMapping()
        {
          Index = index,
          Name = name
        };
        res.Add(map);

        ++index;
      }

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private List<LegacyWineEntry> ExtractWines(CSVFile file)
    {
      List<LegacyWineEntry> res = new List<LegacyWineEntry>();


      List<CSVLine> useLines = file.Lines;

      LegacyWineEntry current = null;
      int internalIndex = 0;
      foreach (var l in useLines)
      {
        // See if we are parsing a new entry.
        if (l["Vintage"] != "" && l["Type"] != "")
        {
          internalIndex = 0;
          current = l.CreateData<LegacyWineEntry>();
          res.Add(current);

          // Some cleanup...
          if (current.Bottles == "") { current.Bottles = "0"; }

        }
        else
        {
          // This is a continuation of the current entry.
          // We are just shoving the data into the 'Extended Notes' section since I don't know what else to do with it.
          string notes = l["Winery"];
          current.ExtendedNotes += (internalIndex > 0 ? Environment.NewLine : "") + notes;
          ++internalIndex;
        }
      }



      return res;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanOpenCSVFile()
    {
      string srcPath = TestFolder + "\\wine-backup-again.csv";
      CSVFile file = new CSVFile(srcPath, "\t");

      const int EXPECTED = 29;
      Assert.AreEqual(EXPECTED, file.Columns.Count, $"There should be {EXPECTED} columns in this file!");
    }


  }


  // ============================================================================================================================
  public class WineRack
  {
    public string Name { get; set; }
  }

  // ============================================================================================================================
  public class SomeOtherData
  {
    public int Number { get; set; }
    public string Content { get; set; }
    public double OtherNumber { get; set; }
  }

}
