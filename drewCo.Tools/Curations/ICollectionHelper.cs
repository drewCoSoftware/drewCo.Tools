using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace drewCo.Curations
{


  // ============================================================================================================================
  /// <summary>
  /// Provides useful manipulations for ICollections.
  /// </summary>
  public static class ICollectionHelper
  {
    private static Random RNG = new Random();

    // TODO: 12.14.2011
    // A cool function that will let us remove / filter data in a source list based on data in the comparison list.
    // A condition (FUNC<T>) would also be a nice option instead of a straight match.

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns a random entry from the given collection.
    /// </summary>
    public static T GetRandomEntry<T>(ICollection<T> src)
    {
      if (src.Count < 1) { throw new InvalidOperationException("Source collection must have at least one entry!"); }

      int index = RNG.Next(0, src.Count);

      return src.ElementAt(index);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Adds an object to the given collection as long as it is not null.
    /// _TEST:
    /// </summary>
    public static void AddNonNull<T>(ICollection<T> src, T addTo)
      where T : class
    {
      if (addTo != null) { src.Add(addTo); }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// If the given item exists in the source collection, a reference to it will be returned.
    /// If not, it will be added to the source collection.
    /// _TEST: Demonstrate this cool functionality.  Both found, and add cases!
    /// </summary>
    public static T GetOrAddItem<T>(ICollection<T> src, T item)
    {
      for (int i = 0; i < src.Count; i++)
      {
        if (src.ElementAt(i).Equals(item)) { return item; }
      }
      src.Add(item);
      return item;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Adds a new item to the collection, but only if it doesn't already exist.
    /// FUTURE: Some way to have a custom equality evaluation, like that of the item listed in 'IEnumHelpers'
    /// </summary>
    /// <remarks>A HashSet is probably a better choice instead of a function like this.</remarks>
    public static void AddUnique<T>(ICollection<T> src, T item)
    {
      if (src.Contains(item)) { return; }
      src.Add(item);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Merges the elements of <paramref name="from"/> into <paramref name="mergeTo"/>
    /// <paramref name="mergeTo"/> Will be modified.
    /// _TEST:
    /// </summary>
    public static void MergeInto<T>(ICollection<T> mergeTo, ICollection<T> from)
    {
      for (int i = 0; i < from.Count; i++)
      {
        mergeTo.Add(from.ElementAt(i));
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Like that of 'MergeInto' but only includes unique items from <paramref name="from"/>
    /// </summary>
    public static void MergeUniqueInto<T>(ICollection<T> mergeTo, ICollection<T> from)
    {
      for (int i = 0; i < from.Count; i++)
      {
        AddUnique(mergeTo, from.ElementAt(i));
      }
    }


  }

}
