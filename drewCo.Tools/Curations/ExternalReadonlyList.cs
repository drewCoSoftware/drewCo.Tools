// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright (c)2010-2015 Andrew A. Ritz, all rights reserved.
// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using drewCo.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drewCo.Curations
{
  // ============================================================================================================================
  /// <summary>
  /// This is a special list that can be used internally in a class for simple things like add/remove/etc.
  /// It has the special feature of internally tracking a readonly collection that can then be used and exposed outside
  /// of the containing class.
  /// The purpose it to reduce a lot of boilerplate that is normally associated with this type of feature.
  /// </summary>
  public class ExternalReadonlyList<T> : IList<T>
  {

    private ReadOnlyCollection<T> _External = null;
    public ReadOnlyCollection<T> External
    {
      get { return _External ?? (_External = new ReadOnlyCollection<T>(Internal)); }
    }

    private List<T> Internal = new List<T>();

    // --------------------------------------------------------------------------------------------------------------------------
    public ExternalReadonlyList()
    {
      RefreshExternal();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private void RefreshExternal()
    {
      // By setting this to null, the collection will get refreshed when items are added or removed to it...
      _External = null;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public int IndexOf(T item)
    {
      return Internal.IndexOf(item);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Insert(int index, T item)
    {
      Internal.Insert(index, item);
      RefreshExternal();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void RemoveAt(int index)
    {
      Internal.RemoveAt(index);
      RefreshExternal();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public T this[int index]
    {
      get { return Internal[index]; }
      set { throw new NotImplementedException(); }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Add(T item)
    {
      Internal.Add(item);
      RefreshExternal();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Clear()
    {
      Internal.Clear();
      RefreshExternal();
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public bool Contains(T item)
    {
      return Internal.Contains(item);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public void CopyTo(T[] array, int arrayIndex)
    {
      int len = array.Length;
      for (int i = arrayIndex; i < len; i++)
      {
        array[i] = Internal[i];
      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public int Count
    {
      get { return Internal.Count; }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public bool IsReadOnly
    {
      get { return false; }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public bool Remove(T item)
    {
      bool res = Internal.Remove(item);
      RefreshExternal();

      return res;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public IEnumerator<T> GetEnumerator()
    {
      return Internal.GetEnumerator();
    }


    // --------------------------------------------------------------------------------------------------------------------------
    IEnumerator IEnumerable.GetEnumerator()
    {
      return Internal.GetEnumerator();
    }
  }

}
