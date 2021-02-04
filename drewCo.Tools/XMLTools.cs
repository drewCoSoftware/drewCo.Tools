//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright ©2018 Andrew A. Ritz, All Rights Reserved
//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace drewCo.Tools
{

  // ============================================================================================================================
  public static class XMLTools
  {

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Serialize the given data to an XML fragment.
    /// </summary>
    public static XElement SerializeFragment<T>(T data)
    {
      Type t = typeof(T);
      var rootAttr = ReflectionTools.GetAttribute<XmlRootAttribute>(t);

      string useName = rootAttr == null ? t.Name : rootAttr.ElementName;

      XElement res = new XElement(useName);

      var props = ReflectionTools.GetPropertiesWithAttribute<T, XmlAttributeAttribute>().ToList();
      foreach (var p in props)
      {
        string attrName = ReflectionTools.GetAttribute<XmlAttributeAttribute>(p).AttributeName;
        object value = p.GetValue(data, null);
        if (value != null)
        {
          res.Add(new XAttribute(attrName, value));
        }
      }

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Deserialize the given XElement into the corresponding type.
    /// </summary>
    /// <remarks>
    /// At this time, this function will only deserialize attributes defined on the fragment.  Child elements
    /// will be ignored.
    /// </remarks>
    public static T DeserializeFragment<T>(XElement src)
      where T : new()
    {

      T res = new T();
      Type t = typeof(T);

      var props = ReflectionTools.GetPropertiesWithAttribute<T, XmlAttributeAttribute>().ToList();

      foreach (var p in props)
      {
        // string name = a.Name;
        string attrName = ReflectionTools.GetAttribute<XmlAttributeAttribute>(p).AttributeName;
        XAttribute xattr = src.Attribute(attrName);
        if (xattr != null)
        {
          object val = ReflectionTools.ConvertEx(p.PropertyType, xattr.Value);
          p.SetValue(res, val, null);
        }
      }


      return res;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public static void AddAttribute(XElement elem, string attrName, object attrValue)
    {
      if (attrValue != null)
      {
        elem.Add(new XAttribute(attrName, attrValue.ToString()));
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    // TODO: Put this with some XML tools?
    public static T ResolveAttribute<T>(XElement elem, string attrName, T fallback = default(T))
    {
      XAttribute attr = elem.Attribute(attrName);
      if (attr == null) { return fallback; }

      return ReflectionTools.ConvertEx<T>(attr.Value);
    }

  }


}
