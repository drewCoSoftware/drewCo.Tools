// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright (c)2010-2014 Andrew A. Ritz, all rights reserved.
// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace drewCo.Curations
{

  // ============================================================================================================================
  /// <summary>
  /// Like a normal list, but disposes items when they are removed.
  /// </summary>
  public class DisposingList<T> : IList<T>, IDisposable
    where T : IDisposable
  {
    private List<T> List = new List<T>();


    // --------------------------------------------------------------------------------------------------------------------------
    public DisposingList()
    { }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Create a disposing list from a normal source list.
    /// </summary>
    public DisposingList(IList<T> source)
    {
      List = source.ToList();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public int IndexOf(T item)
    {
      return List.IndexOf(item);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Insert(int index, T item)
    {
      List.Insert(index, item);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void RemoveAt(int index)
    {
      // _TEST: TODO: We need to look to see if there are other items in the list that match the
      // reference.  If there are, we should remove them as well ?
      T item = List[index];
      item.Dispose();
      List.RemoveAt(index);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public T this[int index]
    {
      get
      {
        return List[index];
      }
      set
      {
        // TODO: We must check the references.  If they are different, then we should dispose the existing item.
        throw new NotImplementedException();
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Add(T item)
    {
      List.Add(item);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Clear()
    {
      foreach (var item in List)
      {
        item.Dispose();
      }
      List.Clear();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public bool Contains(T item)
    {
      throw new NotImplementedException();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void CopyTo(T[] array, int arrayIndex)
    {
      throw new NotImplementedException();
    }

#region Properties

    public int Count
    {
      get { return List.Count; }
    }

    public bool IsReadOnly
    {
      get { throw new NotImplementedException(); }
    }

#endregion

    // --------------------------------------------------------------------------------------------------------------------------
    public bool Remove(T item)
    {
      // _TEST: Make sure that calling this won't dispose the item if it doesnt' exist in the list!
      if (List.Contains(item))
      {
        item.Dispose();
      }
      return List.Remove(item);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public IEnumerator<T> GetEnumerator()
    {
      throw new NotImplementedException();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      throw new NotImplementedException();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Clear the list, effectively disposing each item contained within.
    /// </summary>
    public void Dispose()
    {
      this.Clear();
    }
  }
}
