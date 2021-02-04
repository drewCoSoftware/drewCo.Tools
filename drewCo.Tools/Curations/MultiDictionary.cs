// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright (c)2010-2014 Andrew A. Ritz, all rights reserved. 
//
// MultiDictionary.cs
// Code to provide Dictionary like objects that use a natural keys instead of single keys.
// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace drewCo.Curations
{
  // ============================================================================================================================
  /// <summary>
  /// Like a normal dictionary, but with a natural key instead of a single one.
  /// </summary>
#if IS_TOOLS_LIB
  public class MultiDictionary<TKey1, TKey2, TValue> : IEnumerable<TValue>
#else
  internal class MultiDictionary<TKey1, TKey2, TValue> : IEnumerable<TValue>
#endif
  {
    private Dictionary<TKey1, List<Dictionary<TKey2, TValue>>> Data = new Dictionary<TKey1, List<Dictionary<TKey2, TValue>>>();

    #region Properties

    private List<TValue> _Values = new List<TValue>();
    public ICollection<TValue> Values
    {
      get { return _Values; }
    }

    private List<Tuple<TKey1, TKey2>> _Keys = new List<Tuple<TKey1, TKey2>>();
    public ICollection<Tuple<TKey1, TKey2>> Keys
    {
      get { return _Keys; }
    }

    #endregion

    // --------------------------------------------------------------------------------------------------------------------------
    public void Clear()
    {
      // Clear everything out!
      // NOTE: I wonder if this is actually sufficient.....
      // NOTE: This method may need to be part of some 'IMultiDictionary' interface should we ever get that far....
      Data.Clear();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Check to see if the first key is used in this instance.
    /// </summary>
    public bool ContainsKey1(TKey1 key1)
    {
      bool res = Data.ContainsKey(key1);
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Check to see if the second key is used in this instance.
    /// </summary>
    public bool ContainsKey2(TKey2 key2)
    {
      foreach (List<Dictionary<TKey2, TValue>> item in Data.Values)
      {
        if (item.Any(x => x.ContainsKey(key2)))
        {
          return true;
        }
      }
      return false;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public bool ContainsKey(TKey1 key1, TKey2 key2)
    {
      if (Data.ContainsKey(key1))
      {
        foreach (var item in Data[key1])
        {
          if (item.ContainsKey(key2)) { return true; }
        }
      }
      return false;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Add(TKey1 key1, TKey2 key2, TValue value)
    {
      ValidateAddKey(key1, key2);

      if (!Data.ContainsKey(key1))
      {
        Data.Add(key1, new List<Dictionary<TKey2, TValue>>());
      }

      Dictionary<TKey2, TValue> child = new Dictionary<TKey2, TValue>()
      {
        {key2, value}
      };

      Data[key1].Add(child);

      // Shove the value in anyway.
      Values.Add(value);
      Keys.Add(new Tuple<TKey1, TKey2>(key1, key2));
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public TValue this[TKey1 key1, TKey2 key2]
    {
      get
      {
        foreach (var item in Data[key1])
        {
          if (item.ContainsKey(key2)) { return item[key2]; }
        }
        throw new KeyNotFoundException(string.Format("The key '{0}:{1}' could not be found!", key1, key2));
      }
      set
      {
        foreach (var item in Data[key1])
        {
          if (item.ContainsKey(key2)) { item[key2] = value; }
        }
        throw new KeyNotFoundException(string.Format("The key '{0}:{1}' could not be found!", key1, key2));
      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Makes sure that the key we are trying to add is indeed valid!
    /// Also is responsible for adding the 'key1' list into the system.
    /// </summary>
    private void ValidateAddKey(TKey1 key1, TKey2 key2)
    {
      // TODO: This sure could use some performance improvements, don't ya think?
      if (KeyExists(key1, key2))
      {
        throw new ArgumentException(string.Format("The key '{0}:{1}' has already been used!", key1, key2));
      }

    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Removes items from the dictionary.
    /// </summary>
    public void Remove(TKey1 key1, TKey2 key2)
    {
      if (!KeyExists(key1, key2))
      {
        throw new ArgumentException(string.Format("The key '{0}:{1}' does not exist in the dictionary!", key1, key2));
      }

      // TODO: This is basically a repeat of checking for the existance of the key.  We can (and should) see it removed.
      foreach (var item in Data[key1])
      {
        if (item.ContainsKey(key2))
        {
          Values.Remove(item[key2]);
          Data[key1].Remove(item);
          Keys.Remove(new Tuple<TKey1, TKey2>(key1, key2));
          break;
        }
      }

    }

    // --------------------------------------------------------------------------------------------------------------------------
    private bool KeyExists(TKey1 key1, TKey2 key2)
    {
      if (Data.ContainsKey(key1))
      {
        foreach (var item in Data[key1])
        {
          if (item.ContainsKey(key2))
          {
            return true;
          }
        }
      }
      return false;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
    {
      return new Enumerator(this);
    }


    // ==========================================================================================================================
    private struct Enumerator : IEnumerator<TValue>
    {
      private MultiDictionary<TKey1, TKey2, TValue> Dict;
      private ICollection<Tuple<TKey1, TKey2>> Keys;

      // The current index and represented value.
      private int Index;
      private TValue _Current;

      // --------------------------------------------------------------------------------------------------------------------------
      public Enumerator(MultiDictionary<TKey1, TKey2, TValue> dict_)
      {
        Dict = dict_;
        Keys = Dict.Keys;

        Index = 0;
        _Current = default(TValue);
      }

      #region Properties

      public TValue Current
      {
        get
        {
          return _Current;
        }
      }

      object IEnumerator.Current
      {
        get { return Current; }
      }

      #endregion

      // --------------------------------------------------------------------------------------------------------------------------
      public bool MoveNext()
      {
        Index++;
        if (Index > Keys.Count) { return false; }

        var curKey = Keys.ElementAt(Index - 1);
        _Current = Dict[curKey.Item1, curKey.Item2];
        return true;
      }

      // --------------------------------------------------------------------------------------------------------------------------
      public void Reset()
      {
        Index = 0;
        _Current = default(TValue);
      }

      // --------------------------------------------------------------------------------------------------------------------------
      public void Dispose()
      {
        Keys.Clear();
        Keys = null;
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    IEnumerator IEnumerable.GetEnumerator()
    {
      throw new NotImplementedException();
    }
  }



}
