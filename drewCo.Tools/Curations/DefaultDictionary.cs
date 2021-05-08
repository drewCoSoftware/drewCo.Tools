// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright (c)2010-2018 Andrew A. Ritz, all rights reserved.
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
  /// <summary>
  /// Like a normal dictionary, but will provide a default value in place of 'KeyNotFoundExceptions' being thrown.
  /// Optionally, you may provide a function that will supply a default value for a given key.
  /// </summary>
  public class DefaultDictionary<TKey, TValue> : Dictionary<TKey, TValue>
  {
    private TValue Default = default(TValue);
    private Func<TKey, TValue> Generator = null;
    private bool _CreateDefault = false;

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// New instances of TKey will be created for default instances.
    /// </summary>
    /// <remarks>Creating a new instance as a default will not work for types that don't have a default constructor.</remarks>
    public DefaultDictionary()
    {
      _CreateDefault = true;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public DefaultDictionary(Func<TKey, TValue> generator_)
    {
      if (generator_ == null) { throw new ArgumentNullException("generator_"); }
      Generator = generator_;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public DefaultDictionary(TValue default_)
      : base()
    {
      Default = default_;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public new TValue this[TKey key]
    {
      get
      {
        if (!base.ContainsKey(key))
        {
          if (_CreateDefault)
          {
            TValue res = Activator.CreateInstance<TValue>();
            return res;
          }
          else if (Generator == null)
          {
            return Default;
          }

          TValue useVal = Generator.Invoke(key);
          base[key] = useVal;

          return useVal;

        }
        return base[key];
      }
      set { base[key] = value; }
    }

  }
}
