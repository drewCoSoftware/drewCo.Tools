// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright ©2012-2014 Andrew A. Ritz, all rights reserved.
//
// This code is released under the ms-pl (Microsoft Public License)
// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace drewCo
{

  // ============================================================================================================================
  /// <summary>
  /// Represents an abstracted source of data.  Basically members on a class, either fields or properties.
  /// </summary>
  public abstract class DataSource
  {
    public object Source { get; set; }


    protected MethodInfo Setter = null;
    protected MethodInfo Getter = null;


    public Type MemberType { get; protected set; }
    public string Name { get; protected set; }

    public abstract void ResolveSetter();
    public abstract void ResolveGetter();

    public abstract object GetPropData();
    public abstract void SetPropData(object data);
  }



  // ============================================================================================================================
  public abstract class DataSource<TMember> : DataSource
  {


    private TMember _MemberInfo;
    public TMember MemberInfo
    {
      get { return _MemberInfo; }
      protected set
      {
        _MemberInfo = value;

        ResolveGetter();
        ResolveSetter();
      }
    }
  }


  // ============================================================================================================================
  public class FieldDataSource : DataSource<FieldInfo>
  {
    // --------------------------------------------------------------------------------------------------------------------------
    public FieldDataSource(object src_, FieldInfo fieldInfo_)
    {
      //if (src_ == null)
      //{
      //  throw new ArgumentNullException();
      //}
      Source = src_;
      MemberInfo = fieldInfo_;

      MemberType = MemberInfo.FieldType;
      Name = MemberInfo.Name;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public override void ResolveSetter()
    { }

    // --------------------------------------------------------------------------------------------------------------------------
    public override void ResolveGetter()
    { }

    // --------------------------------------------------------------------------------------------------------------------------
    public override object GetPropData()
    {
      return MemberInfo.GetValue(Source);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public override void SetPropData(object data)
    {
      MemberInfo.SetValue(Source, data);
    }
  }

  // ============================================================================================================================
  public class PropDataSource : DataSource<PropertyInfo>
  {
    // --------------------------------------------------------------------------------------------------------------------------
    public PropDataSource(object src_, PropertyInfo propInfo_)
    {
      //if (src_ == null)
      //{
      //  throw new ArgumentNullException();
      //}
      Source = src_;
      MemberInfo = propInfo_;

      MemberType = MemberInfo.PropertyType;
      Name = MemberInfo.Name;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public override void ResolveGetter()
    {
#if NETFX_CORE
      Getter = MemberInfo.GetMethod;
#else
      Getter = MemberInfo.GetGetMethod();
#endif
      if (Getter == null) { throw new Exception(string.Format("The getter on property '{0}' is not available!", MemberInfo.Name)); }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public override void ResolveSetter()
    {
#if NETFX_CORE
      Setter = MemberInfo.SetMethod;
#else
      Setter = MemberInfo.GetSetMethod();
#endif
      if (Setter != null) { return; }

#if !NETFX_CORE
      Setter = MemberInfo.GetSetMethod(true);
#endif
      if (Setter == null)
      {
        string msg = "The setter on property '{0}' is not available!  Creating a 'private set' method may be the solution.";
        throw new Exception(string.Format(msg, MemberInfo.Name));
      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public override object GetPropData()
    {
      return Getter.Invoke(Source, null);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public override void SetPropData(object data)
    {
      Setter.Invoke(Source, new[] { data });
    }


  }


}
