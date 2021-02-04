//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright ©2019-2020 Andrew A. Ritz, All Rights Reserved
//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace drewCo.Tools.CSV
{
  // ============================================================================================================================
  public class CSVLine
  {
    private CSVColumnMap ColumnMap = null;

    // --------------------------------------------------------------------------------------------------------------------------
    public CSVLine(CSVColumnMap colMap_)
    {
      ColumnMap = colMap_;

      // Yes, we should be using a dictionary.
      Values = new List<string>(ColumnMap.Count);
      for (int i = 0; i < ColumnMap.Count; i++)
      {
        Values.Add(null);
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    // TODO: Check the counts, etc.
    public CSVLine(CSVColumnMap colMap_, IList<string> values)
    {
      ColumnMap = colMap_;
      Values = values.ToList();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public CSVLine(CSVColumnMap colMap_, string values, string separator)
    {
      // NOTE: This also doesn't limit the total number of entries!  It probably should!
      ColumnMap = colMap_;
      Values = ParseLine(values, separator);
      while (Values.Count < ColumnMap.Count)
      {
        Values.Add(string.Empty);
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Parse out the contents of the line, being sensitive to quoted fields.
    /// </summary>
    private List<string> ParseLine(string values, string separator)
    {
      var res = new List<string>();
      string combined = null;
      bool inQuotes = false;
      List<string> src = values.Split(new[] { separator }, StringSplitOptions.None).ToList();
      foreach (var p in src)
      {
        string part = p; //p.Trim();

        bool startsWithQuote = StartWithQuote(part);
        bool endsWithQuote = EndsWithQuote(part);

        if (inQuotes)
        {
          combined += (separator + part);
          if (endsWithQuote)
          {
            inQuotes = false;
            combined = combined.Substring(1, combined.Length - 2);
            res.Add(combined);
            combined = null;
          }
        }
        else if (startsWithQuote && !endsWithQuote)
        {
          inQuotes = true;
          combined = part;
        }
        else
        {
          // Dequote.
          if (startsWithQuote)
          {
            part = part.Substring(1, part.Length - 2);
          }

          if (part == "")
          {
            // This is a dirty hack to make it so that quoted empty strings are preserved as empty strings, whereas
            // plain old empty values are interpreted as null.
            res.Add(p == "\"\"" ? string.Empty : null);
          }
          else
          {
            res.Add(part);
          }
        }
      }

      // Include anything that doesn't have its closing quotes.
      if (combined != null)
      {
        res.Add(combined);
      }

      for (int i = 0; i < res.Count; i++)
      {
        if (res[i] != null)
        {
          // Fix up the escaped quotes.
          res[i] = res[i].Replace("\"\"", "\"").Trim();
        }
      }


      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Checks to see if we start with a single, unescaped quote.
    /// </summary>
    private bool EndsWithQuote(string input)
    {
      input = input.TrimEnd();
      if (input.Length == 0) { return false; }

      int lastIndex = input.Length - 1;
      bool res = input[lastIndex] == '\"';
      if (input.Length > 1)
      {
        int checkIndex = lastIndex - 1;
        while (checkIndex > 0)
        {
          // We need to check for pairs of quotes, not just single quotes.
          if (input[checkIndex] == '\"')
          {
            if (input[checkIndex - 1] == '\"')
            {
              checkIndex -= 2;
            }
            else
            {
              res = false;
              break;
            }
          }
          else
          {
            // Last Pair check was OK
            break;
          }
        }
      }
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Checks to see if we start with a single, unescaped quote.
    /// </summary>
    private bool StartWithQuote(string input)
    {
      input = input.TrimStart();
      if (input.Length == 0) { return false; }

      bool res = input[0] == '\"';
      if (input.Length > 1)
      {
        bool nextQuote = input[1] == '\"';
        if (nextQuote)
        {
          return input.Length == 2;
        }
        //else
        //{

        //}
        //res &= input[1] != '\"';
      }
      return res;
    }

    public List<string> Values { get; set; }

    // --------------------------------------------------------------------------------------------------------------------------
    public string this[string name]
    {
      get
      {
        int index = ColumnMap.GetIndex(name);
        if (index == -1) { return null; }
        return Values[index];
      }
      set
      {
        int index = ColumnMap.GetIndex(name);
        if (index == -1) { throw new KeyNotFoundException($"There is no mapped column with the name {name}!"); }
        Values[index] = value;
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public string this[int index]
    {
      get
      {
        //int index = ColumnMap.GetIndex(name);
        //   if (index == -1) { return null; }
        if (index < 0 || index > Values.Count - 1) { return null; }
        return Values[index];
      }
      set
      {
        if (index < 0 || index > Values.Count - 1) { throw new IndexOutOfRangeException(); }
        Values[index] = value;
      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    private static Dictionary<Type, List<CSVColToPropertyMap>> TypesToPropList = new Dictionary<Type, List<CSVColToPropertyMap>>();


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Create an instance of a type from this line.
    /// Members with matching names will be populated from the CSV data.
    /// </summary>
    public T CreateData<T>()
    {
      Type t = typeof(T);

      List<CSVColToPropertyMap> propList = GetPropList(t, ColumnMap);

      T res = Activator.CreateInstance<T>();
      foreach (var p in propList)
      {
        string useValue = this[p.ColName];
        p.PropInfo.SetValue(res, useValue, null);
      }

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    internal static List<CSVColToPropertyMap> GetPropList(Type t, CSVColumnMap colMap)
    {
      if (!TypesToPropList.TryGetValue(t, out List<CSVColToPropertyMap> propList))
      {
        propList = CreatePropertyList(t, colMap);
        TypesToPropList.Add(t, propList);
      }

      return propList;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private static List<CSVColToPropertyMap> CreatePropertyList(Type t, CSVColumnMap colMap)
    {
      List<CSVColToPropertyMap> propList;
      var props = t.GetProperties();
      propList = new List<CSVColToPropertyMap>();

      foreach (var m in props)
      {
        var match = (from x in colMap.Names
                     where (x.Replace(" ", "") == m.Name)
                     select x).SingleOrDefault();

        if (match != null)
        {
          propList.Add(new CSVColToPropertyMap()
          {
            ColName = match,
            PropInfo = m
          });
        }
      }

      return propList;
    }
  }

  // ============================================================================================================================
  internal class CSVColToPropertyMap
  {
    public string ColName { get; set; }
    public PropertyInfo PropInfo { get; set; }
  }


}
