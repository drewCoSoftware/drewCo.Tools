//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright ©2009-2018 Andrew A. Ritz, All Rights Reserved
//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace drewCo.Tools
{

  // ============================================================================================================================
  public abstract class EZWriter
  {
    private static Dictionary<Type, object> Writers = new Dictionary<Type, object>();

    // --------------------------------------------------------------------------------------------------------------------------
    static EZWriter()
    {
      Writers.Add(typeof(UInt16), new TypeWriter<UInt16>(Write));
      Writers.Add(typeof(UInt32), new TypeWriter<UInt32>(Write));
      Writers.Add(typeof(UInt64), new TypeWriter<UInt64>(Write));
      Writers.Add(typeof(decimal), new TypeWriter<decimal>(Write));
      Writers.Add(typeof(string), new TypeWriter<string>(Write));
      Writers.Add(typeof(Int32), new TypeWriter<Int32>(Write));
      Writers.Add(typeof(Int64), new TypeWriter<Int64>(Write));
      Writers.Add(typeof(byte), new TypeWriter<byte>(Write));
      Writers.Add(typeof(bool), new TypeWriter<bool>(Write));
      Writers.Add(typeof(Int32[]), new TypeWriter<Int32[]>(Write));

      Writers.Add(typeof(char), new TypeWriter<char>(Write));
      Writers.Add(typeof(float), new TypeWriter<float>(Write));
      Writers.Add(typeof(double), new TypeWriter<double>(Write));
      Writers.Add(typeof(char[]), new TypeWriter<char[]>(Write));
      Writers.Add(typeof(byte[]), new TypeWriter<byte[]>(Write));
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Lazy lists are used in certain serialization scenarios where read speed is important, or all data doesn't need to be 
    /// loaded initially.
    /// </summary>
    /// <remarks>
    /// At time of writing, there is no C# support for lazy lists, or their readback.  We provided these tools solely for C#
    /// writing and C++ reading.
    /// </remarks>
    public static void WriteLazyList<T>(Stream s, List<T> list, Action<Stream, T> writeFunc)
    {
      // The first 16 bytes are for address + size information.
      // We will come back in an re-write the size data once we know it.
      long startPos = s.Position;
      Write(s, (UInt64)startPos);
      Write(s, (UInt64)0);

      // Now the count of items, and the data for each.  We will need to compile address information for each of the list items,
      // So we will skip ahead (again), and backfill this information when we are ready.
      uint count = (uint)list.Count;
      Write(s, count);

      long addrListPos = s.Position;
      List<UInt64> itemAddresses = new List<UInt64>((int)count);
      s.Seek(sizeof(UInt64) * list.Count, SeekOrigin.Current);


      // Write each item, making note of its starting address...
      for (int i = 0; i < count; i++)
      {
        itemAddresses.Add((UInt64)s.Position);
        writeFunc.Invoke(s, list[i]);
      }

      long endPos = s.Position;
      long size = endPos - startPos;

      // Write in the size + address data now + set the write cursor at the correct position!
      s.Seek(startPos + sizeof(UInt64), SeekOrigin.Begin);
      Write(s, (UInt64)size);

      s.Seek(addrListPos, SeekOrigin.Begin);
      for (int i = 0; i < count; i++)
      {
        Write(s, itemAddresses[i]);
      }

      // Set the write cursor at the appropriate position.
      s.Seek(endPos, SeekOrigin.Begin);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    [Obsolete("Use 'Write' version with no length instead!")]
    public static void WriteChars(Stream s, char[] src, int count)
    {
      for (int i = 0; i < count; i++)
      {
        Write(s, src[i]);
      }

    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void WriteBytes(Stream s, byte[] val)
    {
      int len = val.Length;
      s.Write(val, 0, len);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Write the byte with 'val' to the stream 'count' times.
    /// </summary>
    public static void WriteBytes(Stream s, byte val, int count)
    {
      for (int i = 0; i < count; i++)
      {
        Write(s, val);
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    // NOTE: This is provided for easier reflection based method resolution.
    public static void WriteList<T>(Stream s, List<T> list)
    {
      Write(s, list);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Write the list of data, using the given write function for the elements.
    /// </summary>
    public static void WriteList<T>(Stream s, List<T> list, Action<Stream, T> writeFunc)
    {
      List<T> res = new List<T>();
      uint count = (uint)list.Count;
      Write(s, count);
      for (int i = 0; i < count; i++)
      {
        writeFunc(s, list[i]);
      }
    }

    //// --------------------------------------------------------------------------------------------------------------------------
    ///// <remarks>NOT OPTIMIZED!</remarks>
    //public static void Write<T>(Stream s, T data)
    //{
    //  var m = typeof(EZWriter).GetMethod("Write", new Type[] { typeof(Stream), typeof(T) } );
    //  if (m == null)
    //  {
    //    throw new InvalidOperationException(string.Format("Could not find write method for type {0}!", typeof(T)));
    //  }

    //  m.Invoke(null, new object[] { s, data });

    //}

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Write the list of the given type to the stream.
    /// </summary>
    public static void Write<T>(Stream s, List<T> list)
    {
      var writer = (TypeWriter<T>)Writers[typeof(T)];

      uint count = (uint)list.Count;
      Write(s, count);
      for (int i = 0; i < count; i++)
      {
        writer.Write(s, list[i]);
      }
    }



    // --------------------------------------------------------------------------------------------------------------------------
    public static void Write(Stream s, byte data)
    {
#if TRACE_SERIAL
      Debug.WriteLine("W.BYTE \t{0}", s.Position);
#endif
      s.WriteByte(data);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void Write(Stream s, bool data)
    {
#if TRACE_SERIAL
      Debug.WriteLine("W.BOOL \t{0}", s.Position);
#endif
      s.WriteByte(data ? (byte)1 : (byte)0);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void Write(Stream s, Int32[] data)
    {
#if TRACE_SERIAL
      Debug.WriteLine("W.INT32[] \t{0}", s.Position);
#endif

      int count = data.Length;
      Write(s, count);

      // NOTE: We could possibly do a 'fixed' / 'pointer' type thing and cast.  Could be faster.....
      for (int i = 0; i < count; i++)
      {
        Write(s, data[i]);
      }

    }


    // --------------------------------------------------------------------------------------------------------------------------
    public static void Write(Stream s, string input)
    {
#if TRACE_SERIAL
      Debug.WriteLine("W.STR \t{0}", s.Position);
#endif

      // First byte indicates null status.
      byte hasRef = (byte)(input == null ? 0 : 1);
      s.WriteByte(hasRef);
      if (hasRef == 0) { return; }

      UInt32 len = (UInt32)input.Length;
      Write(s, len);

      for (int i = 0; i < len; i++)
      {
        s.WriteByte((byte)input[i]);
      }

      // Always null terminate!
      s.WriteByte(0);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void Write(Stream s, UInt16 input)
    {
#if TRACE_SERIAL
      Debug.WriteLine("W.UINT16 \t{0}", s.Position);
#endif

      for (int i = 0; i < sizeof(UInt16); i++)
      {
        var val = (byte)((input >> (i * 8)) & 0xFF);
        s.WriteByte(val);
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void Write(Stream s, UInt32 input)
    {
#if TRACE_SERIAL
      Debug.WriteLine("W.UINT32 \t{0}", s.Position);
#endif

      for (int i = 0; i < sizeof(uint); i++)
      {
        var val = (byte)((input >> (i * 8)) & 0xFF);
        s.WriteByte(val);
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void Write(Stream s, UInt64 input)
    {
#if TRACE_SERIAL
      Debug.WriteLine("W.UINT64 \t{0}", s.Position);
#endif

      for (int i = 0; i < sizeof(UInt64); i++)
      {
        var val = (byte)((input >> (i * 8)) & 0xFF);
        s.WriteByte(val);
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void Write(Stream s, decimal input)
    {
#if TRACE_SERIAL
      Debug.WriteLine("W.DECIMAL \t{0}", s.Position);
#endif

      int[] dBits = decimal.GetBits(input);
      Write(s, dBits);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void Write(Stream s, Int32 input)
    {
#if TRACE_SERIAL
      Debug.WriteLine("W.INT32 \t{0}", s.Position);
#endif

      for (int i = 0; i < sizeof(Int32); i++)
      {
        var val = (byte)((input >> (i * 8)) & 0xFF);
        s.WriteByte(val);
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void Write(Stream s, Int64 input)
    {
#if TRACE_SERIAL
      Debug.WriteLine("W.INT32 \t{0}", s.Position);
#endif

      for (int i = 0; i < sizeof(Int64); i++)
      {
        var val = (byte)((input >> (i * 8)) & 0xFF);
        s.WriteByte(val);
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void Write(Stream s, byte[] val)
    {
#if TRACE_SERIAL
      Debug.WriteLine("W.BYTE[] \t{0}", s.Position);
#endif
      Write(s, val.Length);
      s.Write(val, 0, val.Length);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void Write(Stream s, char input)
    {
#if TRACE_SERIAL
      Debug.WriteLine("W.CHAR \t{0}", s.Position);
#endif

      Write(s, (byte)input);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void Write(Stream s, float input)
    {
#if TRACE_SERIAL
      Debug.WriteLine("W.FLOAT \t{0}", s.Position);
#endif

      uint casted = CastUint32(input);
      Write(s, casted);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void Write(Stream s, char[] chars)
    {
#if TRACE_SERIAL
      Debug.WriteLine("W.CHAR[] \t{0}", s.Position);
#endif
      int len = chars.Length;
      for (int i = 0; i < len; i++)
      {
        s.WriteByte((byte)chars[i]);
      }

    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void Write(Stream s, double input)
    {
#if TRACE_SERIAL
      Debug.WriteLine("W.DBL \t{0}", s.Position);
#endif

      UInt64 casted = CastUint64(input);
      Write(s, casted);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Writes a raw string to the given stream, no length information is encoded.
    /// </summary>
    public static void RawString(Stream s, string data, int crlfCount = 0)
    {
#if TRACE_SERIAL
      Debug.WriteLine("W.RAWSTRING \t{0}", s.Position);
#endif

      int len = data.Length;
      byte[] writeBytes = new byte[len + (crlfCount * 2)];

      Encoding.UTF8.GetBytes(data, 0, len, writeBytes, 0);
      //      ASCIIEncoding.ASCII.GetBytes(data, 0, len, writeBytes, 0);


      for (int i = 0; i < crlfCount; i++)
      {
        writeBytes[len + (i * 2)] = 13;
        writeBytes[len + (i * 2) + 1] = 10;
      }
      s.Write(writeBytes, 0, writeBytes.Length);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Bitwise typecast for saving to disk!
    /// </summary>
    private static unsafe UInt32 CastUint32(float value)
    {
      UInt32 val = *((UInt32*)&value);
      return val;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Bitwise typecast for saving to disk!
    /// </summary>
    private static unsafe UInt64 CastUint64(double value)
    {
      UInt64 val = *((UInt64*)&value);
      return val;
    }


  }


  // ============================================================================================================================
  public class TypeWriter<T>
  {
    // --------------------------------------------------------------------------------------------------------------------------
    public TypeWriter(Action<Stream, T> Writer_)
    {
      Write = Writer_;
    }

    public readonly Action<Stream, T> Write;
  }

}
