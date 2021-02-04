// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright (c)2017-2019 Andrew A. Ritz, all rights reserved.
// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using drewCo.Curations;
using drewCo.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drewCo.Curations
{

// ============================================================================================================================
public static class EZListReader
{
  // --------------------------------------------------------------------------------------------------------------------------
  public static EZList<T> ReadEZList<T>(Stream s, Func<Stream, T> readFunc)
  {
    EZList<T> res = new EZList<T>();
    uint count = EZReader.ReadUInt32(s);
    for (int i = 0; i < count; i++)
    {
      T next = readFunc(s);
      res.Add(next);
    }
    return res;
  }
}

// ============================================================================================================================
public static class EZListWriter
{
  // --------------------------------------------------------------------------------------------------------------------------
  public static void WriteEZList<T>(Stream s, EZList<T> data, Action<Stream, T> writeFunc)
  {
    EZList<T> res = new EZList<T>();
    uint count = (uint)data.Count;
    EZWriter.Write(s, count);
    for (int i = 0; i < count; i++)
    {
      T next = data[i];
      writeFunc(s, next);
      res.Add(next);
    }

  }
}

}
