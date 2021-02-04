using System;
using System.Collections.Generic;
using System.Linq;

namespace drewCo.Tools.Testers
{

  // ============================================================================================================================
  /// <summary>
  /// Used to map a property name to a column in a <see cref="CSVFile"/> instance.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property)]
  public class CSVCol : Attribute
  {
    public CSVCol(string colname_) { ColumnName = colname_; }
    public string ColumnName { get; private set; }
  }

  // ============================================================================================================================
  public class NewWineEntry
  {

    public string Vintage { get; set; }
    public string Winery { get; set; }
    public string Label { get; set; }
    public string Variety { get; set; }
    public string Region { get; set; }
    public string Country { get; set; }

    //[CSVCol("Drink By")]
    //public string DrinkBy { get; set; }
    public string Best { get; set; }

    public string Location { get; set; }


    [CSVCol("Wine Type")]
    public string WineType { get; set; }
    public string Qty { get; set; }
    public int QtyPerCase { get; set; } = 12;
    public string BottleSize { get; set; }
    public string Price { get; set; }
    public string UserNotes1 { get; set; }
    public string UserNotes2 { get; set; }

    public string RackNames { get; set; }
    public string RackCols { get; set; }
    public string RackRows { get; set; }

    public string UserField3 { get; set; }

    private static List<string> Reds = new List<string>()
    {
        "sauvignon",
        "grenache",
        "nebbiolo",
        "tempranillo",
        "montepulciano",
        "gsm",
        "sangiovese",
        "syrah",
        "red",
        "mourvedre"
        , "merlot"
        , "malbec"
        , "noir"
        , "shiraz"
        , "cabernet franc"
        , "amarone"
        , "rioja"
        , "gamay"
        , "crianza"
        , "sirah"
        , "verdot"
        , "mencia"
    };

    private static List<string> Whites = new List<string>()
    {
      "chardonnay",
    };

    // --------------------------------------------------------------------------------------------------------------------------
    public NewWineEntry(LegacyWineEntry oldData)
    {
      // Basic copy....
      DTOMapper.CopyMembers(oldData, this);


      // Now the manual stuff.....
      Label = oldData.Name;
      Variety = oldData.Varietal;

      WineType = TranslateType(oldData.Type, oldData.Varietal);
      Qty = oldData.Bottles;

      BottleSize = oldData.Size;

      Best = oldData.PeakYear;

      UserNotes1 = oldData.TastingNotes;
      UserNotes2 = oldData.Notes + Environment.NewLine + oldData.ExtendedNotes;

      //RackNames = string.Join("
      Location = oldData.Location; // + ", " + oldData.Position;
      UserField3 = oldData.Position;

      // This is how we will handle the racks....
      if (oldData.Bottles == "1")
      {
        RackNames = "Rack 1";
        RackCols = "1";
        RackRows = "1";
      }

    }

    // --------------------------------------------------------------------------------------------------------------------------
    private string TranslateType(string type, string varietal)
    {
      varietal = (varietal ?? "").ToLower();
      string useType = (type ?? "").ToLower();

      if (useType.Contains("red")) { return "R"; }

      switch (useType)
      {
        case "red":
          return "R";
        case "white":
          return "W";

        case "premium":
        default:

          if (string.IsNullOrWhiteSpace(varietal)) { return "R"; }

          foreach (var r in Reds)
          {
            if (varietal.Contains(r)) { return "R"; }
          }
          foreach (var w in Whites)
          {
            if (varietal.Contains(w)) { return "W"; }
          }

          //if (Reds.Any(x=>varietal.IndexOf(x) != -1)) { return "R"; }
          //if (Whites.Any(x=>varietal.IndexOf(x) != -1)) { return "W"; }

          // eh.... most of them are red anyway....
          return "R";

          // throw new NotImplementedException($"I don't know how to translate type '{type}'! varietal = {varietal}");

      }
    }

  }


  // ============================================================================================================================
  public class LegacyWineEntry
  {
    // --------------------------------------------------------------------------------------------------------------------------
    public LegacyWineEntry() { }

    //// --------------------------------------------------------------------------------------------------------------------------
    //public WineEntry(CSVLine line)
    //{
    //  // NOTE:  I think we can do a dto mapper thingy... ??
    //  Winery = line["Winery"];
    //  Type = line["Type"];
    //}

    public string Winery { get; set; }
    public string Name { get; set; }
    public string Vintage { get; set; }
    public string Varietal { get; set; }
    public string Type { get; set; }
    public string Size { get; set; }
    public string Country { get; set; }
    public string Region { get; set; }
    public string Price { get; set; }

    // These are both for tagging in the actual racks.
    public string Location { get; set; }
    public string Position { get; set; }

    // Quantity
    public string Bottles { get; set; }

    public string PeakYear { get; set; }
    public string TastingNotes { get; set; }
    public string Notes { get; set; }
    public string Tasted { get; set; }

    /// <summary>
    /// These are notes that I have extracted from the input CSV.  They were represented as extra lines
    /// in the CSV, mostly using the 'Winery' field to store the data.
    /// </summary>
    public string ExtendedNotes { get; set; }
    //      public int Qty { get; set; }
  }


}
