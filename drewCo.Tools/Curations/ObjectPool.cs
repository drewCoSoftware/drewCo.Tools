using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace drewCo.Curations
{
  // ============================================================================================================================
  public class ObjectPool
  {
    /// <summary>
    /// Indicated that an unlimited number of items can be used in the pool.
    /// This may cause problems if you aren't careful.  
    /// </summary>
    public const int UNLIMITED = 0;

  }

  // ============================================================================================================================
  /// <summary>
  /// Maintains a pool of objects so that they can be reused instead of allocated / deallocated all of the time.
  /// </summary>
  public class ObjectPool<T> : ObjectPool
    where T : class
  {

    /// <summary>
    /// The maximum number of items allowed in the pool.
    /// </summary>
    private int _MaxItems = UNLIMITED;
    public virtual int MaxItems
    {
      get { return _MaxItems; }
      protected set { _MaxItems = value; }
    }

    /// <summary>
    /// Indicates that one or items may be available for reuse.
    /// </summary>
    public bool HasAvailableItem { get { return NextAvailable.Count > 0; } }

    private HashSet<T> ActiveItems = new HashSet<T>();
    private Stack<T> NextAvailable = new Stack<T>();

    // --------------------------------------------------------------------------------------------------------------------------
    public ObjectPool(int maxItems_)
    {
      _MaxItems = maxItems_;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Manually add an item that is to be used in the pool.
    /// DO NOT add the same item twice, or you will get an exception.
    /// </summary>
    public void AddItem(T newItem)
    {
      if (newItem == null) { throw new ArgumentNullException("newItem"); }
      if (MaxItems != UNLIMITED && ActiveItems.Count == MaxItems)
      {
        throw new InvalidOperationException("Exceeded max items in pool!");
      }

      ActiveItems.Add(newItem);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets the next available item.  If no item are available, an alternate generator function can be used to provide one or an
    /// exception will be thrown.
    /// </summary>
    public T GetNextAvailable(Func<T> generator = null)
    {
      T res = null;
      if (!HasAvailableItem)
      {
        if (generator == null)
        {
          throw new InvalidOperationException("There are no items available.  Please add one!");
        }
        else
        {
          res = generator();
        }
      }
      else
      {
        res = NextAvailable.Pop();
      }

      AddItem(res);

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Marks the item as being available for reuse.
    /// </summary>
    public void ReleaseItem(T item)
    {
      if (!ActiveItems.Contains(item))
      {
        throw new InvalidOperationException("The given item is not part of this pool!");
      }

      ActiveItems.Remove(item);
      NextAvailable.Push(item);
    }
  }
}
