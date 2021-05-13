using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

#if NETFX_CORE
using Windows.Storage;
#else

#endif

namespace drewCo.Tools
{
  // ============================================================================================================================
  /// <summary>
  /// Encapsulates a simple set of XML save/load operations.
  /// </summary>
  /// <typeparam name="T"></typeparam>
#if NETFX_CORE
  [DataContract]
#else
  [Serializable]
#endif
  public class XMLFile<T> where T : XMLFile<T>
  {

#if NETFX_CORE
    [IgnoreDataMember]
#else
    [XmlIgnore]
#endif

    public string FilePath { get; private set; }

    protected Sentinel Serializer = new Sentinel();
    protected bool IsLoaded = false;

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Saves the file data to the given path.  Existing files will be overwritten.
    /// </summary>
    /// <param name="filePath_">The path to write the file to.</param>
    /// <param name="createDir">Optionally create the directory that the file is to be written to, if it doesn't already
    /// exist.</param>
#if NETFX_CORE
    public async Task Save(string filePath_ = null, bool createDir = false)
#else
    public void Save(string filePath_ = null, bool createDir = false)
#endif
    {
#if NETFX_CORE
      if (createDir) { await FileTools.CreateDirectory(Path.GetDirectoryName(filePath_)); }
#else
      if (createDir) { FileTools.CreateDirectory(Path.GetDirectoryName(filePath_)); }
#endif
      if (filePath_ != null)
      {
        FilePath = filePath_;
      }
      else
      {
        if (FilePath == null)
        {
          throw new InvalidOperationException("Please provide a non-null file path!");
        }
      }

      PreSave();
#if NETFX_CORE
      await InternalSave();
#else
      InternalSave();
#endif
      PostSave();
    }


    // --------------------------------------------------------------------------------------------------------------------------
#if NETFX_CORE
    public static async Task<T> Load(string filePath_, bool createDefault = false)
#else
    public static T Load(string filePath_, bool createDefault = false)
#endif
    {
      if (createDefault)
      {
#if NETFX_CORE
        if (!(await FileTools.FileExists(filePath_)))
#else
        if (!File.Exists(filePath_))
#endif
        {
          string dir = Path.GetDirectoryName(filePath_);

#if NETFX_CORE
          await FileTools.CreateDirectory(dir);
#else
          FileTools.CreateDirectory(dir);
#endif


          // We create a new one, and then we can set the default properties.
          T instance = Activator.CreateInstance<T>();

          IEnumerable<AttributeAndProp<DefaultValueAttribute>> attrs = ReflectionTools.GetAttributesOnProperties<DefaultValueAttribute, T>();
          foreach (var item in attrs)
          {

            object val = null;
            if (item.Attribute.UseNewInstace)
            {
              val = Activator.CreateInstance(item.Property.PropertyType);
            }
            else
            {
              val = item.Attribute.Value;
            }

            item.Property.SetValue(instance, val, null);

          }

#if NETFX_CORE
          await instance.Save(filePath_, false);
#else
          instance.Save(filePath_, false);
#endif
          return instance;

        }
      }

#if NETFX_CORE
      var res = await InternalLoad(filePath_);
      res.PostLoad();
#else
      var res = InternalLoad(filePath_);
      res.PostLoad();
#endif

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
#if NETFX_CORE
    private async Task InternalSave()
#else
    private void InternalSave()
#endif
    {

#if NETFX_CORE
      DataContractSerializer serial = new DataContractSerializer(typeof(T));

      string dir = Path.GetDirectoryName(FilePath);
      StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(dir);

      StorageFile file = await folder.CreateFileAsync(Path.GetFileName(FilePath), CreationCollisionOption.ReplaceExisting);
      using (Stream stream = await file.OpenStreamForWriteAsync())
      {
        serial.WriteObject(stream, this);
      }

#else
      XmlSerializer serial = new XmlSerializer(typeof(T));
      using (FileStream stream = new FileStream(FilePath, FileMode.Create, FileAccess.Write))
      {
        serial.Serialize(stream, this);
      }
#endif


    }

    // --------------------------------------------------------------------------------------------------------------------------
#if NETFX_CORE
    private async static Task<T> InternalLoad(string filePath_)
#else
    private static T InternalLoad(string filePath_)
#endif
    {
#if NETFX_CORE

      DataContractSerializer serial = new DataContractSerializer(typeof(T));

      string dir = Path.GetDirectoryName(filePath_);
      StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(dir);

      StorageFile file = await folder.GetFileAsync(Path.GetFileName(filePath_));
      using (Stream stream = await file.OpenStreamForReadAsync())
      {
        T res = (T)serial.ReadObject(stream);
        (res as XMLFile<T>).FilePath = filePath_;

        res.IsLoaded= true;
        return res;
      }
#else
      XmlSerializer serial = new XmlSerializer(typeof(T));
      using (FileStream stream = new FileStream(filePath_, FileMode.Open, FileAccess.Read))
      {
        object res = serial.Deserialize(stream);
        (res as XMLFile<T>).FilePath = filePath_;
        (res as XMLFile<T>).IsLoaded = true;
        return (T)res;
      }
#endif
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public string ToString(Encoding encoding)
    {
      XmlSerializer serial = new XmlSerializer(typeof(T));
      using (var writer = new CustomStringWriter(encoding))
      {
        serial.Serialize(writer, this);
        return writer.ToString();
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public override string ToString()
    {
      return ToString(Encoding.ASCII);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static T FromString(string xmlData)
    {
      XmlSerializer serial = new XmlSerializer(typeof(T));
      using (var stream = new CustomStringReader(xmlData, Encoding.ASCII))
      {
        object res = serial.Deserialize(stream);
        (res as XMLFile<T>).FilePath = null;
        (res as XMLFile<T>).IsLoaded = true;
        return (T)res;
      }

    }

    // --------------------------------------------------------------------------------------------------------------------------
    protected virtual void PreSave() { }

    // --------------------------------------------------------------------------------------------------------------------------
    protected virtual void PostSave() { }

    // --------------------------------------------------------------------------------------------------------------------------
    protected virtual void PostLoad() { }
  }


}

// ============================================================================================================================
class CustomStringReader : StringReader
{
  internal CustomStringReader(string s, Encoding encoding_)
   : base(s)
  {
    this.UseEncoding = encoding_;
  }

  private Encoding UseEncoding;
}

// ============================================================================================================================
class CustomStringWriter : StringWriter
{
  private Encoding UseEncoding;

  // --------------------------------------------------------------------------------------------------------------------------
  internal CustomStringWriter(Encoding encoding_)
    : base()
  {
    UseEncoding = encoding_;
  }

  public override Encoding Encoding => UseEncoding;
}
