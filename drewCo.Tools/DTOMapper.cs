//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright ©2014-2022 Andrew A. Ritz, All Rights Reserved
//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace drewCo.Tools
{
  // ============================================================================================================================
  /// <summary>
  /// Just a class to help us copy values between types.  In any mutli-tiered system, there are plenty of DTOs to keep things
  /// isolated, but manual copying can be a real pain...
  /// </summary>
  public class DTOMapper
  {


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Creates an object of the given type, using <paramref name="copyFrom"/> as the property source.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="copyFrom"></param>
    /// <returns></returns>
    public static object CreateFrom(Type t, object copyFrom, bool allowNulls = true)
    {
      if (copyFrom == null)
      {
        if (!allowNulls)
        {
          { throw new ArgumentNullException("copyFrom"); }
        }
        return null;
      }

      object res = Activator.CreateInstance(t);

      Type fromType = copyFrom.GetType();

      // We need to create a generic method here.
      // NOTE: This would actually be a useful function to add to reflection tools.
      MethodInfo m = (from x in ReflectionTools.GetMethods(typeof(DTOMapper), nameof(CopyMembers))
                      where x.IsGenericMethod
                      select x).Single();
      m = m.MakeGenericMethod(fromType, t);

      m.Invoke(null, new object[] { copyFrom, res, allowNulls });


      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Copy a single property from source to target by name.  If there is no matching property with the same name and
    /// type then nothing will happen.
    /// </summary>
    public static void CopyProperty<TFrom, TTo>(string name, TFrom from, TTo to)
    {
      CopyProperty(name, typeof(TFrom), typeof(TTo), from, to);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void CopyProperty(string name, Type tFrom, Type tTo, object from, object to)
    {
      PropertyInfo pFrom = ReflectionTools.GetPropertyInfo(tFrom, name, false, false);
      PropertyInfo pTo = ReflectionTools.GetPropertyInfo(tTo, name, false, false);

      if (pFrom == null | pTo == null) { return; }

      pTo.SetValue(to, ReflectionTools.ConvertEx(pTo.PropertyType, pFrom.GetValue(from, null)), null);
    }


    //// --------------------------------------------------------------------------------------------------------------------------
    ///// <summary>
    ///// Non-Generic version of CopyProperties(TFrom, TTo)
    ///// </summary>
    //public static void CopyProperties(Type tFrom, Type tTo, object from, object to, bool allowNulls = true)
    //{
    //  var mi = (from x in ReflectionTools.GetMethods(typeof(DTOMapper))
    //            where x.IsGenericMethod
    //            select x).Single();

    //  mi = mi.MakeGenericMethod(new[] { tFrom, tTo });
    //  mi.Invoke(null, new object[] { from, to });
    //}


    //// --------------------------------------------------------------------------------------------------------------------------
    //public static void CopyMembers(object from, object to, bool allowNulls = true)
    //{
    //  Type t1 = from.GetType();
    //  Type t2 = to.GetType();

    //  MethodInfo m = (from x in typeof(DTOMapper).GetMethods(BindingFlags.Static | BindingFlags.Public)
    //                  where x.Name == "CopyMembers" && x.IsGenericMethod
    //                  select x).Single().MakeGenericMethod(new[]
    //                  {
    //                        t1,
    //                        t2
    //                      });
    //  m.Invoke(null, new object[] { from, to, allowNulls });
    //}

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Automatically copies properties of the same name and type from one object to another...
    /// If nulls are allowed, and either item is null, no action will be taken.
    /// </summary>
    /// <param name="allowNulls">Allows the <param name="from"/> and <paramref name="to"/> to be null.</param>
    public static void CopyMembers<TFrom, TTo>(TFrom from, TTo to, bool allowNulls = true)
    {
      if (!allowNulls)
      {
        if (from == null) { throw new ArgumentNullException("from"); }
        if (to == null) { throw new ArgumentNullException("to"); }
      }

      if (from == null || to == null) { return; }

      DataMember[] fromMembers = GetDataMembers(from.GetType()); // <TFrom>();
      DataMember[] toMembers = GetDataMembers(to.GetType()); // <TTo>();


      // Obviously it would make a lot more sense to cache this, as it could be very expensive
      // with each copy.....
      foreach (var fromMember in fromMembers)
      {
        foreach (var toMember in toMembers)
        {

          if (fromMember.Name == toMember.Name)
          {

            if (fromMember.DataType == toMember.DataType && toMember.CanWrite)
            {
#if NET_40
              Type fromType = fromMember.DataType;
              object fromVal = fromMember.GetValue(from);
              if (ReflectionTools.HasInterface<IList>(fromType))
              {
                object toList = CopyListData(fromVal);
                toMember.SetValue(to, toList);
              }
              else
              {
                if (ReflectionTools.IsSimpleType(fromType) || fromType.IsEnum)
                {
                  toMember.SetValue(to, fromVal);
                }
                else
                {
                  object toVal = CreateCopy(fromType, fromVal);
                  toMember.SetValue(to, toVal);
                }
              }
#else
              Type fromType = fromMember.DataType;
              object fromVal = fromMember.GetValue(from);
              if (ReflectionTools.HasInterface<IList>(fromType))
              {
                object toList = CopyListData(fromVal);
                toMember.SetValue(to, toList);
              }
              else
              {
                if (ReflectionTools.IsSimpleType(fromType) || fromType.GetTypeInfo().IsEnum)
                {
                  toMember.SetValue(to, fromVal);
                }
                else
                {
                  object toVal = CreateCopy(fromType, fromVal);
                  toMember.SetValue(to, toVal);
                }
              }
              //blerp..  this won't compile.  See the above example please.
              //object fromVal = fromProp.GetValue(from);
              //toProp.SetValue(to, fromVal);
#endif
            }
            else
            {
              // Test for compatible lists.
              Type listType1 = null;
              Type listType2 = null;
              if (AreCompatibleLists(fromMember, toMember, out listType1, out listType2))
              {
                CopyListDataEx(from, to, fromMember, toMember, listType1, listType2);
              }
              else
              {
                // These may be composite types, in which case we can attempt to copy anyway.
                // We know they are disimilar, so we can just create a new instance to copy to.
#if NETFX_CORE
                TypeInfo fromTi = fromMember.DataType.GetTypeInfo();
                TypeInfo toTi = toMember.DataType.GetTypeInfo();
                if (fromTi.IsClass && toTi.IsClass && toMember.DataType != typeof(string))
#else
                if (fromMember.DataType.IsClass && toMember.DataType.IsClass && toMember.DataType != typeof(string))
#endif
                {
                  CopyComposite(from, to, allowNulls, fromMember, toMember);
                }
              }
            }
          }

        }
      }

    }

    // --------------------------------------------------------------------------------------------------------------------------
    private static DataMember[] GetDataMembers(Type t)
    {
      var props = ReflectionTools.GetProperties(t);
      var fields = ReflectionTools.GetFields(t, false);

      var res = new List<DataMember>();

      foreach (var p in props)
      {
        res.Add(new DataMember(p));
      }
      foreach (var f in fields)
      {
        res.Add(new DataMember(f));
      }

      return res.ToArray();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private static DataMember[] GetDataMembers<T>()
    {
      return GetDataMembers(typeof(T));
      //var props = ReflectionTools.GetProperties<T>();
      //var fields = ReflectionTools.GetFields<T>();

      //var res = new List<DataMember>();

      //foreach (var p in props)
      //{
      //  res.Add(new DataMember(p));
      //}
      //foreach (var f in fields)
      //{ 
      //  res.Add(new DataMember(f));
      //}

      //return res.ToArray();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private static void CopyComposite<TFrom, TTo>(TFrom from, TTo to, bool allowNulls, DataMember fromMember, DataMember toMember)
    {
      object fromVal = fromMember.GetValue(from);
      object copied = CreateFrom(toMember.DataType, fromVal, allowNulls);
      toMember.SetValue(to, copied);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private static object CopyListData(object srcList)
    {
      if (srcList == null) { return null; }

      var src = srcList as IList;
      if (src == null)
      {
        throw new InvalidOperationException("The source is not a list, or supported list type (IList)!");
      }

      IList res = (IList)Activator.CreateInstance(src.GetType());

      foreach (var i in src)
      {
        object itemVal = CreateCopy(i);
        res.Add(itemVal);
      }

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private static void CopyListDataEx<TFrom, TTo>(TFrom from, TTo to, DataMember fromMember, DataMember toMember, Type listType1, Type listType2)
    {
      IList sourceList = (IList)fromMember.GetValue(from);
      IList target = (IList)toMember.GetValue(to);
      if (target == null)
      {
        if (toMember.CanWrite)
        {
          target = (IList)Activator.CreateInstance(toMember.DataType, null);
          toMember.SetValue(to, target);
        }
      }
      else
      {
        target.Clear();
      }

      if (sourceList != null)
      {
        if (ReflectionTools.IsSimpleType(listType1))
        {
          foreach (var item in sourceList)
          {
            target.Add(item);
          }
        }
        else
        {
          foreach (var item in sourceList)
          {
            object nextItem = DTOMapper.CreateCopy(listType1, listType2, item);
            target.Add(nextItem);
          }
        }
      }
      else
      {
        // We can't directly write to the list (which is how we got here) so we will just clear it out instead.
        if (target != null) { target.Clear(); }
      }

    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Determines if both property types are IList instances of the same type.
    /// </summary>
    // TODO: Make a similar copy of this for ReflectionTools.
    private static bool AreCompatibleLists(DataMember fromMember, DataMember toMember, out Type listType1, out Type listType2)
    {
      listType1 = null;
      listType2 = null;

      if (ReflectionTools.HasInterface<IList>(fromMember.DataType) &&
          ReflectionTools.HasInterface<IList>(toMember.DataType))
      {

        // Make sure they are both composite or simple..
#if NETFX_CORE
        listType1 = fromMember.DataType.GetTypeInfo().GenericTypeArguments[0];
        listType2 = toMember.DataType.GetTypeInfo().GenericTypeArguments[0];
#else
        listType1 = fromMember.DataType.GetGenericArguments()[0];
        listType2 = toMember.DataType.GetGenericArguments()[0];
#endif

        bool simple1 = ReflectionTools.IsSimpleType(listType1);
        bool simple2 = ReflectionTools.IsSimpleType(listType2);

        if (simple1 && (simple1 == simple2))
        {
          bool res = listType1 == listType2;
          return res;
        }
        else
        {
          // One of these is simple, and the other composite....
          return !simple2;
        }
      }

      // They aren't both list types...
      return false;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// When sending data across the internet, it is a good idea to cleanup your strings to prevent XSS and injection attacks.
    /// This function will find all of the public string properties on a type, and make sure that they are clean for sending.
    /// </summary>
    public static void CleanInputStrings<T>(T target)
    {
      var stringProps = from x in ReflectionTools.GetProperties<T>()
                        where x.PropertyType == typeof(string)
                        select x;


      foreach (var prop in stringProps)
      {
#if NET_40
        string baseVal = (string)prop.GetValue(target, null);
        string cleaned = CleanInputString(baseVal);
        prop.SetValue(target, cleaned, null);
#else
        string baseVal = (string)prop.GetValue(target);
        string cleaned = CleanInputString(baseVal);
        prop.SetValue(target, cleaned);
#endif
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    // TODO: Maybe this should go live with the string tools ?
    private static string CleanInputString(string input)
    {
      if (string.IsNullOrEmpty(input)) { return input; }

      // NOTE: The fastest way would probably be to analyze each character, and replace accordingly.
#if NET_40
      string res = StringTools.EscapeXml(input);
      res = StringTools.EscapeString(res);
#else
      string res = StringTools.EscapeXml(input);
      res = StringTools.EscapeString(res);
#endif

      // TODO: Others ??

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static object CreateCopy(Type fromType, Type toType, object from)
    {
      var matches = ReflectionTools.GetMethods(typeof(DTOMapper)).Where(x => x.IsStatic &&
                                                                             x.IsPublic &&
                                                                             x.Name == "CreateCopy" &&
                                                                             x.ContainsGenericParameters &&
                                                                             x.GetGenericArguments().Length == 2).ToList();
      MethodInfo m = matches.Single();

      m = m.MakeGenericMethod(fromType, toType);
      return m.Invoke(null, new object[] { from });

    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static object CreateCopy(Type type, object from)
    {
      if (from == null) { return null; }

      // Just make a copy of any struct types...  
#if NETFX_CORE
      if (type.GetTypeInfo().IsValueType)
#else
      if (type.IsValueType)
#endif
      {
        return from;
      }

      // TODO: Some kind of IL emitter to speed this up would be really cool!
      var matches = ReflectionTools.GetMethods(typeof(DTOMapper)).Where(x => x.IsStatic &&
                                                                             x.IsPublic &&
                                                                             x.Name == "CreateCopy" &&
                                                                             x.ContainsGenericParameters &&
                                                                             x.GetGenericArguments().Length == 1).ToList();
      MethodInfo m = matches.Single();

      // NOTE: This is kind of a waste, maybe.  Why check for interfaces when we can always just grab
      // the type directly from 'from'
      if (type.IsInterface)
      {
        type = from.GetType();
      }
      m = m.MakeGenericMethod(type);
      return m.Invoke(null, new object[] { from });
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static object CreateCopy(object source)
    {
      if (source == null) { return null; }

      Type srcType = source.GetType();
      if (ReflectionTools.IsSimpleType(srcType))
      {
        return source;
      }
      else
      {
        return CreateCopy(srcType, source);
      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public static T CreateCopy<T>(T source) where T : new()
    {
      T res = Activator.CreateInstance<T>();
      DTOMapper.CopyMembers(source, res);
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Creates a copy of TTo, cloning compatible properties from <param name="from"/>
    /// </summary>
    public static TTo CreateCopy<TFrom, TTo>(TFrom from) where TTo : new()
    {
      TTo res = Activator.CreateInstance<TTo>();
      CopyMembers(from, res);
      return res;
    }
  }


  // ============================================================================================================================
  /// <summary>
  /// Represents a data member, either a property or a field.
  /// This is abstract member SET/GET during DTOMapper copying.
  /// </summary>
  class DataMember
  {

    private PropertyInfo PropInfo = null;
    private FieldInfo FieldInfo = null;

    // --------------------------------------------------------------------------------------------------------------------------
    public DataMember(PropertyInfo propInfo)
    {
      Name = propInfo.Name;
      MemberType = EDataMemberType.Property;
      PropInfo = propInfo;
      DataType = PropInfo.PropertyType;
      CanWrite = PropInfo.CanWrite;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public DataMember(FieldInfo fieldInfo)
    {
      Name = fieldInfo.Name;
      MemberType = EDataMemberType.Field;
      FieldInfo = fieldInfo;
      DataType = FieldInfo.FieldType;
      CanWrite = !FieldInfo.IsInitOnly && !FieldInfo.IsLiteral;
    }


    public bool CanWrite { get; private set; }

    public string Name { get; private set; }
    public EDataMemberType MemberType { get; private set; }
    public Type DataType { get; private set; }

    // --------------------------------------------------------------------------------------------------------------------------
    public object GetValue(object from)
    {
      switch (MemberType)
      {
        case EDataMemberType.Property:
          return PropInfo.GetValue(from, null);
        case EDataMemberType.Field:
          return FieldInfo.GetValue(from);
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void SetValue(object target, object value)
    {
      switch (MemberType)
      {
        case EDataMemberType.Property:
          PropInfo.SetValue(target, value, null);
          break;
        case EDataMemberType.Field:
          FieldInfo.SetValue(target, value);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

    }


  }


  // ============================================================================================================================
  enum EDataMemberType
  {
    Invalid = 0,
    Property,
    Field
  }

}
