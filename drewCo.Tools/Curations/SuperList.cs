// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright (c)2010-2017 Andrew A. Ritz, all rights reserved.
//
// This code is released under the ms-pl (Microsoft Public License)
// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace drewCo.Curations
{
  // ============================================================================================================================
  public delegate void ListChangedHandler<T>(object sender, ChangeListEventArgs<T> e);

  // ============================================================================================================================
  /// <summary>
  /// It's like a normal list, but has events for when items are added or removed.
  /// </summary>
  [Obsolete("Use EZList instead.  It uses proper events that can be used in other places.")]
  public class SuperList<T> : List<T>
  {
    private static Random RNG = new Random();

    public event ListChangedHandler<T> ItemAdded;
    public event ListChangedHandler<T> ItemRemoved;

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Randomizes the order of the items in the list.
    /// </summary>
    public void Shuffle()
    {
      SuperList<int> indexes = new SuperList<int>();
      for (int i = 0; i < this.Count; i++)
      {
        indexes.Add(i);
      }

      for (int i = 0; i < indexes.Count; i++)
      {
        int from = indexes.GetRandom(true);
        int to = indexes.Count == 0 ? 0 : indexes.GetRandom(true);

        // Swap them...
        T src = this[from];
        T dest = this[to];
        this[from] = dest;
        this[to] = src;
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets a random entry from the list, optionally removing it at the same time.
    /// </summary>
    public T GetRandom(bool remove = false)
    {
      int rand = RNG.Next(0, Count - 1);
      T val = this[rand];
      if (remove) { this.RemoveAt(rand); }

      return val;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public new void Add(T item)
    {
      base.Add(item);
      if (ItemAdded != null)
      {
        ItemAdded(this, new ChangeListEventArgs<T>(item));
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public new void Remove(T item)
    {
      base.Remove(item);
      if (ItemRemoved != null)
      {
        ItemRemoved(this, new ChangeListEventArgs<T>(item));
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public new void AddRange(IEnumerable<T> collection)
    {
      throw new NotImplementedException("Decide what to do about the events!");
      base.AddRange(collection);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public new void Clear()
    {
      throw new NotImplementedException("Decide what to do about the events!");
    }
  }

  // ============================================================================================================================
  public class ChangeListEventArgs<T> : EventArgs
  {
    public readonly T Item;

    // --------------------------------------------------------------------------------------------------------------------------
    public ChangeListEventArgs(T item_)
    {
      Item = item_;
    }
  }



}
