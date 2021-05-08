// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright ©2012-2018 Andrew A. Ritz, all rights reserved.
// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using System.Xml.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Xml;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace drewCo.Tools
{

  // ============================================================================================================================
  /// <summary>
  /// Filled with many helpful reflection based operations and manipulations.
  /// </summary>
  public partial class ReflectionTools
  {


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Create an instance of the given type, using the default constructor.  
    /// Non public constructors will also be invoked.
    /// </summary>
    public static T CreateInstance<T>()
    {
      return (T)CreateInstance(typeof(T));
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Create an instance of the given type, using the default constructor.  
    /// Non public constructors will also be invoked.
    /// </summary>
    public static object CreateInstance(Type t)
    {
      object res = null;

#if NETFX_CORE
      try
      {
        if (t.GetTypeInfo().IsValueType)
        {
          res = Activator.CreateInstance(t);
        }
        else
        {
          var cTors = t.GetTypeInfo().DeclaredConstructors;
          var defaultCtor = (from x in cTors
                             where x.GetParameters().Count() == 0
                             select x).SingleOrDefault();
          if (defaultCtor == null)
          {
            throw new MissingMemberException("can't find the default (non public?) constructor.");
          }
          res = defaultCtor.Invoke(null);
        }

      }
      catch (MissingMemberException mmex)
#else
      try
      {
        res = Activator.CreateInstance(t, true);
      }
      catch (MissingMethodException mmex)
#endif
      {
        string errMsg = string.Format("The type '{0}' does not have a public constructor, and can't be deserialized!", t.Name);
        throw new Exception(errMsg, mmex);
      }

      return res;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Tells if the given type (<paramref name="t"/>) is a subclass of <paramref name="parentType"/>
    /// </summary>
    public static bool IsSubclassOf(Type t, Type parentType)
    {
#if NETFX_CORE
      bool res = t.GetTypeInfo().IsSubclassOf(parentType);
#else
      bool res = t.IsSubclassOf(parentType);
#endif
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static ConstructorInfo GetCtor(Type t, params Type[] paramTypes)
    {
#if NETFX_CORE
      int ptLen = paramTypes.Length;


      IEnumerable<ConstructorInfo> ctors = t.GetTypeInfo().DeclaredConstructors;

      foreach (var res in ctors)
      {
        Type[] pTypes = (from x in res.GetParameters()
                         select x.ParameterType).ToArray();
        int thisPtLen = pTypes.Length;

        if (thisPtLen == ptLen)
        {
          // They possibly match!
          bool isMatch = true;
          for (int i = 0; i < ptLen; i++)
          {
            if (pTypes[i] != paramTypes[i])
            {
              isMatch = false;
              break;
            }
          }
          if (!isMatch) { break; }

          // We found a match for the types!
          ConstructorInfo match = res;
          return res;

        }
      }

      // Nothing found.....
      throw new InvalidOperationException("Could not find a constructor with the given parameters!");
#else
      ConstructorInfo res = t.GetConstructor(paramTypes);
      return res;
#endif
    }


#if NETFX_CORE
    // --------------------------------------------------------------------------------------------------------------------------
    public static MethodInfo GetMethod(Type t, string name, EMemberScope mScope)
    {
      MethodInfo res = null;

      List<MethodInfo> methods = t.GetTypeInfo().DeclaredMethods.ToList();
      int len = methods.Count;

      for (int i = 0; i < len; i++)
      {
        MethodInfo m = methods[i];
        if (m.Name != name) { continue; }

        if ((mScope & EMemberScope.Public) == EMemberScope.Public && !m.IsPublic) { continue; }
        if ((mScope & EMemberScope.Private) == EMemberScope.Private && !m.IsPrivate) { continue; }
        if ((mScope & EMemberScope.Static) == EMemberScope.Static && !m.IsStatic) { continue; }

        //if (m.IsPrivate && (mScope & EMemberScope.Private) != EMemberScope.Private) { continue; }
        //if (m.IsStatic && (mScope & EMemberScope.Static) != EMemberScope.Static) { continue; }



        res = m;
        break;
      }

      return res;
    }
#endif


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets the method with the given name on the given type.
    /// Throws an exception if there is more than one method with that name.
    /// </summary>
    /// <returns>The matching method, or null if none exists.</returns>
    public static MethodInfo GetMethod(Type t, string name)
    {
#if NETFX_CORE

      var res = t.GetTypeInfo().GetDeclaredMethod(name);
      return res;

      //IEnumerable<MethodInfo> methods = ;

      //var methodsWithName = (from x in methods where x.Name == name select x);
      //int count = methodsWithName.Count();
      //if (count > 1)
      //{
      //  throw new AmbiguousMatchException(string.Format("There are {0} methods on the type '{1}' with the name '{2}'!", count, t, name));
      //}

      //MethodInfo res = methods.FirstOrDefault();
      //return res;
#else
      // NOTE: The WinRT version will find private + static versions, and this is only looking at public stuff.
      MethodInfo res = t.GetMethod(name);
      return res;
#endif
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets the method with the given name, and argument types.
    /// </summary>
    public static MethodInfo GetMethod(Type t, string name, Type[] argTypes)
    {
#if NETFX_CORE

      MethodInfo res = null;
      IEnumerable<MethodInfo> methods = t.GetTypeInfo().DeclaredMethods;

      foreach (var m in methods)
      {
        if (m.Name != name) { continue; }

        Type[] paramTypes = (from x in m.GetParameters() select x.ParameterType).ToArray();
        if (paramTypes.Length != argTypes.Length) { continue; }

        int len = argTypes.Length;
        bool allMatch = true;
        for (int i = 0; i < len; i++)
        {
          if (paramTypes[i] != argTypes[i])
          {
            allMatch = false;
            break;
          }
        }

        if (allMatch)
        {
          res = m;
          break;
        }

      }

      return res;
#else
      MethodInfo res = t.GetMethod(name, argTypes);
      return res;
#endif
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static MethodInfo[] GetMethods(Type t)
    {

#if NETFX_CORE
      MethodInfo[] res = t.GetTypeInfo().DeclaredMethods.ToArray();
      return res;
#else
      var res = t.GetMethods();
      return res;
#endif

    }



    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Get all of the types defined in the given type's assembly.
    /// </summary>
    public static Type[] GetAssemblyTypes<TType>()
    {
#if NETFX_CORE

      IEnumerable<TypeInfo> infos = typeof(TType).GetTypeInfo().Assembly.DefinedTypes;
      Type[] res = (from x in infos
                    select x.AsType()).ToArray();
      return res;
#else
      Type[] res = typeof(TType).Assembly.GetTypes();
      return res;

#endif
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public static bool IsNumeric(object o)
    {
      return IsNumericType(o.GetType());
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static bool IsNullable(Type type)
    {
      return type.Name.StartsWith("Nullable`1");
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Given the set of input arguments, this will adjust them in terms of the given method.
    /// Basically, this will find missing optional args, and fill them in as needed.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="toFollow"></param>
    public static object[] FixOptionalArgs(object[] args, MethodInfo toFollow)
    {

      ParameterInfo[] pInfo = toFollow.GetParameters();
      int srcLength = args.Length;
      int pLength = pInfo.Length;

      object[] res = new object[pLength];

      for (int i = 0; i < pLength; i++)
      {
        if (i > srcLength - 1)
        {
          if (pInfo[i].IsOptional)
          {
            res[i] = pInfo[i].DefaultValue;
          }
          else
          {
            string errMsg = string.Format("Missing required argument at index {0}!", i);
            throw new InvalidOperationException(errMsg);
          }
        }
        else
        {
          // NOTE: We aren't going to do any type checking at this time.
          res[i] = args[i];
        }
      }

      return res;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public static string GetMemberName<TSource>(Expression<Func<TSource, object>> expression)
    {
      PropertyInfo info = GetPropertyInfo<TSource>(expression);
      if (info != null) { return info.Name; }

      FieldInfo fInfo = GetFieldInfo<TSource>(expression);
      if (fInfo != null) { return fInfo.Name; }

      throw new NotImplementedException("I don't know what to do here!");
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets the name of the property, as determined by its expression.
    /// </summary>
    public static string GetPropertyName<TSource>(Expression<Func<TSource, object>> expression)
    {
      PropertyInfo info = GetPropertyInfo<TSource>(expression);
      return info.Name;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets the name of the property listed in the expression.
    /// Nesting is ignored, and only the final name will appear, i.e. -> loc.Person.Name.Middle = "Middle"
    /// </summary>
    public static string GetPropertyName(Expression propExp)
    {
      PropertyInfo info = GetPropertyInfo(propExp);
      return info == null ? null : info.Name;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets reflection information about the given property.
    /// </summary>
    public static PropertyInfo GetPropertyInfo<TSource>(Expression<Func<TSource, object>> expression)
    {

      var lambda = expression as LambdaExpression;
      MemberExpression memberExpression = ResolveMemberExpression(lambda);

      if (memberExpression == null)
      {
        throw new Exception("Please provide a lambda expression like 'n => n.PropertyName'");
      }

      var propertyInfo = memberExpression.Member as PropertyInfo;
      return propertyInfo;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private static MemberExpression ResolveMemberExpression(LambdaExpression lambda)
    {
      if (lambda.Body is UnaryExpression)
      {
        var unaryExpression = lambda.Body as UnaryExpression;
        return unaryExpression.Operand as MemberExpression;
      }
      else
      {
        return lambda.Body as MemberExpression;
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Attempts to resolve the field given the type and field name.
    /// Returns null if the field in question can't be found.
    /// _TEST: We need to make sure that this will locate private members.
    /// </summary>
    public static FieldInfo GetFieldInfo(Type srcType, string fieldName, bool includeNonPublic = true)
    {
#if !NETFX_CORE
      FieldInfo field = srcType.GetField(fieldName);
#else
      FieldInfo field = srcType.GetRuntimeField(fieldName);
#endif
      if (field == null && includeNonPublic)
      {
#if !NETFX_CORE
        field = srcType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
#else
        var allFields = srcType.GetRuntimeFields().ToList();
        field = (from x in allFields
                 where x.Name == fieldName
                 select x).SingleOrDefault();
#endif

      }
      return field;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets reflection information about the given field.
    /// </summary>
    public static FieldInfo GetFieldInfo<TSource>(Expression<Func<TSource, object>> expression)
    {

      var lambda = expression as LambdaExpression;
      MemberExpression memberExpression;
      if (lambda.Body is UnaryExpression)
      {
        var unaryExpression = lambda.Body as UnaryExpression;
        memberExpression = unaryExpression.Operand as MemberExpression;
      }
      else
      {
        memberExpression = lambda.Body as MemberExpression;
      }

      if (memberExpression == null)
      {
        throw new Exception("Please provide a lambda expression like 'n => n.PropertyName'");
      }

      var fieldInfo = memberExpression.Member as FieldInfo;
      return fieldInfo;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Converts the given input object into the given type.
    /// </summary>
    /// <remarks>
    /// Only widening and 'ToString' operations are available at this time.
    /// </remarks>
    [Obsolete("Use ConvertEx!")]
    public static object Convert(Type cType, object input)
    {
      if (input == null)
      {

#if NETFX_CORE
        if (cType.GetTypeInfo().IsValueType) { throw new NotSupportedException(); }
#else
        if (cType.IsValueType) { throw new NotSupportedException(); }
#endif
        return null;
      }

      if (cType == typeof(string)) { return input.ToString(); }

      // TODO: We can use that code to determine widening conversion to exit early...
      if (input.GetType() == cType) { return input; }

      if (input is string)
      {
        return Convert(cType, (string)input);
      }

      if (input is byte)
      {
        if (cType == typeof(int)) { return (int)(byte)input; }
        if (cType == typeof(long)) { return (long)(byte)input; }
        if (cType == typeof(decimal)) { return (decimal)(byte)input; }
        if (cType == typeof(float)) { return (float)(byte)input; }
        if (cType == typeof(double)) { return (double)(byte)input; }
      }

      if (input is int)
      {
        if (cType == typeof(long)) { return (long)(int)input; }
        if (cType == typeof(decimal)) { return (decimal)(int)input; }
        if (cType == typeof(float)) { return (float)(int)input; }
        if (cType == typeof(double)) { return (double)(int)input; }
      }

      if (input is long)
      {
        if (cType == typeof(decimal)) { return (decimal)(long)input; }
        if (cType == typeof(float)) { return (float)(long)input; }
        if (cType == typeof(double)) { return (double)(long)input; }
      }

      if (input is decimal)
      {
        if (cType == typeof(float)) { return (float)(decimal)input; }
        if (cType == typeof(double)) { return (double)(decimal)input; }
      }

      if (input is float)
      {
        if (cType == typeof(double)) { return (double)(float)input; }
      }

      throw GetConvertException(input.GetType(), cType);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    // NOTE: This is almost exactly like the function that actually does the conversion.
    // We should find a clever way, via overloads, to combine the functionality of the two!
    public static bool CanConvert(Type cType, string input)
    {
      if (cType == typeof(string)) { return true; }
      if (cType == typeof(bool))
      {
        bool val = false;
        return bool.TryParse(input, out val);
      }

      if (cType == typeof(int))
      {
        int val = 0;
        return int.TryParse(input, out val);
      }

      if (cType == typeof(float))
      {
        float val = 0;
        return float.TryParse(input, out val);
      }

      if (cType == typeof(double))
      {
        double val = 0;
        return double.TryParse(input, out val);
      }


      // TODO: Merge this with main lib.
      if (cType == typeof(Int64))
      {
        Int64 val = 0;
        return Int64.TryParse(input, out val);
      }

      throw new Exception(string.Format("Can't evaluate type '{0}' for possible conversion.", cType));
    }


    // --------------------------------------------------------------------------------------------------------------------------
    private static Exception GetConvertException(Type fromType, Type toType)
    {
      return new Exception(string.Format("Could not from type'{0}' to '{1}'", fromType, toType));
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Converts the given string input into the target return type.
    /// TODO: Should we just use the object based version ??  I think that might be a good idea.
    /// </summary>
    public static T Convert<T>(string input)
    {
      return (T)Convert(typeof(T), input);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private static void ThrowConvertException(string input, Type cType)
    {
      string err = string.Format("Could not convert the input value '{0}' to type '{1}'", input, cType);
      throw new InvalidOperationException(err);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static MemberExpression GetMemberExpression(Expression propExp)
    {
      LambdaExpression lambda = propExp as LambdaExpression;
      MemberExpression memberExpression;
      if (lambda.Body is UnaryExpression)
      {
        var unaryExpression = lambda.Body as UnaryExpression;
        memberExpression = unaryExpression.Operand as MemberExpression;
      }
      else
      {
        memberExpression = lambda.Body as MemberExpression;
      }
      Debug.Assert(memberExpression != null, "Please provide a lambda expression like 'n => n.PropertyName'");

      return memberExpression;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Get a PropertyInfo object assocaited with the given general expression.
    /// </summary>
    public static PropertyInfo GetPropertyInfo(Expression propExp)
    {
      MemberExpression memberExp = GetMemberExpression(propExp);
      return GetPropertyInfo(memberExp);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Get a PropertyInfo object assicted with the given member expression.
    /// </summary>
    public static PropertyInfo GetPropertyInfo(MemberExpression memberExp)
    {
      if (memberExp != null)
      {
        var propertyInfo = memberExp.Member as PropertyInfo;

        return propertyInfo;
      }
      return null;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public static object GetPropertyValue<TSource>(TSource target, Expression<Func<TSource, object>> propExp)
    {
      PropertyInfo info = GetPropertyInfo<TSource>(propExp);
#if NETFX_CORE
      return info.GetMethod.Invoke(target, null);
#else
      return info.GetGetMethod().Invoke(target, null);
#endif
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns the current value of the given property on the target.
    /// </summary>
    public static object GetPropertyValue(object target, Expression memberExp)
    {
      PropertyInfo info = GetPropertyInfo(memberExp);
#if NETFX_CORE
      return info.GetMethod.Invoke(target, null);
#else
      return info.GetGetMethod().Invoke(target, null);
#endif
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This will tell us if we have a numeric type or not.
    /// </summary>
    public static bool IsNumericType(Type t)
    {
      return t == typeof(sbyte) ||
             t == typeof(byte) ||
             t == typeof(short) ||
             t == typeof(ushort) ||
             t == typeof(int) ||
             t == typeof(uint) ||
             t == typeof(long) ||
             t == typeof(ulong) ||
             t == typeof(float) ||
             t == typeof(double) ||
             t == typeof(decimal);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets the name of the property listed in the expression.
    /// Nesting is preserved so the entire path will appear, i.e. -> loc.Person.Name.Middle = "Person.Name.Middle"
    /// </summary>
    public static string GetNestedPropertyName(Expression propExp)
    {
      MemberExpression memberExp = ReflectionTools.GetMemberExpression(propExp);
      string memberString = memberExp.ToString();
      return memberString.Substring(memberString.IndexOf('.') + 1);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns the current value of the given property on the target.
    /// This is slower than finding simple properties, but will work for simple / nested cases.
    /// </summary>
    public static object GetNestedPropertyValue(object source, Expression memberExp)
    {

      // Yes, all of this will work for a nested property expression, but maybe we should search for a faster way ?
      LambdaExpression lambda = memberExp as LambdaExpression;
      MemberExpression memberExpression = ResolveMemberExpression(lambda);

      string name = memberExpression.ToString().Replace(lambda.Parameters[0] + ".", string.Empty);
      string[] nameParts = name.Split('.');
      if (nameParts.Length == 1)
      {
        PropertyInfo info = GetPropertyInfo(memberExp);
#if NETFX_CORE
        return info.GetMethod.Invoke(source, null);
#else
        return info.GetGetMethod().Invoke(source, null);
#endif
      }

      foreach (var part in name.Split('.'))
      {
        if (source == null) { return null; }

        Type type = source.GetType();

#if NETFX_CORE
        PropertyInfo info = type.GetRuntimeProperty(part);
#else
        PropertyInfo info = type.GetProperty(part);
#endif

        if (info == null) { return null; }
        source = info.GetValue(source, null);
      }

      return source;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static PropertyInfo GetPropertyInfo(Type sourceType, string propertyPath)
    {
      return GetPropertyInfo(sourceType, propertyPath, false, true);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets the property info from the <paramref name="sourceType"/> using a string based property path.
    /// </summary>
    public static PropertyInfo GetPropertyInfo(Type sourceType, string propertyPath, bool includeNonPublic, bool throwOnNull)
    {
      Type searchType = sourceType;
      PropertyInfo res = null;
      string[] pathParts = propertyPath.Split('.');
      foreach (var part in pathParts)
      {
#if !NETFX_CORE
        res = searchType.GetProperty(part);
        if (res == null & includeNonPublic)
        {
          res = searchType.GetProperty(part, BindingFlags.Instance | BindingFlags.NonPublic);
        }
#else

        res = (from x in searchType.GetRuntimeProperties()
               where x.Name == part
               select x).SingleOrDefault();

        //if (!includeNonPublic)
        //{
        //  res = searchType.GetRuntimeProperty(part);
        //}
        //else
        //{
        //  var allProps = searchType.GetRuntimeProperties().ToList();
        //  res = (from x in allProps
        //         where x.Name == part
        //         select x).SingleOrDefault();
        //}
#endif
        if (res == null)
        {
          if (throwOnNull)
          {
            string errMsg = "Could not find the property '{0}' on source type '{1}' using path '{2}'!";
            throw new InvalidOperationException(string.Format(errMsg, part, sourceType, propertyPath));
          }
          return null;
        }
        searchType = res.PropertyType;
      }

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Attempts to resolve the property given the type and property name.
    /// Returns null if the property in question can't be found.
    /// _TEST: We need to make sure that this will locate private members.
    /// </summary>
    public static PropertyInfo GetPropertyInfo(Type srcType, string propName, bool includeNonPublic)
    {
#if !NETFX_CORE
      PropertyInfo prop = srcType.GetProperty(propName);
#else
      PropertyInfo prop = srcType.GetRuntimeProperty(propName);
#endif
      if (prop == null && includeNonPublic)
      {
#if !NETFX_CORE
        prop = srcType.GetProperty(propName, BindingFlags.NonPublic | BindingFlags.Instance);
#else
        // _TEST: This may not actually resolve non-publics....
        prop = srcType.GetRuntimeProperty(propName);
#endif

      }
      return prop;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static PropertyInfo GetPropertyInfo<TSource>(string propertyPath)
    {
      return GetPropertyInfo(typeof(TSource), propertyPath, false, true);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private static PropertyInfo GetNestedPropertyInfo(Type type, string[] pathParts, int index)
    {
      PropertyInfo prop = GetPropertyInfo(type, pathParts[index]);
      if (prop == null) { return null; }

      if (index == pathParts.Length - 1)
      {
        return prop;
      }

      return GetNestedPropertyInfo(prop.PropertyType, pathParts, index + 1);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static PropertyInfo GetNestedPropertyInfo<T>(string propPath)
    {
      return GetNestedPropertyInfo(typeof(T), propPath);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static PropertyInfo GetNestedPropertyInfo(Type srcType, string propPath)
    {
      string[] memberParts = propPath.Split(new[] { '.' });
      return GetNestedPropertyInfo(srcType, memberParts, 0);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void SetNestedPropertyValue(string propPath, object target, object value)
    {
      string[] pathParts = propPath.Split(new[] { '.' });
      SetNestedPropertyValue(target.GetType(), pathParts, 0, target, value);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private static void SetNestedPropertyValue(Type type, string[] pathParts, int index, object target, object value)
    {

      PropertyInfo prop = GetPropertyInfo(type, pathParts[index]);
      if (prop == null)
      {
        string path = string.Join(".", pathParts);
        string msg = string.Format("The property path {0} for type {1} is not valid!", path, type);
        throw new InvalidOperationException(msg);
      }

      if (index == pathParts.Length - 1)
      {
        prop.SetValue(target, value, null);
        return;
      }

      // Get the next 'target' value, and move down the line.
      object newTarget = prop.GetValue(target, null);
      SetNestedPropertyValue(prop.PropertyType, pathParts, index + 1, newTarget, value);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Tells us if the given type is the base type, or derived from it.
    /// </summary>
    public static bool IsOfBaseType(Type t, Type baseType)
    {
      if (t == baseType) { return true; }

#if !NETFX_CORE
      while (t.BaseType != null)
      {
        if (t.BaseType == baseType) { return true; }
        t = t.BaseType;
      }
#else

      TypeInfo ti = t.GetTypeInfo();
      while (ti.BaseType != null)
      {
        if (ti.BaseType == baseType) { return true; }
        ti = ti.BaseType.GetTypeInfo();
      }

#endif

      return false;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Resovles an assembly from the given name.  The name can be simple, ala: "MyLib" or fully qualified.
    /// </summary>
    public static Assembly ResolveAssembly(string name)
    {

      Assembly[] src = AppDomain.CurrentDomain.GetAssemblies();
      foreach (var item in src)
      {
        if (item.FullName.IndexOf(name, StringComparison.OrdinalIgnoreCase) != -1)
        {
          return item;
        }
      }

      return null;

    }


    private static Dictionary<string, Type> ResolvedTypeCache = new Dictionary<string, Type>();
    private static List<Assembly> DomainAssemblies = null; //new List<Assembly>();

    // WinRT assemblies that don't always load by default.  We include them to make sure that we can resolve various collection
    // types.
    //private static string[] WinRTAsmNames = new[]
    //{
    //  "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
    //  "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
    //};

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Resolves a type from the current app domain based on the name.
    /// </summary>
    /// <remarks>
    /// This function can be very slow as it will search assemblies in the domain in sequential order.
    /// Searches are case sensitive.
    /// Namespaces aren't taken into consideration.  If there is a conflicting name, the first name will be returned.
    /// </remarks>
    public static Type ResolveType(string typeName, bool ignoreTypeLoadExceptions = true, bool throwOnNull = false)
    {
      typeName = typeName.Trim();

      Type resolved = null;
      if (ResolvedTypeCache.TryGetValue(typeName, out resolved)) { return resolved; }

#if !NETFX_CORE
      ResolveDomainAssemblies();
#endif

      Type match = Type.GetType(typeName);
      if (match != null)
      {
        ResolvedTypeCache.Add(typeName, match);
        return match;
      }

      if (DomainAssemblies == null)
      {
        throw new InvalidOperationException($"No domain assemblies are defined!  The type {typeName} can't be resolved!");
      }
      int asmCount = DomainAssemblies.Count;
      for (int i = 0; i < asmCount; i++)
      {
        Assembly asm = DomainAssemblies[i];

        try
        {
#if !NETFX_CORE
          Type[] types = asm.GetTypes();
#else
          Type[] types = asm.ExportedTypes.ToArray();
#endif
          int typeCount = types.Length;

          for (int j = 0; j < typeCount; j++)
          {
            Type t = types[j];
            if (t.Name == typeName || t.FullName == typeName)
            {
              ResolvedTypeCache.Add(typeName, t);
              return t;
            }
          }
        }
        catch (ReflectionTypeLoadException)
        {
          if (!ignoreTypeLoadExceptions)
          {
            throw;
          }
        }
      }

      if (throwOnNull)
      {
        throw new InvalidOperationException(string.Format("Could not find a type with name '{0}'!  Have you called ReflectionTools.ResolveDomainAssemblies?", typeName));
      }

      return null;
    }

    // --------------------------------------------------------------------------------------------------------------------------
#if NETFX_CORE
    [Obsolete("Please add domain assemblies one at a time.  This function will be removed in a future update!")]
    public static async Task ResolveDomainAssemblies()
#else
    public static void ResolveDomainAssemblies()
#endif
    {
      if (DomainAssemblies == null)
      {
        AppDomain cDomain = AppDomain.CurrentDomain;

#if NETFX_CORE
        DomainAssemblies = await cDomain.GetAssemblyListAsync();
#else
        DomainAssemblies = cDomain.GetAssemblies().ToList();
#endif

#if NETFX_CORE
        // This is a hack to work around the many types (collection types) that may not be loaded via assembly.
        var asm1 = typeof(SortedDictionary<,>).GetTypeInfo().Assembly;
        var asm2 = typeof(HashSet<>).GetTypeInfo().Assembly;

        DomainAssemblies.Add(asm1);
        DomainAssemblies.Add(asm2);
#endif

        //foreach (var name in WinRTAsmNames)
        //{
        //  if (!DomainAssemblies.Any(x => x.FullName == name))
        //  {
        //    var a = Assembly.Load(new AssemblyName(name));
        //    DomainAssemblies.Add(a);
        //  }
        //}

      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Add an assembly that the type resolver can use to search when resolving types.
    /// </summary>
    /// <param name="asm"></param>
    public static void AddDomainAssembly(Assembly asm)
    {
      if (DomainAssemblies == null) { DomainAssemblies = new List<Assembly>(); }
      DomainAssemblies.Add(asm);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Add a lookup entry for the type resolver.
    /// </summary>
    public static void AddTypeLookup(string name, Type t)
    {
      ResolvedTypeCache.Add(name, t);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void ClearResolvedTypeCache()
    {
      ResolvedTypeCache.Clear();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static MethodInfo ResolveMethod<T>(string name, Type[] types)
    {
      return ResolveMethod(typeof(T), name, types);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static MethodInfo ResolveMethod(Type t, string name, Type[] types, bool includeNonPublic = false)
    {

#if !NETFX_CORE
      MethodInfo exactMatch = t.GetMethod(name, types);
#else
      MethodInfo exactMatch = t.GetRuntimeMethod(name, types);
#endif
      if (exactMatch != null) { return exactMatch; }


#if !NETFX_CORE
      IEnumerable<MethodInfo> src = t.GetMethods();
      if (includeNonPublic)
      {
        BindingFlags nonPublicFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.NonPublic;
        src = src.Concat(t.GetMethods(nonPublicFlags));
      }
#else
      IEnumerable<MethodInfo> src = t.GetRuntimeMethods();
      if (!includeNonPublic)
      {
        src = from x in src
              where x.IsPublic
              select x;
      }
#endif

      MethodInfo[] byName = (from x in src
                             where x.Name == name
                             select x).ToArray();

      if (byName.Length == 0) { throw new InvalidOperationException(string.Format("Could not find a method named {0}", name)); }
      if (byName.Length == 1 && types.Length == 0)
      {
        return byName.First();
      }


      int tIndex = 0;
      while (true)
      {
        // We need to match the listed types, sort of one by one.
        Type cTYpe = tIndex > types.Length - 1 ? null : types[tIndex];


        List<MethodInfo> match = new List<MethodInfo>();
        foreach (var m in byName)
        {
          ParameterInfo[] param = m.GetParameters();
          if (param.Length < tIndex) { continue; }


          ParameterInfo p = param[tIndex];
          if ((cTYpe == null && p.IsOptional) || (param[tIndex].ParameterType == cTYpe))
          {
            match.Add(m);
          }
        }

        if (match.Count == 0) { throw new InvalidOperationException(string.Format("Could not find a method named {0} with the matching types!", name)); }
        if (match.Count == 1) { return match[0]; }

        byName = match.ToArray();
        tIndex++;
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns all of the properties + fields as 'TypeMember' instances.
    /// </summary>
    public static List<TypeMember> GetAllTypeMembers(Type target, bool includenonPublic = false)
    {
      List<TypeMember> res = new List<TypeMember>();

#if !NETFX_CORE
      BindingFlags flags = BindingFlags.Instance | (includenonPublic ? BindingFlags.NonPublic : BindingFlags.Public);
      var allProps = target.GetProperties(flags);
      var allFields = target.GetFields(flags);
#else

      var allProps = target.GetRuntimeProperties();
      var allFields = target.GetRuntimeFields();

      if (!includenonPublic)
      {
        throw new NotSupportedException("Don't know how to filter private properties in WinRT!");
        allFields = from x in allFields
                    where x.IsPublic
                    select x;
      }

#endif
      foreach (var item in allProps)
      {
        res.Add(new TypeMember(item));
      }

      foreach (var item in allFields)
      {
        res.Add(new TypeMember(item));
      }
      return res;

    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns an instance of <see cref="TypeMember"/> that is defined on the given type, with matching name.
    /// </summary>
    /// <returns>Null if no match can be found.</returns>
    public static TypeMember GetTypeMember(Type target, string memberName, bool includeNonPublic = false)
    {
      PropertyInfo prop = ReflectionTools.GetPropertyInfo(target, memberName, includeNonPublic);
      if (prop != null) { return new TypeMember(prop); }

      FieldInfo field = ReflectionTools.GetFieldInfo(target, memberName, includeNonPublic);
      if (field != null) { return new TypeMember(field); }

      // Nothing found....
      return null;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Resolves both the getter and setter for the given property.
    /// Throws an exception if both items can't be found!
    /// </summary>
    public static void ResolveSetterAndGetter(PropertyInfo prop, out MethodInfo getter, out MethodInfo setter, bool throwOnNullSetter = true)
    {
#if !NETFX_CORE
      getter = prop.GetGetMethod();
      setter = prop.GetSetMethod();
#else
      getter = prop.GetMethod;
      setter = prop.SetMethod;
#endif

#if !NETFX_CORE
      if (setter == null)
      {
        setter = prop.GetSetMethod(true);
      }
#endif

      if (setter == null && throwOnNullSetter)
      {
        throw new Exception(string.Format("Could not locate a setter for the property '{0}'", prop.Name));
      }
    }


#if !NETFX_CORE

    // --------------------------------------------------------------------------------------------------------------------------
    public static TAttr GetAttribute<TAttr>(PropertyInfo prop)
    {
      var all = prop.GetCustomAttributes(typeof(TAttr), false);
      TAttr res = (TAttr)all.SingleOrDefault();
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static TAttr GetAttribute<TAttr>(Type t) where TAttr : Attribute
    {
      var res = t.GetCustomAttributes(typeof(TAttr), false).SingleOrDefault() as TAttr;
      return res;
    }

    // TODO: Take the two functions defined here and tupleize them or something. (GetAttributes)
    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Finds and returns all attributes on all of the given types in an assembly.
    /// </summary>
    public static IEnumerable<TAttr> GetAttributes<TAttr>(Assembly asm = null)
      where TAttr : Attribute
    {

      //    if (asm == null) { throw new ArgumentNullException("asm", "The assembly is required in NetFX!"); }
      if (asm == null) { asm = Assembly.GetExecutingAssembly(); }
      Type toGet = typeof(TAttr);
      foreach (Type t in asm.GetTypes())
      {
        foreach (var attr in t.GetCustomAttributes(toGet, false))
        {
          yield return (TAttr)attr;
        }
      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns all of the given types that have the <typeparamref name="TAttr"/> attribute set upon them.
    /// </summary>
    public static IEnumerable<Type> GetTypesWithAttribute<TAttr>(Assembly asm = null)
    {
      if (asm == null) { asm = Assembly.GetCallingAssembly(); }

      Type toGet = typeof(TAttr);
      foreach (Type t in asm.GetTypes())
      {
        foreach (var attr in t.GetCustomAttributes(toGet, false))
        {
          yield return t;
        }
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets the GUID of the current (calling) assembly, or an optionally supplied assembly.
    /// </summary>
    /// <returns>A string representing the GUID of the given assembly, or null if none has been defined.</returns>
    /// TODO: This and methods of related functionality should be pushed off to 'AssemblyHelpers' or something like that....
    public static string GetAssemblyGUID(Assembly asm = null)
    {
      if (asm == null) { asm = Assembly.GetCallingAssembly(); }
      object[] objects = asm.GetCustomAttributes(typeof(GuidAttribute), false);
      if (objects.Length > 0)
      {
        return ((GuidAttribute)objects[0]).Value;
      }
      else
      {
        return null;
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static string GetAssemblyName(Assembly asm = null)
    {
      if (asm == null) { asm = Assembly.GetCallingAssembly(); }
#if NETFX_CORE
      // TODO: _TEST: Find out if this is an exact match or not!  It might just be a simple name which doesn't have parity with
      // the .NET version of this call (they should produce identical results!)
      return asm.FullName.Split(',')[0];
#else
      return asm.GetName().Name;
#endif

    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Get a formatted version string of the calling or supplied assembly in the form '1.2.3.4'
    /// </summary>
    public static string GetAssemblyVersion(Assembly asm = null)
    {
#if NETFX_CORE
      throw new NotImplementedException();
#else
      if (asm == null) { asm = Assembly.GetCallingAssembly(); }
      return asm.GetName().Version.ToString();

#endif

    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets the name of the property listed in the expression.
    /// Nesting is preserved so the entire path will appear, i.e. -> loc.Person.Name.Middle = "Person.Name.Middle"
    /// </summary>
    [Obsolete("prefer 'get nested property name!")]
    public static string GetFullPropertyName(Expression propExp)
    {
      MemberExpression memberExp = ReflectionTools.GetMemberExpression(propExp);
      string memberString = memberExp.ToString();
      return memberString.Substring(memberString.IndexOf('.') + 1);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Extracts an embedded resource from the given assembly, returning it as a Stream.  If your resource is in a sub-folder, 
    /// use the 'folder.name' convention or it won't be located by this function.
    /// </summary>
    /// <param name="asm">The assembly to search in.  If this is null, the currently executing assembly will be used.</param>
    /// <remarks>
    /// Dispose the resulting stream when you are done using it!
    /// </remarks>
    public static Stream GetEmbeddedStream(string resourceName, Assembly asm = null)
    {
      if (asm == null)
      {
        asm = Assembly.GetCallingAssembly();
      }


      bool autoResolve = false;
      try
      {
        if (autoResolve)
        {
          string[] names = asm.GetManifestResourceNames();
          foreach (string n in names)
          {
            if (n.EndsWith(resourceName))
            {
              resourceName = n;
              break;
            }
          }
        }

        Stream data = asm.GetManifestResourceStream(resourceName);

        if (data == null)
        {

          string msg = "Could not locate embedded resource '" + resourceName + "' in assembly '" + GetAssemblyName(asm) + "'";
          msg += Environment.NewLine + Environment.NewLine + "Valid names are:" + Environment.NewLine;
          foreach (string item in asm.GetManifestResourceNames())
          {
            msg += item + Environment.NewLine;
          }
          throw new Exception(msg);
        }

        return data;
      }
      catch (Exception e)
      {
        throw new Exception(GetAssemblyName(asm) + ": " + e.Message);
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static byte[] GetEmbeddedBytes(string resourceName, Assembly asm = null)
    {
      if (asm == null)
      {
        asm = Assembly.GetCallingAssembly();
      }

      using (Stream data = GetEmbeddedStream(resourceName, asm))
      {
        byte[] all = new byte[data.Length];
        data.Read(all, 0, all.Length);
        return all;
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns the entire contents of the given resource as an ASCII string.
    /// </summary>
    /// <remarks>
    /// Be careful when using this with resources that are large in size.
    /// </remarks>
    public static string GetEmbeddedString(string resourceName, Assembly asm = null)
    {
      if (asm == null)
      {
        asm = Assembly.GetCallingAssembly();
      }

      byte[] data = GetEmbeddedBytes(resourceName, asm);
      string res = ASCIIEncoding.ASCII.GetString(data, 0, (int)data.Length);
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Locates the embedded resource and attempts to load it as XML data and return it as an XDocument.
    /// </summary>
    /// <param name="asm">The assembly to search in.  If this is null, the currently executing assembly will be used.</param>
    public static XDocument GetEmbeddedXML(string resourceName, Assembly asm = null)
    {
      //Find and read XML template
      var stream = GetEmbeddedStream(resourceName, asm);

      var reader = XmlReader.Create(stream);
      XDocument ret = XDocument.Load(reader);
      reader.Close();

      return ret;
    }



#endif

    // --------------------------------------------------------------------------------------------------------------------------
    public static IEnumerable<PropertyInfo> GetProperties<T>(bool includePrivate = false)
    {
      return GetProperties(typeof(T), includePrivate);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static IEnumerable<PropertyInfo> GetProperties(Type type, bool includePrivate = false)
    {
#if !NETFX_CORE
      BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
      IEnumerable<PropertyInfo> res = type.GetProperties(flags);
#else
      IEnumerable<PropertyInfo> res = type.GetRuntimeProperties();
#endif

      if (includePrivate)
      {
#if !NETFX_CORE
        flags = BindingFlags.NonPublic | BindingFlags.Instance;
        IEnumerable<PropertyInfo> privateProps = type.GetProperties(flags);

        res = res.Concat(privateProps);
      }
#else
      }
      else
      {
        // We have to manually filter to get only the public props.  Kind of silly if you ask me...
        List<PropertyInfo> publics = new List<PropertyInfo>();
        foreach (var item in res)
        {
          PropertyInfo p = type.GetRuntimeProperty(item.Name);
          if (p != null)
          {
            publics.Add(p);
          }
        }
        res = publics;
      }
#endif

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static IEnumerable<FieldInfo> GetFields<T>(bool includeNonPublics = false)
    {
      return GetFields(typeof(T), includeNonPublics);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static IEnumerable<FieldInfo> GetFields(Type type, bool includeNonPublics = false, bool filterAutoProps = true)
    {
      // REFACTOR: This code could be cleaned up.  Sort of along the lines of the WinRT approach.
      if (!filterAutoProps) { throw new NotImplementedException(); }

#if !NETFX_CORE
      BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
      IEnumerable<FieldInfo> res = type.GetFields(flags);
#else

      IEnumerable<FieldInfo> res = type.GetRuntimeFields();
      if (!includeNonPublics)
      {
        res = from x in res
              where !x.IsPrivate
              select x;
      }
#endif

#if !NETFX_CORE

      if (includeNonPublics)
      {
        flags = BindingFlags.NonPublic | BindingFlags.Instance;
        IEnumerable<FieldInfo> privateProps = type.GetFields(flags);

        // NOTE: We are filtering out the backing members of auto properties...
        // 5.2012 --> At this time there is no support to include them!
        IEnumerable<FieldInfo> privateFields = from x in type.GetFields(flags)
                                               where !x.Name.StartsWith("<") &&
                                                     !x.Name.EndsWith("BackingField")
                                               select x;


        res = res.Concat(privateFields);
      }
#endif

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static bool HasAttribute<TAttr>(Type onType, bool inherit = true)
    where TAttr : Attribute
    {
      if (onType == null) { throw new ArgumentNullException("onType"); }
      var attrs = onType.GetCustomAttributes(inherit);

      foreach (var a in attrs)
      {
        if (a.GetType() == typeof(TAttr)) { return true; }
      }
      return false;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Find and return all of the properties that have the given attribute.
    /// </summary>
    public static bool HasAttribute<TAttr>(FieldInfo field)
      where TAttr : Attribute
    {
      var attrs = field.GetCustomAttributes(typeof(TAttr), true);
      var match = (from x in attrs
                   where x.GetType() == typeof(TAttr)
                   select x).SingleOrDefault();
      return match != null;

    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Find and return all of the properties that have the given attribute.
    /// </summary>
    public static bool HasAttribute<TAttr>(PropertyInfo prop)
      where TAttr : Attribute
    {
#if !NETFX_CORE
      var attrs = prop.GetCustomAttributes(typeof(TAttr), true);
#else
      var attrs = prop.GetCustomAttributes<TAttr>();
#endif

      var match = (from x in attrs
                   where x.GetType() == typeof(TAttr)
                   select x).SingleOrDefault();
      return match != null;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public static bool TypeIsWider(Type type1, Type type2)
    {
#if !NETFX_CORE
      if (!type1.IsPrimitive || !type2.IsPrimitive)
#else
      if (!type1.GetTypeInfo().IsPrimitive || !type2.GetTypeInfo().IsPrimitive)
#endif
      {
        throw new NotImplementedException("Non primitive comparisons aren't supported at this time!");
      }

      if (PrimitveWideningTable == null) { CreatePrimitiveWideningTable(); }
      return PrimitveWideningTable[type1].Contains(type2);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Determines if the given type has the interface <typeparamref name="TInterface"/>
    /// </summary>
    public static bool HasInterface<TInterface>(Type type)
    {
      return HasInterface(type, typeof(TInterface));
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static bool HasInterface(Type type, Type interfaceType)
    {
#if NETFX_CORE
      var ti = type.GetTypeInfo();
      var i = (from x in ti.ImplementedInterfaces
               where x.Name == interfaceType.Name
               select x).SingleOrDefault();
#else
      var i = type.GetInterface(interfaceType.Name);
#endif

      return i != null;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private static Dictionary<Type, List<Type>> PrimitveWideningTable = null;
    private static void CreatePrimitiveWideningTable()
    {
      var pTable = new Dictionary<Type, List<Type>>();

      // I baiscally got this list from:
      // http://msdn.microsoft.com/en-us/library/k1e94s7e(v=vs.80).aspx

      pTable.Add(typeof(byte), new List<Type>()
      {
        typeof(byte), typeof(short), typeof(int), typeof(long), typeof(decimal), typeof(float), typeof(double)
      });

      pTable.Add(typeof(short), new List<Type>()
      {
        typeof(short), typeof(int), typeof(long), typeof(decimal), typeof(float), typeof(double),
      });

      pTable.Add(typeof(uint), new List<Type>()
      {
        typeof(uint), typeof(int), typeof(long), typeof(decimal), typeof(float), typeof(double),
      });

      pTable.Add(typeof(int), new List<Type>()
      {
        typeof(int), typeof(long), typeof(decimal), typeof(float), typeof(double),
      });

      pTable.Add(typeof(long), new List<Type>()
      {
        typeof(long), typeof(decimal), typeof(float), typeof(double),
      });

      pTable.Add(typeof(decimal), new List<Type>()
      {
        typeof(decimal), typeof(float), typeof(double),
      });

      pTable.Add(typeof(float), new List<Type>()
      {
        typeof(float), typeof(double),
      });

      pTable.Add(typeof(double), new List<Type>()
      {
        typeof(double),
      });


      PrimitveWideningTable = pTable;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void ApplyPropertyValues<T>(T target, IEnumerable<Tuple<string, object>> propDatas)
    {
      Type t = typeof(T);
#if !NETFX_CORE
      PropertyInfo[] props = t.GetProperties();
#else
      var props = t.GetRuntimeProperties();
#endif
      Dictionary<PropertyInfo, object> matches = GetMatchingProperties(propDatas, props);

      ApplyPropertyValues(target, matches);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void ApplyPropertyValues<T>(T target, IEnumerable<KeyValuePair<PropertyInfo, object>> propDatas)
    {

      foreach (var data in propDatas)
      {
        PropertyInfo prop = data.Key;

#if NET_40

        var setMethod = prop.GetSetMethod();
        if (setMethod != null)
        {
          prop.SetValue(target, ConvertEx(prop.PropertyType, data.Value), null);
        }
#else
        if (prop.SetMethod != null)
        {
          prop.SetValue(target, ConvertEx(prop.PropertyType, data.Value));
        }
#endif
        else
        {
          Debug.WriteLine("The property '{0}' on type '{1}' does not have a setter!", prop.Name, typeof(T));
        }
      }

    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Creates an instance of, and populates members of T based on the passed list of property values.
    /// </summary>
    public static T Populate<T>(IEnumerable<Tuple<string, object>> propDatas)
      where T : new()
    {
      Type t = typeof(T);

#if !NETFX_CORE
      PropertyInfo[] props = t.GetProperties();
#else
      var props = t.GetRuntimeProperties();
#endif
      Dictionary<PropertyInfo, object> matches = GetMatchingProperties(propDatas, props);

      T res = Activator.CreateInstance<T>();
      ApplyPropertyValues(res, matches);


      return res;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    private static Dictionary<PropertyInfo, object> GetMatchingProperties(IEnumerable<Tuple<string, object>> propDatas,
                                                                          IEnumerable<PropertyInfo> props)
    {
      var matches = new Dictionary<PropertyInfo, object>();
      foreach (var p in props)
      {
        var kvp = (from x in propDatas
                   where x.Item1 == p.Name
                   select x).SingleOrDefault();
        if (kvp != null)
        {
          matches.Add(p, kvp.Item2);
        }
      }
      return matches;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns all of the types in the assembly that implement <typeparam name="TInterface"/>
    /// </summary>
#if NETFX_CORE
    public static IEnumerable<Type> GetTypesWithInterface<TInterface>(Assembly asm)
    {
#else
    public static IEnumerable<Type> GetTypesWithInterface<TInterface>(Assembly asm = null)
    {
      if (asm == null) { asm = Assembly.GetCallingAssembly(); }
#endif

      string interfaceName = typeof(TInterface).Name;

      Type toGet = typeof(TInterface);
      foreach (Type t in asm.GetTypes())
      {
#if NETFX_CORE
        IEnumerable<Type> interfaces = t.GetTypeInfo().ImplementedInterfaces;
        foreach (var i in interfaces)
        {
          if (i == toGet)
          {
            yield return t;
            break;
          }
        }

#else
        var res = t.GetInterface(interfaceName);
        if (res != null)
        {
          yield return t;
        }
#endif
      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns all of the properties that have the given <typeparamref name="TAttr"/> attribute applied.
    /// </summary>
    public static IEnumerable<AttributeAndProp<TAttr>> GetAttributesOnProperties<TAttr, TTarget>()
      where TAttr : Attribute
    {

      Type attrType = typeof(TAttr);
      Type target = typeof(TTarget);

#if !NETFX_CORE
      var allProps = target.GetProperties();
#else
      var allProps = target.GetRuntimeProperties();
#endif

      foreach (var prop in allProps)
      {
        foreach (var a in prop.GetCustomAttributes(attrType, false))
        {
          yield return new AttributeAndProp<TAttr>((TAttr)a, prop);
        }
      }

    }


#if !NETFX_CORE

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This will return all of the attributes that match <typeparamref name="TAttr"/> on the given type.
    /// </summary>
    public static IEnumerable<TAttr> GetAttributeOnType<TAttr>(Type targetType, Assembly asm)
    {
      if (asm == null) { throw new ArgumentNullException("asm"); }

      Type attrType = typeof(TAttr);
      foreach (var attr in targetType.GetCustomAttributes(attrType, false))
      {
        yield return (TAttr)attr;
      }

    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This will return all of the attributes that match <typeparamref name="TAttr"/> on the given type.
    /// </summary>
    public static IEnumerable<TAttr> GetAttributeOnType<TAttr, TTarget>(Assembly asm = null)
    {
      if (asm == null) { asm = Assembly.GetCallingAssembly(); }
      return GetAttributeOnType<TAttr>(typeof(TTarget), asm);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Finds and returns all of the attribute objects marked with <typeparamref name="TAttr"/> and their associated types.
    /// </summary>
    public static IEnumerable<Tuple<TAttr, Type>> GetAttributesAndTypes<TAttr>(Assembly asm = null)
    {
      if (asm == null) { asm = Assembly.GetCallingAssembly(); }

      Type toGet = typeof(TAttr);
      foreach (Type t in asm.GetTypes())
      {
        foreach (var attr in t.GetCustomAttributes(toGet, false))
        {
          yield return new Tuple<TAttr, Type>((TAttr)attr, t);
        }
      }

    }

#endif

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Find and return all of the properties that have the given attribute.
    /// </summary>
    [Obsolete("Use the version of this function that takes zero arguments!")]
    public static IEnumerable<PropertyInfo> GetPropertiesWithAttribute<TTarget, TAttr>(TTarget instance) where TAttr : Attribute

    {
      Type tTarget = typeof(TTarget);
      Type tAttr = typeof(TAttr);

      var props = GetProperties<TTarget>();
      foreach (var prop in props)
      {
        IEnumerable<object> attrs = prop.GetCustomAttributes(tAttr, false);
        if (attrs.Count() > 0)
        {
          yield return prop;
        }
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static IEnumerable<PropertyInfo> GetPropertiesWithAttribute<TTarget, TAttr>() where TAttr : Attribute
    {
      Type tTarget = typeof(TTarget);
      Type tAttr = typeof(TAttr);

      var props = GetProperties<TTarget>();
      foreach (var prop in props)
      {
        IEnumerable<object> attrs = prop.GetCustomAttributes(tAttr, false);
        if (attrs.Count() > 0)
        {
          yield return prop;
        }
      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Return all of the methods with the given name on the given type.
    /// </summary>
    public static MethodInfo[] GetMethods(Type t, string name)
    {

#if !NETFX_CORE
      IEnumerable<MethodInfo> srcMethods = t.GetMethods();
#else
      IEnumerable<MethodInfo> srcMethods = t.GetRuntimeMethods();
#endif
      var res = (from x in srcMethods where x.Name == name select x).ToArray();
      return res;

    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// For our purposes, we define a 'simple' type as something that is a primitive or a string.
    /// String is included because it is routinely treated like a primitive, even though it is actually a reference type.
    /// </summary>
    public static bool IsSimpleType(Type t)
    {
#if !NETFX_CORE
      return t.IsPrimitive || t == typeof(string) || t == typeof(decimal);
#else
      return t.GetTypeInfo().IsPrimitive || t == typeof(string) || t == typeof(decimal);
#endif
    }


#if !NETFX_CORE

    // --------------------------------------------------------------------------------------------------------------------------
    public static IEnumerable<Type> GetTypesWithBase<TBase>(Assembly asm = null)
    {
      if (asm == null) { asm = Assembly.GetCallingAssembly(); }
      Type toGet = typeof(TBase);


      Type[] allTypes = asm.GetTypes();
      foreach (Type t in allTypes)
      {
        Type tBase = t.BaseType;

        while (tBase != null)
        {

          if (tBase == toGet)
          {
            yield return t;
          }

          // NOTE: This is going to go up the chain of inheritance.
          // We could easily add overload of this function to only search a certain depth, etc.
          tBase = tBase.BaseType;
        }

      }
    }



    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Extracts an embedded resource from the given assembly, returning it as a Stream.  If your resource is in a sub-folder, 
    /// use the 'folder.name' convention or it won't be located by this function.
    /// </summary>
    /// <param name="asm">The assembly to search in.  If this is null, the currently executing assembly will be used.</param>
    public static Stream GetEmbeddedResource(string resourceName, Assembly asm = null)
    {
      if (asm == null) { asm = Assembly.GetCallingAssembly(); }


      bool autoResolve = false;
      try
      {
        if (autoResolve)
        {
          string[] names = asm.GetManifestResourceNames();
          foreach (string n in names)
          {
            if (n.EndsWith(resourceName))
            {
              resourceName = n;
              break;
            }
          }
        }

        Stream data = asm.GetManifestResourceStream(resourceName);

        if (data == null)
        {

          string msg = "Could not locate embedded resource '" + resourceName + "' in assembly '" + GetAssemblyName(asm) + "'";
          msg += Environment.NewLine + Environment.NewLine + "Valid names are:" + Environment.NewLine;
          foreach (string item in asm.GetManifestResourceNames())
          {
            msg += item + Environment.NewLine;
          }
          throw new Exception(msg);
        }

        return data;
      }
      catch (Exception e)
      {
        throw new Exception(GetAssemblyName(asm) + ": " + e.Message);
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Locates the embeded resource, and returns it as a string.  Useful for reading in bits of text, etc.
    /// </summary>
    public static string GetEmbeddedString(string resourceName, Assembly asm = null, Encoding encoding = null)
    {
      if (asm == null) { asm = Assembly.GetCallingAssembly(); }
      var stream = GetEmbeddedResource(resourceName, asm);

      byte[] data = new byte[stream.Length];
      stream.Read(data, 0, data.Length);

      Encoding useEncoding = encoding ?? Encoding.ASCII;
      return (useEncoding.GetString(data));
    }


#endif

    // --------------------------------------------------------------------------------------------------------------------------
    public static IEnumerable<FieldInfo> GetFields(Type type, bool includePrivate = false)
    {
#if !NETFX_CORE
      BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
      IEnumerable<FieldInfo> res = type.GetFields(flags);
      if (includePrivate)
      {
        flags = BindingFlags.NonPublic | BindingFlags.Instance;
        IEnumerable<FieldInfo> privateProps = type.GetFields(flags);

        res = res.Concat(privateProps);
      }
#else

      IEnumerable<FieldInfo> res = type.GetRuntimeFields();
      if (!includePrivate)
      {
        res = from x in res
              where x.IsPublic
              select x;
      }

#endif
      return res;
    }



    // --------------------------------------------------------------------------------------------------------------------------
    public static bool TryConvert(Type cType, object input, out object output)
    {
      // NOTE: This is obviously not a very efficient operation if the conversion is not possible.
      // 07.31.2013 --> I don't really feel like it is worth addressing at this time however.
      try
      {
        output = ConvertEx(cType, input);
        return true;
      }
      catch (Exception ex)
      {
        if (ex is FormatException || ex is InvalidCastException)
        {
          output = null;
          return false;
        }
        throw;
      }

    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Attempts to resolve an enum value from the given string (case insensitive)
    /// It will also return default values if the given name is null or empty.
    /// </summary>
    public static TEnum ParseEnum<TEnum>(string name)
    {
      // Provide a reasonable default.
      if (string.IsNullOrWhiteSpace(name))
      {
        return default(TEnum);
      }

      string[] names = Enum.GetNames(typeof(TEnum));
      string[] match = (from x in names where x.Equals(name, StringComparison.OrdinalIgnoreCase) select x).ToArray();
      if (match.Length == 0)
      {
        throw new InvalidOperationException(string.Format("The name {0} is non a member of enum Type '{1}'!", name, typeof(TEnum)));
      }

      string useName = null;
      if (match.Length > 1)
      {
        throw new NotSupportedException("Can't resolve ambiguous matches at this time!");
      }
      else
      {
        useName = match[0];
      }

      return (TEnum)Enum.Parse(typeof(TEnum), useName);
    }



    // --------------------------------------------------------------------------------------------------------------------------
    // TODO: We should have a generic version of this function.
    // TODO: It should also handle the parsing of enums....
    public static object ConvertEx(Type cType, object input)
    {

      if (IsNullable(cType))
      {
#if !NETFX_CORE
        Type convertToType = cType.GetGenericArguments()[0];
#else
        Type convertToType = cType.GenericTypeArguments[0];
#endif

        object res = null;
        if (input != null)
        {
          TryConvert(convertToType, input, out res);
        }
        return res;

      }
      else if (IsDateType(cType) && IsDateType(input.GetType()))
      {
        // We may have compatible date types that need a bit of extra coaxing to work.
        Type iType = input.GetType();
        if (cType == iType)
        {
          return input;
        }
        else
        {
          if (cType == typeof(DateTimeOffset) && iType == typeof(DateTime))
          {
            return new DateTimeOffset((DateTime)input);
          }
          else if (cType == typeof(DateTime) && iType == typeof(DateTimeOffset))
          {
            DateTimeOffset local = ((DateTimeOffset)input);

            DateTime res = new DateTime(local.Ticks, DateTimeKind.Utc).ToLocalTime();
            return res;
          }
          else
          {
            // This is unlikely, but is some future case....
            throw new NotSupportedException(string.Format("Conversion form date type '{0}' to date type '{1}' is not supported!", iType, cType));
          }
        }
      }
      else if (input is Guid)
      {
        return input.ToString();
      }
      else
      {
        // Last ditch!
        return System.Convert.ChangeType(input, cType);
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static T ConvertEx<T>(object input)
    {
      return (T)ConvertEx(typeof(T), input);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static bool IsDateType(Type type)
    {
      if (IsNullable(type))
      {
#if !NETFX_CORE
        Type innerType = type.GetGenericArguments()[0];
#else
        Type innerType = type.GenericTypeArguments[0];
#endif
        return IsDateType(innerType);
      }

      return type == typeof(DateTime) ||
             type == typeof(DateTimeOffset);
    }

  }


  // ============================================================================================================================
  /// <summary>
  /// Representative of a member of a type (field or property), and its value.
  /// </summary>
  [DebuggerDisplay("{Name} : {Type}")]
  public class TypeMember
  {
    private PropertyInfo Prop = null;
    private MethodInfo PropGetter = null;
    private FieldInfo Field = null;


    // --------------------------------------------------------------------------------------------------------------------------
    public TypeMember(PropertyInfo prop_)
    {
      if (prop_ == null) { throw new ArgumentNullException("prop_"); }
      MemberType = EMemberType.Property;

      Prop = prop_;
      Name = Prop.Name;
      Type = Prop.PropertyType;
      DeclaringType = Prop.DeclaringType;

#if !NETFX_CORE
      PropGetter = Prop.GetGetMethod();
      if (PropGetter == null) { PropGetter = Prop.GetGetMethod(true); }
#else
      // _TEST: If the property is readonly, will this work ??
      PropGetter = Prop.GetMethod;
#endif

      if (PropGetter == null)
      {
        throw new Exception(string.Format("Could not resolve a getter for property '{0}'", Prop.Name));
      }

#if !NETFX_CORE
      IsReadonly = Prop.GetSetMethod(true) == null;
#else
      // NOTE: The version above is including non-public setters!
      IsReadonly = Prop.SetMethod == null;
#endif

    }


    // --------------------------------------------------------------------------------------------------------------------------
    public TypeMember(FieldInfo field_)
    {
      if (field_ == null) { throw new ArgumentNullException("field_"); }
      MemberType = EMemberType.Field;

      Field = field_;
      Name = Field.Name;
      Type = Field.FieldType;
      DeclaringType = Field.DeclaringType;
      IsReadonly = Field.IsInitOnly | Field.IsLiteral;
    }


    #region Properties

    public Type DeclaringType { get; private set; }

    public string Name { get; private set; }
    public Type Type { get; private set; }

    public bool IsIndexer
    {
      get
      {

        if (MemberType == EMemberType.Property)
        {
          return Prop.GetIndexParameters().Length > 0;
        }
        return false;
      }
    }

    public EMemberType MemberType { get; private set; }

    /// <summary>
    /// Determines if the member is readonly or not.
    /// </summary>
    public bool IsReadonly { get; private set; }

    #endregion


    // --------------------------------------------------------------------------------------------------------------------------
    public object GetValue(object target)
    {
      switch (MemberType)
      {
        case EMemberType.Property:
          return Prop.GetValue(target, null);
        case EMemberType.Field:
          return Field.GetValue(target);

        default:
          throw new ArgumentOutOfRangeException(string.Format("The value {0} is not supported", MemberType));
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Sets the value on the given target.
    /// </summary>
    /// <returns>A reference to the target, in case of setting 'struct' values.  Normally passing structs
    /// will make a copy of them, and so without another copy it just doesn't work...</returns>
    public object SetValue(object target, object value)
    {
      switch (MemberType)
      {
        case EMemberType.Property:
#if NET_40
          Prop.SetValue(target, value, null);
#else
          Prop.SetValue(target, value);
#endif
          break;

        case EMemberType.Field:
          Field.SetValue(target, value);
          break;

        default:
          throw new ArgumentOutOfRangeException(string.Format("The value {0} is not supported", MemberType));
      }

      return target;
    }

  }

#if !NETFX_CORE
#endif



  // ============================================================================================================================
  public class AttributeAndProp<T>
    where T : Attribute
  {
    // --------------------------------------------------------------------------------------------------------------------------
    public AttributeAndProp(T attr_, PropertyInfo prop_)
    {
      Attribute = attr_;
      Property = prop_;
    }

    public T Attribute { get; private set; }
    public PropertyInfo Property { get; private set; }
  }


  // ============================================================================================================================
  public enum EMemberType
  {
    Invalid = 0,
    Property,
    Field,
  }


  // ============================================================================================================================
  /// <summary>
  /// A generalized version of 'BindingFlags' that both win32/winRT callers can use.
  /// </summary>
  public enum EMemberScope
  {
    Invalid = 0,
    Public = 1,
    Private = 2,
    Static = 4,
    //    Instance = 8,
  }


}
