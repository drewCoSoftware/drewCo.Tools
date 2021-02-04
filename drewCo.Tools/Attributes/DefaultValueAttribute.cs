using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace drewCo.Tools
{

  // ============================================================================================================================
  /// <summary>
  /// This is used when setting default values for XMLFile properties, etc.
  /// These values are set upon object creation.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property)]
  public class DefaultValueAttribute : Attribute
  {
    // --------------------------------------------------------------------------------------------------------------------------
    public DefaultValueAttribute()
    { }

    // --------------------------------------------------------------------------------------------------------------------------
    public DefaultValueAttribute(object value_)
    {
      Value = value_;
    }

    /// <summary>
    /// Indicate that a new instance of the property type should be used.
    /// Useful for populating lists, etc.
    /// </summary>
    /// <remarks>
    /// If this is set to true, then the 'Value' property will be ignored.
    /// </remarks>
    public bool UseNewInstace { get; set; }
    public object Value { get; set; }
  }

}
