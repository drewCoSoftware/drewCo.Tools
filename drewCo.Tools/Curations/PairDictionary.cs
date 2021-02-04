// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright (c)2010-2019 Andrew A. Ritz, all rights reserved.
// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drewCo.Curations
{

  // ============================================================================================================================
  /// <summary>
  /// This is a lot like a normal dictionary, but both the keys, and the values must be unique.
  /// From there we are allowed to look up the keys that are associated with values, etc.
  /// </summary>
  public class PairDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
  {

    protected Dictionary<TKey, TValue> _ByKey = null;
    protected Dictionary<TValue, TKey> _ByValue = null;

    protected IEqualityComparer<TKey> KeyComparer = null;
    protected IEqualityComparer<TValue> ValueComparer = null;

    // --------------------------------------------------------------------------------------------------------------------------
    public PairDictionary()
    {
      _ByKey = new Dictionary<TKey, TValue>();
      _ByValue = new Dictionary<TValue, TKey>();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public PairDictionary(IEqualityComparer<TKey> keyComparer_)
    {
      KeyComparer = keyComparer_;

      _ByKey = new Dictionary<TKey, TValue>(KeyComparer);
      _ByValue = new Dictionary<TValue, TKey>();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public PairDictionary(IDictionary<TKey, TValue> source)
      : this()
    {
      foreach (var item in source)
      {
        Add(item.Key, item.Value);
      }
    }

    #region Properties

    public virtual TKey this[TValue index]
    {
      get
      {
        return Key(index);
      }
    }

    public virtual TValue this[TKey index]
    {
      get
      {
        return Value(index);
      }
      set
      {
        _ByKey[index] = value;
      }
    }

    /// <summary>
    /// The total number of pairs stored in the ditionary.
    /// </summary>
    public int Count
    {
      get { return _ByKey.Count; }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public ReadOnlyCollection<TKey> Keys
    {
      get
      {
        return new ReadOnlyCollection<TKey>((from x in _ByKey.Keys select x).ToList());
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public ReadOnlyCollection<TValue> Values
    {
      get
      {
        return new ReadOnlyCollection<TValue>((from x in _ByValue.Keys select x).ToList());
      }
    }

    #endregion

    // --------------------------------------------------------------------------------------------------------------------------
    public  bool TryGetKey(TValue val, out TKey key)
    {
      return _ByValue.TryGetValue(val, out key);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Add(TKey key, TValue value)
    {
      _ByKey.Add(key, value);
      _ByValue.Add(value, key);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns the key for the corresponding value.
    /// </summary>
    public virtual TKey Key(TValue value)
    {
      return _ByValue[value];
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns the value for the corresponding key.
    /// </summary>
    public virtual TValue Value(TKey key)
    {
      return _ByKey[key];
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      foreach (var k in _ByKey)
      {
        yield return k;
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public bool ContainsKey(TKey key)
    {
      return _ByKey.ContainsKey(key);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public bool ContainsValue(TValue value)
    {
      return _ByValue.ContainsKey(value);
    }

    #region IDictionary Implementation;



    ICollection<TKey> IDictionary<TKey, TValue>.Keys
    {
      get { throw new NotImplementedException(); }
    }

    public bool Remove(TKey key)
    {
      throw new NotImplementedException();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public bool TryGetValue(TKey key, out TValue value)
    {
      return _ByKey.TryGetValue(key, out value);
    }

    ICollection<TValue> IDictionary<TKey, TValue>.Values
    {
      get { throw new NotImplementedException(); }
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
      throw new NotImplementedException();
    }

    public void Clear()
    {
      _ByKey.Clear();
      _ByValue.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
      throw new NotImplementedException();
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      int len = _ByKey.Count;

      int index = 0;
      foreach (var item in _ByKey)
      {
        array[index] = new KeyValuePair<TKey, TValue>(item.Key, item.Value);
        ++index;
      }
    }

    public bool IsReadOnly
    {
      get { return false; }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    // _TEST:
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
      if (_ByKey.TryGetValue(item.Key, out TValue val))
      {
        _ByKey.Remove(item.Key);
        _ByValue.Remove(val);
        return true;
      }

      return false;
    }

    #endregion

  }



  // ============================================================================================================================
  public class DefaultPairDictionary<TKey, TValue> : PairDictionary<TKey, TValue>
  {
    public TKey DefaultKey { get; private set; }
    public TValue DefaultValue { get; private set; }


    // --------------------------------------------------------------------------------------------------------------------------
    public DefaultPairDictionary(TKey defaultKey_, TValue defaultValue_, IEqualityComparer<TKey> keyComparer_)
      : base(keyComparer_)
    {
      InitDefaults(defaultKey_, defaultValue_);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public DefaultPairDictionary(TKey defaultKey_, TValue defaultValue_)
      : base()
    {
      InitDefaults(defaultKey_, defaultValue_);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private void InitDefaults(TKey defaultKey_, TValue defaultValue_)
    {
      DefaultKey = defaultKey_;
      DefaultValue = defaultValue_;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public override TValue Value(TKey key)
    {
      if (!_ByKey.TryGetValue(key, out TValue val))
      {
        return DefaultValue;
      }
      else
      {
        return val;
      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public override TKey Key(TValue value)
    {
      if (!_ByValue.TryGetValue(value, out TKey key))
      {
        return DefaultKey;
      }
      else
      {
        return key;
      }
    }

  }

}
