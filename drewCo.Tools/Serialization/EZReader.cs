//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright ©2009-2019 Andrew A. Ritz, All Rights Reserved
//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace drewCo.Tools
{
  // ============================================================================================================================
  public class TypeReader<T>
  {
    // --------------------------------------------------------------------------------------------------------------------------
    public TypeReader(Func<Stream, T> reader_)
    {
      Read = reader_;
    }

    public readonly Func<Stream, T> Read;
  }

  // ============================================================================================================================
  public class EZReader
  {
    const int BYTE = 8;

    private static Dictionary<Type, object> Readers = new Dictionary<Type, object>();

    // --------------------------------------------------------------------------------------------------------------------------
    static EZReader()
    {
    //  Readers.Add(typeof(UInt16), new TypeReader<UInt16>(ReadUInt16));
      Readers.Add(typeof(UInt32), new TypeReader<UInt32>(ReadUInt32));
      Readers.Add(typeof(UInt64), new TypeReader<UInt64>(ReadUInt64));
      Readers.Add(typeof(decimal), new TypeReader<decimal>(ReadDecimal));

      Readers.Add(typeof(string), new TypeReader<string>(ReadString));
      Readers.Add(typeof(Int16), new TypeReader<Int16>(ReadInt16));
      Readers.Add(typeof(Int32), new TypeReader<Int32>(ReadInt32));
      Readers.Add(typeof(Int64), new TypeReader<Int64>(ReadInt64));
      Readers.Add(typeof(byte), new TypeReader<byte>(ReadByte));
      Readers.Add(typeof(byte[]), new TypeReader<byte[]>(ReadByteArray));

      Readers.Add(typeof(char), new TypeReader<char>(ReadChar));
      Readers.Add(typeof(float), new TypeReader<float>(ReadFloat));
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public static char[] ReadChars(Stream s, int count)
    {
      char[] res = new char[count];
      for (int i = 0; i < count; i++)
      {
        res[i] = ReadChar(s);
      }
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static byte[] ReadBytes(Stream s, int count)
    {
      byte[] res = new byte[count];
      s.Read(res, 0, count);
      return res;
    }

    //// --------------------------------------------------------------------------------------------------------------------------
    //public static char[] ReadChars(Stream s, int count)
    //{
    //  byte[] buffer = new byte[count];
    //  s.Read(buffer, 0, count);

    //  char[] res = new char[count];
    //  for (int i = 0; i < count; i++)
    //  {
    //    res[i] = (char)buffer[i];
    //  }

    //  return res;
    //}

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Generic read function.
    /// </summary>
    /// <remarks>NOT OPTIMIZED!</remarks>
    public static T Read<T>(Stream s)
    {
      var allMethods = ReflectionTools.GetMethods(typeof(EZReader)); //.GetMethods();
      var m = (from x in allMethods
               where x.Name.StartsWith("Read") &&
                     x.ReturnType == typeof(T)
               select x).Single();

      object res = m.Invoke(null, new[] { s });
      return (T)res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static string ReadString(Stream s)
    {

#if TRACE_SERIAL
      Debug.WriteLine("R.STR \t{0}", s.Position);
#endif

      StringBuilder sb = new StringBuilder();

      byte hasRef = (byte)s.ReadByte();
      if (hasRef == 0) { return null; }

      UInt32 len = ReadUInt32(s);

      // Length check!
      if (s.Position + len > s.Length)
      {
        string errMsg = "Reading a string of size {0} exceeds file length {1}";
        throw new InvalidOperationException(string.Format(errMsg, s.Position + len, s.Length));
      }

      for (uint i = 0; i < len; i++)
      {
        char next = (char)s.ReadByte();
        sb.Append(next);
      }

      byte check = (byte)s.ReadByte();
      if (check != 0)
      {
        throw new InvalidOperationException("Invalid string termination!");
      }

      return sb.ToString();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static UInt16 ReadUInt16(Stream s)
    {
#if TRACE_SERIAL
      Debug.WriteLine("R.UINT16 \t{0}", s.Position);
#endif
      byte[] raw = new byte[sizeof(UInt16)];
      s.Read(raw, 0, sizeof(UInt16));

      UInt16 res = BitConverter.ToUInt16(raw, 0);
      return res;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public static UInt32 ReadUInt32(Stream s)
    {
#if TRACE_SERIAL
      Debug.WriteLine("R.UINT32 \t{0}", s.Position);
#endif

      UInt32 res = 0x0;

      int size = sizeof(UInt32);
      for (int i = 0; i < size; i++)
      {
        var b = s.ReadByte();
        res = res | ((UInt32)(b << (BYTE * i)));
      }

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static UInt64 ReadUInt64(Stream s)
    {
#if TRACE_SERIAL
      Debug.WriteLine("R.UINT64 \t{0}", s.Position);
#endif

      UInt64 res = 0x0;

      int size = sizeof(UInt64);
      for (int i = 0; i < size; i++)
      {
        var b = (UInt64)s.ReadByte();
        res = res | ((UInt64)(b << (BYTE * i)));
      }

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static decimal ReadDecimal(Stream s)
    {
#if TRACE_SERIAL
      Debug.WriteLine("R.DECIMAL \t{0}", s.Position);
#endif

      int[] bits = ReadInt32Array(s);
      decimal res = new decimal(bits);
      return res;

      //UInt64 res = 0x0;

      //int size = sizeof(UInt64);
      //for (int i = 0; i < size; i++)
      //{
      //  var b = (UInt64)s.ReadByte();
      //  res = res | ((UInt64)(b << (BYTE * i)));
      //}

      //return res;
    }



    // --------------------------------------------------------------------------------------------------------------------------
    public static byte ReadByte(Stream s)
    {
#if TRACE_SERIAL
      Debug.WriteLine("R.BYTE \t{0}", s.Position);
#endif

      return (byte)s.ReadByte();
    }

        // --------------------------------------------------------------------------------------------------------------------------
    public static byte[] ReadByteArray(Stream s)
    {
#if TRACE_SERIAL
      Debug.WriteLine("R.BYTE \t{0}", s.Position);
#endif
      int count =ReadInt32(s);
      byte[] res = new byte[count];
      s.Read(res, 0, count);

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static bool ReadBool(Stream s)
    {
#if TRACE_SERIAL
      Debug.WriteLine("R.BOOL \t{0}", s.Position);
#endif

      return s.ReadByte() == 1;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static Int32[] ReadInt32Array(Stream s)
    {
#if TRACE_SERIAL
      Debug.WriteLine("R.INT32[] \t{0}", s.Position);
#endif

      int count = ReadInt32(s);
      Int32[] data = new Int32[count];

      // NOTE: We could possibly do a 'fixed' / 'pointer' type thing and cast.  Could be faster.....
      for (int i = 0; i < count; i++)
      {
        data[i] = ReadInt32(s);
      }
      return data;
    }

        // --------------------------------------------------------------------------------------------------------------------------
    public static Int16 ReadInt16(Stream s)
    {
#if TRACE_SERIAL
      Debug.WriteLine("R.INT16 \t{0}", s.Position);
#endif

      Int16 res = 0x0;

      int size = sizeof(Int16);
      if (s.Position + size > s.Length)
      {
        throw new InvalidOperationException("Read Past EOF!");
      }

      for (int i = 0; i < size; i++)
      {
        var b = (Int16)s.ReadByte();
        res = (short)(res | ((Int16)(b << (BYTE * i))));
      }

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static int ReadInt32(Stream s)
    {
#if TRACE_SERIAL
      Debug.WriteLine("R.INT32 \t{0}", s.Position);
#endif

      Int32 res = 0x0;

      int size = sizeof(UInt32);
      if (s.Position + size > s.Length)
      {
        throw new InvalidOperationException("Read Past EOF!");
      }

      for (int i = 0; i < size; i++)
      {
        var b = (UInt32)s.ReadByte();
        res = res | ((Int32)(b << (BYTE * i)));
      }

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static Int64 ReadInt64(Stream s)
    {
#if TRACE_SERIAL
      Debug.WriteLine("R.INT64 \t{0}", s.Position);
#endif

      int size = sizeof(Int64);
      if (s.Position + size > s.Length)
      {
        throw new InvalidOperationException("Read Past EOF!");
      }

      byte[] buffer = new byte[sizeof(Int64)];
      s.Read(buffer, 0, sizeof(Int64));

      Int64 res = BitConverter.ToInt64(buffer, 0);

      return res;
    }
    // --------------------------------------------------------------------------------------------------------------------------
    public static char ReadChar(Stream s)
    {
#if TRACE_SERIAL
      Debug.WriteLine("R.CHAR \t{0}", s.Position);
#endif

      //int res = 0x0;
      return (char)s.ReadByte();
      //int size = sizeof(char);
      //for (int i = 0; i < size; i++)
      //{
      //  var b = s.ReadByte();
      //  res = res | ((Int32)(b << (BYTE * i)));
      //}
      //return (char)res;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public static float ReadFloat(Stream s)
    {
#if TRACE_SERIAL
      Debug.WriteLine("R.FLOAT \t{0}", s.Position);
#endif

      UInt32 data = ReadUInt32(s);
      return CastFloat(data);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static double ReadDouble(Stream s)
    {
#if TRACE_SERIAL
      Debug.WriteLine("R.DOUBLE \t{0}", s.Position);
#endif

      UInt64 data = ReadUInt64(s);
      return CastDouble(data);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public static List<T> ReadList<T>(Stream s)
    {
      var reader = (TypeReader<T>)Readers[typeof(T)];

      List<T> res = new List<T>();
      uint count = ReadUInt32(s);
      for (int i = 0; i < count; i++)
      {
        res.Add(reader.Read(s));
      }

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Read the list of data, using the given read function for the elements.
    /// </summary>
    public static List<T> ReadList<T>(Stream s, Func<Stream, T> readFunc)
    {
      List<T> res = new List<T>();
      uint count = ReadUInt32(s);
      for (int i = 0; i < count; i++)
      {
        T next = readFunc(s);
        res.Add(next);
      }
      return res;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Bitwise typecast for saving to disk!
    /// </summary>
    private static unsafe float CastFloat(UInt32 value)
    {
      float val = *((float*)&value);
      return val;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Bitwise typecast for saving to disk!
    /// </summary>
    private static unsafe double CastDouble(UInt64 value)
    {
      double val = *((double*)&value);
      return val;
    }
  }

}
