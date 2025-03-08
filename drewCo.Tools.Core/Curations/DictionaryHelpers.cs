using drewCo.Tools;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;

namespace drewCo.Curations;

// ==============================================================================================================================
/// <summary>
/// Helpful functions for dictionary (map) related tasks.
/// </summary>
public static class DictionaryHelpers {

  // --------------------------------------------------------------------------------------------------------------------------
  /// <summary>
  /// Partitions the list of inputs into a dictionary using a function to derive a key from those inputs.
  /// </summary>
  private static Dictionary<TKey, List<TInput>> Partition<TInput, TKey>(List<TInput> input, Func<TInput, TKey> keyGenerator)
  {
    var res = new Dictionary<TKey, List<TInput>>();
    foreach (var item in input)
    {
      TKey useKey = keyGenerator(item);
      if (!res.TryGetValue(useKey, out List<TInput> list))
      {
        list = new List<TInput>();
        res[useKey] = list;
      }

      list.Add(item);
    }

    return res;
  }

  // --------------------------------------------------------------------------------------------------------------------------
  /// <summary>
  /// Partitions the inputs into lists grouped by property value.
  /// </summary>
  public static Dictionary<TKey, List<TInput>> PartitionIntoDictionary<TInput, TKey>(List<TInput> input, Expression<Func<TInput, TKey>> keyPropExp)
  {
    var res = new Dictionary<TKey, List<TInput>>();

    foreach (var item in input)
    {
      TKey key = (TKey)ReflectionTools.GetPropertyValue(item, keyPropExp);
      if (!res.TryGetValue(key, out var list))
      {
        list = new List<TInput>();
        res[key] = list;
      }
      list.Add(item);
    }

    return res;

  }

  // --------------------------------------------------------------------------------------------------------------------------
  /// <summary>
  /// Partitions properties of the inputs into lists grouped by property value.
  /// </summary>
  public static Dictionary<TKey, List<TVal>> PartitionIntoDictionary<TInput, TKey, TVal>(List<TInput> input, Expression<Func<TInput, TKey>> keyPropExp, Expression<Func<TInput, TVal>> valPropExp)
  {
    var res = new Dictionary<TKey, List<TVal>>();

    foreach (var item in input)
    {
      TKey key = (TKey)ReflectionTools.GetPropertyValue(item, keyPropExp);
      if (!res.TryGetValue(key, out var list))
      {
        list = new List<TVal>();
        res[key] = list;
      }

      TVal val = (TVal)ReflectionTools.GetPropertyValue(item, valPropExp);
      list.Add(val);
    }

    return res;
  }


  // --------------------------------------------------------------------------------------------------------------------------
  /// <summary>
  /// Partitions the list of values, computed from the inputs, into a dictionary using a function to derive a key from those inputs.
  /// </summary>
  public static Dictionary<TKey, List<TValue>> Partition<TInput, TKey, TValue>(List<TInput> input, Func<TInput, TKey> keyGenerator, Func<TInput, TValue> valGenerator)
  {
    var res = new Dictionary<TKey, List<TValue>>();
    foreach (var item in input)
    {
      TKey useKey = keyGenerator(item);
      if (!res.TryGetValue(useKey, out List<TValue> list))
      {
        list = new List<TValue>();
        res[useKey] = list;
      }

      TValue useval = valGenerator(item);
      list.Add(useval);
    }

    return res;
  }


}