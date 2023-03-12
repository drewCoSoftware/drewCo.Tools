

using drewCo.Tools;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;


// ============================================================================================================================
/// <summary>
/// This is like XMLFile, but uses JSON.
/// </summary>
public class JsonFile<T>
  where T : JsonFile<T>, new()
{
  [JsonIgnore]
  public string? FilePath { get; protected set; }

  protected virtual void PreSave() { }
  protected virtual void PostLoad() { }

  // --------------------------------------------------------------------------------------------------------------------------
  public void Save(string? filePath = null)
  {
    FilePath = filePath != null ? filePath : null;
    if (FilePath == null)
    {
      throw new InvalidOperationException("You must set a valid file path!");
    }

    PreSave();
    FileTools.SaveJson(FilePath, this);
  }

  // --------------------------------------------------------------------------------------------------------------------------
  public static T Load(string filePath_)
  {
    string data = File.ReadAllText(filePath_);

    T res = Parse(data);

    res.FilePath = filePath_;
    res.PostLoad();
    return res;
  }

  // --------------------------------------------------------------------------------------------------------------------------
  public static T Parse(string jsonData)
  {
    var serializerOps = new JsonSerializerOptions()
    {
      AllowTrailingCommas = true,
    };
    T res = JsonSerializer.Deserialize<T>(jsonData, serializerOps);
    if (res == null)
    {
      throw new InvalidOperationException($"Could not deserialize type {typeof(T)} from json data!");
    }

    return res;
  }
}
