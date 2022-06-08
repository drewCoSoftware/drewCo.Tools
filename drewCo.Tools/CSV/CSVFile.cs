//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright ©2019-2020 Andrew A. Ritz, All Rights Reserved
//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace drewCo.Tools.CSV
{
  // ============================================================================================================================
  /// <summary>
  /// Represents CSV data on disk.
  /// </summary>
  public class CSVFile
  {

    public List<CSVColumnMapping> Columns { get; set; } = new List<CSVColumnMapping>();
    public CSVColumnMap ColumnMap { get; set; }
    public List<CSVLine> Lines { get; set; } = new List<CSVLine>();

    /// <summary>
    /// Defines how each column is separated.
    /// </summary>
    public string Separator { get; set; } = ",";

    // --------------------------------------------------------------------------------------------------------------------------
    public CSVFile(IList<string> colNames)
    {
      var cols = new List<CSVColumnMapping>();

      int index = 0;
      foreach (var c in colNames)
      {
        cols.Add(new CSVColumnMapping()
        {
          Index = index,
          Name = c
        });
        ++index;
      }

      Columns = cols;
      ColumnMap = new CSVColumnMap(Columns);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public CSVFile(List<CSVColumnMapping> cols)
    {
      Columns = new List<CSVColumnMapping>(cols);
      ColumnMap = new CSVColumnMap(Columns);
      Separator = ",";
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public CSVFile(string path, string separator = ",", bool includeHeader = true, bool trimWhitespace = true)
    {
      if (!File.Exists(path))
      {
        throw new FileNotFoundException($"The file at path: {path} does not exist!");
      }
      string[] splitWith = new[] { separator };

      Separator = separator;

      using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
      using (StreamReader r = new StreamReader(fs, Encoding.UTF8))
      {
        // r.Read(

        string line = GetNextLine(r);
        string[] firstParts = line.Split(splitWith, StringSplitOptions.None);

        if (includeHeader)
        {
          int index = 0;
          foreach (var p in firstParts)
          {
            var cmap = new CSVColumnMapping()
            {
              Index = index,
              Name = GetColumnValue(firstParts, index, trimWhitespace)
            };
            Columns.Add(cmap);
            ++index;
          }
          // it.MoveNext();
        }
        else
        {
          // The first line is to be treated as data, so we will do that, but generate column names.
          for (int i = 0; i < firstParts.Length; i++)
          {
            var cmap = new CSVColumnMapping()
            {
              Index = i,
              Name = $"Column_{i + 1}",
            };
            Columns.Add(cmap);
          }
        }



        // Now we can process all of the data......
        Lines = new List<CSVLine>();
        CSVColumnMap useCols = new CSVColumnMap(Columns);

        while ((line = GetNextLine(r)) != null)
        {
          CSVLine csvLine = new CSVLine(useCols, line, separator);
          Lines.Add(csvLine);
        }

      }

      return;

    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Get the next line of data from the file stream.
    /// </summary>
    private string GetNextLine(StreamReader fs)
    {
      // Check to see if we are all done?
      if (fs.EndOfStream) { return null; }

      string buffer = null;

      bool inQuotes = false;

      while (true)
      {
        char c = (char)fs.Read();

        // I want to check for a line break, but update the buffer if we are currently in a field...
        if (!inQuotes)
        {
          // A line break can be a carriage return (CR) or a line feed (LF) or a (CRLF)
          // If we are currently reading in a field, we just add these characters to the buffer.
          if (c == 0x0D)
          {
            // Check the next char....
            // If we don't have a linefeed, leave the read-head where it is.
            // In either case we return the current buffer.
            if (PeekNextChar(fs, out char next))
            {
              if (next == 0x0A)
              {
                // Update read head.
                fs.Read();
              }
            }
            else
            {
              // EOF:
              break;
            }

            return buffer;
          }
          else if (c == 0x0A)
          {
            return buffer;
          }

          // Buffer the character.
          buffer += c;

          inQuotes = c == '\"';
        }
        else
        {
          buffer += c;

          if (inQuotes && c == '\"')
          {
            // We are already in a quote block, but this may not be the end.
            // We need to check for further escaped quotes.
            if (PeekNextChar(fs, out char peek))
            {
              if (peek == '\"')
              {
                // This is a double quote, so we will toss the character onto the buffer and update the read head.
                buffer += peek;
                fs.Read();
              }
              else
              {
                // The quote block has ended.
                inQuotes = false;
              }
            }
            else
            {
              // EOF:
              break;
            }
          }
        }

        // EOF:
        if (fs.EndOfStream) { break; }
      }


      return buffer;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private bool PeekNextChar(StreamReader fs, out char c)
    {
      c = (char)0;
      if (fs.EndOfStream) { return false; }
      int peek = fs.Peek();
      if (peek == -1) { return false; }

      c = (char)peek;
      return true;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Save(string outputPath)
    {
      List<string> toWrite = new List<string>(Lines.Count + 1);

      const bool WRITE_HEADER = true;
      if (WRITE_HEADER)
      {
        string header = string.Join(Separator, (from x in Columns select x.Name));
        toWrite.Add(header);
      }
      else
      {
        throw new NotSupportedException();
      }


      foreach (var l in Lines)
      {
        var processedVals = from x in l.Values select "\"" + x.Replace("\"", "\"\"") + "\"";
        // var processedVals = from x in l.Values select x;

        string line = string.Join(Separator, processedVals);
        toWrite.Add(line);
      }


      File.WriteAllLines(outputPath, toWrite);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private string GetColumnValue(string[] parts, int index, bool trimWhitespace)
    {
      string res = parts[index];
      if (trimWhitespace) { res = res.Trim(); }
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void AddLine(params string[] values)
    {
      CSVLine l = new CSVLine(this.ColumnMap);
      int index = 0;
      foreach (var v in values)
      {
        l[index] = v;
        ++index;
      }

      AddLine(l);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void AddLine(CSVLine line)
    {
      Lines.Add(line);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Add a line to the file, using reflection to read source data from the given type.
    /// </summary>
    public void AddLineFromData<T>(T data)
    {
      Type t = typeof(T);
      List<CSVColToPropertyMap> propList = CSVLine.GetPropList(t, ColumnMap);

      string[] vals = new string[ColumnMap.Count];
      foreach (var p in propList)
      {
        int index = ColumnMap.GetIndex(p.ColName);
        if (index != -1)
        {
          object val = p.PropInfo.GetValue(data, null);

          string useVal = val?.ToString() ?? ""; // FixCSVData(val?.ToString() ?? "");

          if (p.PropInfo.PropertyType == typeof(string))
          {
            // Just quote all string fields...
            // Also make sure to escape any existing quotes.
            useVal = useVal.Replace("\"", "\"\"");
            useVal = "\"" + useVal + "\"";
          }
          vals[index] = useVal;

        }
      }

      Lines.Add(new CSVLine(ColumnMap, vals));
    }

    // --------------------------------------------------------------------------------------------------------------------------
    // HACK: This is to fix the format for a particular CSV reader that can't handle escaping correctly....
    private string FixCSVData(string v)
    {
      string res = v.Replace(",", " ").Replace(Environment.NewLine, " ");
      return res;
    }
  }



  public static class Csv
  {

    private const string QUOTE = "\"";
    private const string ESCAPED_QUOTE = "\"\"";
    private static char[] CHARACTERS_THAT_MUST_BE_QUOTED = { ',', '"', '\n' };

    public static string Escape(string s)
    {
      if (s.Contains(QUOTE))
      {
        s = s.Replace(QUOTE, ESCAPED_QUOTE);
      }

      if (s.IndexOfAny(CHARACTERS_THAT_MUST_BE_QUOTED) > -1)
      {
        s = QUOTE + s + QUOTE;
      }

      return s;
    }
  }

}
