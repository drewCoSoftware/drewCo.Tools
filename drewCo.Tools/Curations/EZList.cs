// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright (c)2017-2022 Andrew A. Ritz, all rights reserved.
// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;
using System.ComponentModel;

namespace drewCo.Curations
{

  // ============================================================================================================================
  /// <summary>
  /// Useed to indicate when items are added / removed from an EZList instance.  Because of a giant fail on the part of MS,
  /// we can't raise proper collection changed notifications.  We can only use 'Reset', or suffer an access violation in XAML
  /// components.
  /// </summary>
  public class ItemsChangedEventArgs : EventArgs
  {
    public readonly object[] Items = null;

    // --------------------------------------------------------------------------------------------------------------------------
    public ItemsChangedEventArgs(object item)
    {
      Items = new object[] { item };
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public ItemsChangedEventArgs(IList<object> items)
    {
      if (items == null) { throw new ArgumentNullException("items"); }

      int len = items.Count;
      Items = new object[len];
      for (int i = 0; i < len; i++)
      {
        Items[i] = items[i];
      }
    }
  }


  // ============================================================================================================================
  /// <summary>
  /// A simple extension of List(of T) that supports change notification.
  /// </summary>
  public class EZList<T> : IList, IList<T>, INotifyCollectionChanged, INotifyPropertyChanged
  {
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    /// <summary>
    /// Use this to overcome the stupid CollectionChanged flaw.
    /// </summary>
    public event EventHandler<ItemsChangedEventArgs> ItemsRemoved;

    /// <summary>
    /// Use this to overcome the stupid CollectionChanged flaw.
    /// </summary>
    public event EventHandler<ItemsChangedEventArgs> ItemsAdded;
    public event PropertyChangedEventHandler PropertyChanged;

    private object _SyncRoot = new object();
    private List<T> _List = new List<T>();

    public T this[int index] { get { return _List[index]; } set { _List[index] = value; } }

    public int Count { get { return _List.Count; } }


    // public int Count => _List.Count;
    public bool IsReadOnly => false;
    public bool IsFixedSize => false;
    public bool IsSynchronized => true;
    public object SyncRoot => _SyncRoot;

    object IList.this[int index] { get { return _List[index]; } set { _List[index] = (T)value; } }

    // --------------------------------------------------------------------------------------------------------------------------
    private void NotifyChange(string propertyName)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Add(T item)
    {
      _List.Add(item);
      NotifyChange(nameof(Count));
      if (CollectionChanged != null)
      {
        CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, _List.Count - 1));
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Clear()
    {
      _List.Clear();
      NotifyChange(nameof(Count));

      if (CollectionChanged != null)
      {
        CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public bool Contains(T item)
    {
      return _List.Contains(item);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void CopyTo(T[] array, int arrayIndex)
    {
      _List.CopyTo(array, arrayIndex);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public IEnumerator<T> GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public int IndexOf(T item)
    {
      return _List.IndexOf(item);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Insert(int index, T item)
    {
      _List.Insert(index, item);
      if (CollectionChanged != null)
      {
        CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
      }

      NotifyChange(nameof(Count));
      ItemsAdded?.Invoke(this, new ItemsChangedEventArgs(item));
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public bool Remove(T item)
    {
      bool res = _List.Remove(item);
      if (res && CollectionChanged != null)
      {
        // NOTE: The 'Remove' actions will cause access violations in downstream XAML components.
        // A special thanks goes out to MS for allowing this garbage to make it to production!
        var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset); //new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item);
        CollectionChanged(this, args);

        ItemsRemoved?.Invoke(this, new ItemsChangedEventArgs(item));
        NotifyChange(nameof(Count));
      }

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void RemoveAt(int index)
    {
      T toRemove = _List[index];
      _List.RemoveAt(index);
      if (CollectionChanged != null)
      {
        // NOTE: The 'Remove' actions will cause access violations in downstream XAML components.
        // A special thanks goes out to MS for allowing this garbage to make it to production!
        var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
        CollectionChanged(this, args);

        ItemsRemoved?.Invoke(this, new ItemsChangedEventArgs(toRemove));
        NotifyChange(nameof(Count));
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    IEnumerator IEnumerable.GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns the index of the new item.
    /// </summary>
    public int Add(object value)
    {
      T item = (T)value;
      this.Add(item);

      int index = _List.Count - 1;

      CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));

      ItemsAdded?.Invoke(this, new ItemsChangedEventArgs(item));
      NotifyChange(nameof(Count));

      return index;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public bool Contains(object value)
    {
      return _List.Contains((T)value);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public int IndexOf(object value)
    {
      return _List.IndexOf((T)value);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Insert(int index, object value)
    {
      Insert(index, (T)value);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Remove(object value)
    {
      Remove((T)value);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void CopyTo(Array array, int index)
    {
      _List.CopyTo((T[])array, index);
    }
  }


}
