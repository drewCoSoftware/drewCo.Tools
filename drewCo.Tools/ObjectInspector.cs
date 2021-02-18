using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using drewCo.Curations;
using System.Linq.Expressions;
using drewCo.Tools;

namespace drewCo.UnitTesting
{

  // ============================================================================================================================
  /// <summary>
  /// Responsible for deep inspection of objects and their equality.
  /// The notion is that even the most complex type can be broken down into a collection of simple value types.
  /// </summary>
  public class ObjectInspector
  {
    private HashSet<Type> IgnoredTypes = new HashSet<Type>();
    private Dictionary<Type, Delegate> Comparers = new Dictionary<Type, Delegate>();

    /// <summary>
    /// Each type can have one or more private members that we can include in the inspection.
    /// We don't do it automatically because of the fact that we will wind up looking at
    /// backing members for auto-props, etc...
    /// </summary>
    private Dictionary<Type, HashSet<string>> PrivateMemberListeners = new Dictionary<Type, HashSet<string>>();

    private static MethodInfo RecursiveCompare = null;
    private static MethodInfo ListCompare = null;
    private static MethodInfo DictionaryCompare = null;
    private static MethodInfo ArrayCompare = null;

    // This item is used represent sets of objects that we have already compared.  This prevents us from having problems
    // with circular references causing endless loops / stack overflows....
    // TODO: I don't think that this should be static anymore.
    private MultiDictionary<object, object, InspectionReport> CachedReports = null;

    /// <summary>
    /// We use the reference caches to make sure that the reference trees are preserved when we do comparisons.
    /// This is important, as the graph of an object, is as useful / important as the data contained within.
    /// </summary>
    private PairDictionary<object, int> SrcRefCache = new PairDictionary<object, int>();
    private PairDictionary<object, int> CompRefCache = new PairDictionary<object, int>();

    private Dictionary<Type, HashSet<string>> ExcludedProperties = new Dictionary<Type, HashSet<string>>();


    // --------------------------------------------------------------------------------------------------------------------------
    public ObjectInspector()
    {
      CachedReports = new MultiDictionary<object, object, InspectionReport>();
      SrcRefCache = new PairDictionary<object, int>();
      CompRefCache = new PairDictionary<object, int>();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    static ObjectInspector()
    {

      Type inspectorType = typeof(ObjectInspector);

#if !NETFX_CORE
      RecursiveCompare = inspectorType.GetMethod("InternalCompare", BindingFlags.Instance | BindingFlags.NonPublic);
      if (RecursiveCompare == null) { throw new InvalidOperationException("Could not resolve recursive comparison routine!"); }

      ArrayCompare = inspectorType.GetMethod("CompareArrays");
      if (ArrayCompare == null) { throw new InvalidOperationException("Could not resolve array comparison routine!"); }

      ListCompare = inspectorType.GetMethod("CompareLists");
      if (ListCompare == null) { throw new InvalidOperationException("Could not resolve list comparison routine!"); }

      DictionaryCompare = inspectorType.GetMethod("CompareDictionaries");
      if (DictionaryCompare == null) { throw new InvalidOperationException("Could not resolve dictionary comparison routine!"); }
#else
      TypeInfo ti = inspectorType.GetTypeInfo();
      RecursiveCompare = ti.GetDeclaredMethod("InternalCompare");
      if (RecursiveCompare == null) { throw new InvalidOperationException("Could not resolve recursive comparison routine!"); }

      ArrayCompare = ReflectionTools.GetMethod(inspectorType, "CompareArrays", EMemberScope.Public);
      if (ArrayCompare == null) { throw new InvalidOperationException("Could not resolve array comparison routine!"); }

      ListCompare = ReflectionTools.GetMethod(inspectorType, "CompareLists", EMemberScope.Public);
      if (ListCompare == null) { throw new InvalidOperationException("Could not resolve list comparison routine!"); }

      DictionaryCompare = ReflectionTools.GetMethod(inspectorType, "CompareDictionaries", EMemberScope.Public);
      if (DictionaryCompare == null) { throw new InvalidOperationException("Could not resolve dictionary comparison routine!"); }

      //TypeInfo ti = inspectorType.GetTypeInfo();
      //RecursiveCompare = ReflectionTools.GetPropertyInfo(inspectorType, "InternalCompare", true, true).GetMethod;
      //if (RecursiveCompare == null) { throw new InvalidOperationException("Could not resolve recursive comparison routine!"); }

      //RecursiveCompare = ReflectionTools.GetPropertyInfo(inspectorType, "CompareArrays", false, true).GetMethod;
      //if (ArrayCompare == null) { throw new InvalidOperationException("Could not resolve array comparison routine!"); }

      //RecursiveCompare = ReflectionTools.GetPropertyInfo(inspectorType, "CompareLists", false, true).GetMethod;
      //if (ListCompare == null) { throw new InvalidOperationException("Could not resolve list comparison routine!"); }

      //RecursiveCompare = ReflectionTools.GetPropertyInfo(inspectorType, "CompareDictionaries", false, true).GetMethod;
      //if (DictionaryCompare == null) { throw new InvalidOperationException("Could not resolve dictionary comparison routine!"); }

#endif
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public InspectionReport CompareObjects<T>(T source, T comp, bool throwOnFail = false)
    {
      return CompareObjects(typeof(T), source, comp, throwOnFail);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public InspectionReport CompareObjects(Type type, object source, object comp, bool throwOnFail = false)
    {
      CachedReports = new MultiDictionary<object, object, InspectionReport>();
      SrcRefCache = new PairDictionary<object, int>();
      CompRefCache = new PairDictionary<object, int>();

      InspectionReport res = InternalCompare(type, source, comp, null, throwOnFail);

      // Cleanup and return...
      CachedReports.Clear();
      CachedReports = null;

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    internal InspectionReport InternalCompare(Type type, object source, object comp, string name, bool throwOnFail = false)
    {
      InspectionReport report = new InspectionReport(name, throwOnFail)
      {
        SourceObject = source,
        CompObject = comp,
      };

      // TODO: We really need to clean this method up!
      // In particular, it should be broken into sub methods, and we should really reduce the number
      // of exit points as they can make this very confusing indeed!!

      if (IgnoredTypes.Contains(type))
      {
        string skipMessage = "The type '{0}' is set to be ignored, and so the member '{1}' has been excluded from the inspection";
        report.Skip(string.Format(skipMessage, type, name));
        return report;
      }

      // One or more nulls require some special handling...
      bool continueEval = EvaluateNulls(source, comp, name, report);
      if (!continueEval)
      {
        return report;
      }

      if (!TypesMatch(source, comp, report))
      {
        return report;
      }


      // Since both items are null, we can see if we have already cached this data....
      if (CachedReports.ContainsKey(source, comp))
      {
        return CachedReports[source, comp];
      }
      CachedReports.Add(source, comp, report);




      if (Comparers.ContainsKey(type))
      {
        bool match = (bool)Comparers[type].DynamicInvoke(source, comp);
        if (!match) { report.Fail(null); }

        report.Pass();
        return report;
      }

      if (ReflectionTools.IsSimpleType(type))
      {
        // NOTE: 3.14.2013 --> Technically strings are reference types, but I can't think of a way to get compare
        // their references in a reasonable way at this time.  I am sure that it is possible, but
        // I am still going to treat them as 'primitve' items.

        bool match = source.Equals(comp);
        if (!match)
        {
          report.Fail(null);
        }
        else
        {
          report.Pass();
        }

        return report;
      }


      // Do the reference comparison.
      bool graphOK = CheckReferenceGraph(source, comp);
      if (!graphOK)
      {
        report.Fail(string.Format("Invalid reference at item '{0}'!", name));
        return report;
      }


      if (IsArray(type))
      {
        CompareArrays(type, source, comp, report);
        return report;
      }

      if (IsIList(type))
      {
        CompareLists(type, source, comp, report);
        return report;
      }


      if (IsIDictionary(type))
      {
        CompareDictionaries(type, source, comp, report);
        return report;
      }


      // We will look at each property and determine if it is a value type, or string.
      // If it is, then we can just do a normal equality test.
      // If not, we will continue to decompose the object.
      // Same goes for the fields.
      List<PropertyInfo> props = ResolveProperties(type);
      List<FieldInfo> fields = ResolveFields(type);


      // NOTE: In the WinRT version, we simply get all of the non public members in the above calls.  We should probably port the Win32 code to do the same thing.
      List<TypeMember> nonPublics = ResolvePrivateMembers(type);
      List<TypeMember> allMembers = (from x in props select new TypeMember(x)).Concat(
                                    (from x in fields select new TypeMember(x))).ToList();

      foreach (var item in nonPublics)
      {
        allMembers.Add(item);
      }

      int memberCount = allMembers.Count;
      for (int i = 0; i < memberCount; i++)
      {
        string memberName = allMembers[i].Name;

        if (allMembers[i].IsIndexer)
        {
          InspectionReport memberReport = new InspectionReport(memberName, throwOnFail);
          memberReport.Skip(string.Format("The member '{0}' is an indexer property and will be ignored.", memberName));
          report.AddMemberReport(memberReport);
          continue;
        }

        object srcVal = allMembers[i].GetValue(source);
        object compVal = allMembers[i].GetValue(comp);

        try
        {

          InspectionReport memberReport = null;

          object res = null;
          var caller = RecursiveCompare;
          try
          {
            res = caller.Invoke(this, new[] { allMembers[i].Type, srcVal, compVal, memberName, throwOnFail });
          }
          catch (TargetInvocationException ex)
          {
            // Unwrap and rethrow basically.
            if (ex.InnerException.GetType() == typeof(ObjectInspectorException))
            {
              throw ex.InnerException;
            }
            throw;
          }

          memberReport = (InspectionReport)res;

          report.AddMemberReport(memberReport);

          // NOTE: This was the old condition.  It turned out that sometimes, during recursive compares,
          // we would have non-matching objects, though their evaluation wasn't complete.  This made it
          // seem as though the items did not match at all....
          //if (memberReport.ObjectsMatch == false)

          // So I changed it to this.  This seems like the right thing to do.  Instead of bailing right away,
          // let it finish doing its thing....
          if (memberReport.ObjectsMatch == false) // && memberReport.InspectionComplete)
          {
            string errMsg = "The member values for '{0}' on type '{1}' do not match!\r\n{2}";
            report.Fail(string.Format(errMsg, memberName, type, memberReport.Message));
            return report;
          }
        }
        catch (Exception ex)
        {
          throw;

          string errMsg = "The inspection of the member '{0}' on Type '{1}' failed!\r\n" +
                           "Message:\r\n{2}";
          report.Fail(string.Format(errMsg, memberName, type, ex.Message));
          return report;
        }
      }

      report.Pass();

      return report;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Makes sure that the items in question match their graph position.
    /// The theory is that if the graphs are correct, then the order of the referenced items will also be the same.
    /// </summary>
    private bool CheckReferenceGraph(object source, object comp)
    {
      int srcID = -1;
      int compID = -1;

      if (!SrcRefCache.ContainsKey(source))
      {
        srcID = SrcRefCache.Count;
        SrcRefCache.Add(source, srcID);
      }
      else
      {
        srcID = SrcRefCache[source];
      }


      if (!CompRefCache.ContainsKey(comp))
      {
        compID = CompRefCache.Count;
        CompRefCache.Add(comp, compID);
      }
      {
        compID = CompRefCache[comp];
      }

      // Do they match ??
      return srcID == compID;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private bool TypesMatch(object source, object comp, InspectionReport report)
    {
      Type sType = source.GetType();
      Type cType = comp.GetType();

      if (sType != cType)
      {
        string errMsg = "The source and comparison types are not the same!  Expected '{0}' and found '{1}'";
        report.Fail(string.Format(errMsg, sType, cType));
        return false;
      }

      return true;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private void CompareDictionaries(Type type, object source, object comp, InspectionReport report)
    {
      // Also, handle it...
#if !NETFX_CORE
      Type[] args = type.GetGenericArguments();
#else
      Type[] args = type.GetTypeInfo().GenericTypeArguments;
#endif
      var compMethod = DictionaryCompare.MakeGenericMethod(args);

      var dictComp = (Tuple<bool, string>)compMethod.Invoke(this, new[] { source, comp, null, null });

      bool match = dictComp.Item1;
      if (!match)
      {
        report.Fail(null);
      }
      else
      {
        report.Pass();
        report.Message = dictComp.Item2;
      }

    }


    // --------------------------------------------------------------------------------------------------------------------------
    private void CompareArrays(Type type, object source, object comp, InspectionReport report)
    {
      Type arrayType = type.GetElementType();
      var listMethod = ArrayCompare.MakeGenericMethod(new[] { arrayType });

      var listComp = (Tuple<bool, string>)listMethod.Invoke(this, new[] { (object)source, (object)comp, null });

      bool match = listComp.Item1;
      if (!match)
      {
        report.Fail(null);
      }
      else
      {
        report.Pass();
        report.Message = listComp.Item2;
      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    private void CompareLists(Type type, object source, object comp, InspectionReport report)
    {

#if !NETFX_CORE
      Type listType = type.GetGenericArguments()[0];
#else
      Type listType = type.GetTypeInfo().GenericTypeArguments[0];
#endif

      var listMethod = ListCompare.MakeGenericMethod(new[] { listType });

      var listComp = (Tuple<bool, string>)listMethod.Invoke(this, new[] { (object)source, (object)comp, null });

      bool match = listComp.Item1;
      if (!match)
      {
        report.Fail(null);
      }
      else
      {
        report.Pass();
        report.Message = listComp.Item2;
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private bool IsArray(Type objectType)
    {
      return objectType.IsArray;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private bool IsIList(Type objectType)
    {
      if (objectType.IsArray) { return false; }

#if !NETFX_CORE
      bool isIList = objectType.GetInterface("IList") != null;
      bool hasGenericArg = objectType.GetGenericArguments().Length == 1;
#else
      TypeInfo ti = objectType.GetTypeInfo();
      bool isIList = ti.ImplementedInterfaces.Any(x => x.Name == "IList");
      bool hasGenericArg = ti.GenericTypeArguments.Length == 1;
#endif

      return isIList && hasGenericArg;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private bool IsIDictionary(Type objectType)
    {
#if !NETFX_CORE
      bool isIDictionary = objectType.GetInterface("IDictionary") != null;
      bool hasGenericArgs = objectType.GetGenericArguments().Length == 2;
#else
      TypeInfo ti = objectType.GetTypeInfo();
      bool isIDictionary = ti.ImplementedInterfaces.Any(x => x.Name == "IDictionary");
      bool hasGenericArgs = ti.GenericTypeArguments.Length == 2;
#endif

      return isIDictionary && hasGenericArgs;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private List<FieldInfo> ResolveFields(Type objectType)
    {

#if !NETFX_CORE
      var fields = ReflectionTools.GetFields(objectType, false).ToList();
#else
      var fields = ReflectionTools.GetFields(objectType, false).ToList();
#endif

      return fields;

    }

    // --------------------------------------------------------------------------------------------------------------------------
    private List<PropertyInfo> ResolveProperties(Type objectType)
    {
      List<PropertyInfo> res = new List<PropertyInfo>();

#if !NETFX_CORE
      IEnumerable<PropertyInfo> props = ReflectionTools.GetProperties(objectType);
#else
      IEnumerable<PropertyInfo> props = ReflectionTools.GetProperties(objectType);
#endif

      foreach (var item in props)
      {
        if (ExcludedProperties.ContainsKey(objectType))
        {
          var set = ExcludedProperties[objectType];
          if (set.Contains(item.Name)) { continue; }
        }

        res.Add(item);
      }
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private List<TypeMember> ResolvePrivateMembers(Type objectType)
    {

      if (PrivateMemberListeners.ContainsKey(objectType))
      {

      #if NETFX_CORE
        var fields = (from x in objectType.GetTypeInfo().DeclaredFields
        where !x.IsPublic && !x.IsStatic 
        select x);

        var props = (from x in objectType.GetTypeInfo().DeclaredProperties
        where !x.CanRead && !x.CanWrite
        select x);

//                      var props = objectType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);
      #else
        var fields = objectType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        var props = objectType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);
      #endif

        var res = (from x in fields
                   where PrivateMemberListeners[objectType].Contains(x.Name)
                   select new TypeMember(x))
                   .Concat
                   (from x in props
                    where PrivateMemberListeners[objectType].Contains(x.Name)
                    select new TypeMember(x));

        return res.ToList();
      }

      // We just just return an empty list...
      return new List<TypeMember>();
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Evaluates the input objects for nulls + handles the various conditions.
    /// </summary>
    /// <returns>A boolean value indicating whether the caller should continue evaluation or not.</returns>
    private static bool EvaluateNulls(object source, object comp, string name, InspectionReport report)
    {
      if (source == null)
      {
        if (comp == null)
        {
          // The objects are both null, so we are OK.
          report.Pass();
          return false;
        }
        else
        {
          report.Fail(string.Format("The comparison object is null on member '{0}'!", name));
          return false;
        }
      }
      else if (comp == null)
      {
        report.Fail(string.Format("The comparison object is null on member '{0}'!", name));
        return false;
      }

      // Neither object is null...
      return true;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Adds the given type to the list of filtered types.
    /// This type will no longer be inspected in this instance of ObjectInspector.
    /// </summary>
    public void AddTypeFilter<T>()
    {
      IgnoredTypes.Add(typeof(T));
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void SetPropertyFilter<T>(Expression<Func<T, object>> propExp)
    {
      PropertyInfo p = ReflectionTools.GetPropertyInfo(propExp);
      Type t = p.DeclaringType;

      if (!ExcludedProperties.ContainsKey(t))
      {
        ExcludedProperties.Add(t, new HashSet<string>());
      }

      ExcludedProperties[t].Add(p.Name);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Allows us to provide a custom comparison routine for certain types.
    /// </summary>
    /// <remarks>
    /// Use this feature sparingly.  It is typically reserved for instance comparisons that cannot normally be performed
    /// with <see cref="ObjectInspector"/>
    /// </remarks>
    public void AddTypeComparer<T>(Func<T, T, bool> compFunc)
    {
      Comparers.Add(typeof(T), compFunc);
    }

#if !NETFX_CORE
    // --------------------------------------------------------------------------------------------------------------------------
    public void AddNonPublicListener<T>(string memberName)
    {
      Type t = typeof(T);

      var f = t.GetField(memberName, BindingFlags.NonPublic | BindingFlags.Instance);
      var p = t.GetProperty(memberName, BindingFlags.NonPublic | BindingFlags.Instance);

      if (f == null && p == null)
      {
        throw new Exception(string.Format("There is no member named '{0}' on Type '{1}'!", memberName, t));
      }

      var list = GetNonPublicMemberList(t);
      list.Add(memberName);
    }

#endif

    // --------------------------------------------------------------------------------------------------------------------------
    private HashSet<string> GetNonPublicMemberList(Type t)
    {

      if (PrivateMemberListeners.ContainsKey(t))
      {
        return PrivateMemberListeners[t];
      }
      HashSet<string> hash = new HashSet<string>();
      PrivateMemberListeners.Add(t, hash);

      return hash;
    }





    // --------------------------------------------------------------------------------------------------------------------------
    public Tuple<bool, string> CompareDictionaries<TKey, TValue>(IDictionary<TKey, TValue> src,
                                                                        IDictionary<TKey, TValue> comp,
                                                                        Func<TKey, TKey, bool> keyCompFunc = null,
                                                                        Func<TValue, TValue, bool> valCompFunc = null)
    {
      if (valCompFunc != null)
      {
        // Copy the implementation from list compare...
        throw new NotImplementedException();
      }

      if (src.Count != comp.Count)
      {
        string errMsg = "Entry counts don't match for the given dictionaries! --> src={0} : comp={1}";
        return new Tuple<bool, string>(false, string.Format(errMsg, src.Count, comp.Count));
      }

      for (int i = 0; i < src.Count; i++)
      {

        TKey sKey = src.Keys.ElementAt(i);
        TKey cKey = comp.Keys.ElementAt(i);

        // We actualy have to do a sub inspection of the keys.  The graphs + data have to be the same!
        //ObjectInspector keyInspector = new ObjectInspector();
        //InspectionReport kReport = keyInspector.CompareObjects(sKey, cKey);
        //if (!kReport.Success)
        //{
        //  string errMsg = "The source and comparison dictionary keys don't match at index {0}!\r\n{1}";
        //  return new Tuple<bool, string>(false, string.Format(errMsg, i, kReport.Message));
        //}
        bool keyMatch = false;
        if (CachedReports.ContainsKey(sKey, cKey))
        {
          // TODO: This may need some closer inspection.  We are just skipping items that haven't completed their inspection.
          // They may or may not actually come out OK, but I'm not 100% sure at this point.  Of course, I think that if
          // anything fails, then the whole process will fail.......
          // TODO: I think that the list comparison code may need some similar concessions made......
          var report = CachedReports[sKey, cKey];
          if (report.InspectionComplete)
          {
            keyMatch = report.ObjectsMatch.HasValue && report.ObjectsMatch.Value;
          }
          else
          {
            continue;
          }
        }
        else
        {
          keyMatch = CompareValues<TKey>(sKey, cKey, "Dictionary Key " + i, keyCompFunc);
        }

        if (!keyMatch)
        {
          string errMsg = "Dictionary keys do not match at index {0} --> src={0} : comp={1}";
          return new Tuple<bool, string>(false, string.Format(errMsg, i, sKey, cKey));
        }





        TValue srcVal = src[sKey];
        TValue compVal = comp[cKey];

        bool match = false;
        if (CachedReports.ContainsKey(srcVal, compVal))
        {
          // TODO: This may need some closer inspection.  We are just skipping items that haven't completed their inspection.
          // They may or may not actually come out OK, but I'm not 100% sure at this point.  Of course, I think that if
          // anything fails, then the whole process will fail.......
          // TODO: I think that the list comparison code may need some similar concessions made......
          var report = CachedReports[srcVal, compVal];
          if (report.InspectionComplete)
          {
            match = report.ObjectsMatch.HasValue && report.ObjectsMatch.Value;
          }
          else
          {
            continue;
          }
        }
        else
        {
          match = CompareValues(srcVal, compVal, "Dictionary Item " + sKey, valCompFunc);
        }

        if (!match)
        {
          string errMsg = "The value for key '{0}' does not match! --> src={1} : comp={2}";
          return new Tuple<bool, string>(false, string.Format(errMsg, sKey, srcVal, compVal));
        }

      }

      return new Tuple<bool, string>(true, null);
    }




    // --------------------------------------------------------------------------------------------------------------------------
    public Tuple<bool, string> CompareArrays<TValue>(Array src, Array comp, Func<TValue, TValue, bool> compareFunc = null)
    {
      int srcRank = src.Rank;
      int compRank = comp.Rank;

      if (srcRank != compRank)
      {
        string errMsg = "Array ranks don't match! --> src={0} : comp={1}";
        return new Tuple<bool, string>(false, string.Format(errMsg, srcRank, compRank));
      }


      int[] srcDims = ArrayHelpers.GetDims(src);
      int[] compDims = ArrayHelpers.GetDims(comp);

      for (int i = 0; i < srcDims.Length; i++)
      {
        if (srcDims[i] != compDims[i])
        {
          string errMsg = "Array dimensions at index {0} don't match! --> {1} : {2}";
          return new Tuple<bool, string>(false, string.Format(errMsg, i, srcDims[i], compDims[i]));
        }
      }

      int[] sizes = ArrayHelpers.GetSizes(srcDims);
      int len = ArrayHelpers.GetTotalElements(srcDims);

      for (int i = 0; i < len; i++)
      {
        int[] addr = ArrayHelpers.ComputeIndicies(srcDims, sizes, i);

        TValue srcVal = (TValue)src.GetValue(addr);
        TValue compVal = (TValue)comp.GetValue(addr);


        // TODO: This is basically the same thing as the list comparison function, so let's see about some
        // consolidation....
        bool match = false;
        if (CachedReports.ContainsKey(srcVal, compVal))
        {
          // TODO: This may need some closer inspection.  We are just skipping items that haven't completed their inspection.
          // They may or may not actually come out OK, but I'm not 100% sure at this point.  Of course, I think that if
          // anything fails, then the whole process will fail.......
          // TODO: I think that the list comparison code may need some similar concessions made......
          var report = CachedReports[srcVal, compVal];
          if (report.InspectionComplete)
          {
            match = report.ObjectsMatch.HasValue && report.ObjectsMatch.Value;
          }
          else
          {
            continue;
          }
        }
        else
        {
          match = CompareValues(srcVal, compVal, "List Item " + i, compareFunc);
        }

        if (!match)
        {
          string errMsg = "Array items do not match at location {0} --> src={0} : comp={1}";
          return new Tuple<bool, string>(false, string.Format(errMsg, string.Join(",", addr), srcVal, compVal));
        }
      }


      // Everything was OK!
      return new Tuple<bool, string>(true, null);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    // TODO: I think that the class hierarchy of the 'InspectionReport' needs to be changed to consider that of a list.
    // You see, this function needs to be able to sub-report on all of the members contained within the list itself.
    // That means more recursion, etc.  This is not as easy as it seems I am afraid.  Geez, I probably need to start thinking
    // about going in an async direction as well as this could become time consuming indeed.....
    public Tuple<bool, string> CompareLists<TValue>(IList<TValue> src, IList<TValue> comp, Func<TValue, TValue, bool> compareFunc = null)
    {
      if (src.Count != comp.Count)
      {
        string errMsg = "Entry counts don't match for the given lists! --> src={0} : comp={1}";
        return new Tuple<bool, string>(false, string.Format(errMsg, src.Count, comp.Count));
      }

      for (int i = 0; i < src.Count; i++)
      {
        TValue srcVal = src[i];
        TValue compVal = comp[i];

        // TODO: This is almost exactly like that of the array code.  Consolidate them please!
        bool match = false;
        if (CachedReports.ContainsKey(srcVal, compVal))
        {
          // TODO: This may need some closer inspection.  We are just skipping items that haven't completed their inspection.
          // They may or may not actually come out OK, but I'm not 100% sure at this point.  Of course, I think that if
          // anything fails, then the whole process will fail.......
          // TODO: I think that the list comparison code may need some similar concessions made......
          var report = CachedReports[srcVal, compVal];
          if (report.InspectionComplete)
          {
            match = report.ObjectsMatch.HasValue && report.ObjectsMatch.Value;
          }
          else
          {
            continue;
          }
        }
        else
        {
          match = CompareValues(srcVal, compVal, "List Item " + i, compareFunc);
        }

        if (!match)
        {
          string errMsg = "List items do not match at index {0} --> src={0} : comp={1}";
          return new Tuple<bool, string>(false, string.Format(errMsg, i, src[i], comp[i]));
        }
      }

      return new Tuple<bool, string>(true, null);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private bool CompareValues<TValue>(TValue srcVal, TValue compVal, string name, Func<TValue, TValue, bool> compareFunc = null)
    {
      if (compareFunc == null)
      {
        var compRes = InternalCompare(typeof(TValue), srcVal, compVal, name).ObjectsMatch;
        return compRes.HasValue && compRes.Value;
      }
      else
      {
        return compareFunc.Invoke(srcVal, compVal);
      }

    }


  }


  // ============================================================================================================================
  /// <summary>
  /// Describes the result of an inspection.
  /// </summary>
  public class InspectionReport
  {
    public object SourceObject { get; set; }
    public object CompObject { get; set; }

    /// <summary>
    /// Indicates that the inspection of the objects in question is complete.
    /// This is important to know because of strange conditions that can pop up during recursive compares....
    /// </summary>
    public bool InspectionComplete { get; private set; }
    public bool? ObjectsMatch { get; private set; }

    /// <summary>
    /// Indicates that we have a positive matching flag, and hte inspection was completed correctly.
    /// </summary>
    public bool Success
    {
      get
      {
        return InspectionComplete &&
               ObjectsMatch.HasValue &&
               ObjectsMatch.Value;
      }
    }

    // TODO: Add some type of data to indicate if it was a simple type comparison, and if so, a way to
    // also indicate what the inspected values were??  This may be part of some option for a 'full' report.

    /// <summary>
    /// Name of the member being inspected.
    /// </summary>
    public string MemberName { get; set; }

    /// <summary>
    /// Typically this is used to indicate errors that may have occured during inspections.
    /// </summary>
    public string Message { get; set; }

    private List<InspectionReport> _MemberReports = new List<InspectionReport>();

    public bool ThrowOnFail { get; private set; }

    // --------------------------------------------------------------------------------------------------------------------------
    public InspectionReport(string memberName_, bool throwOnFail_)
    {
      ThrowOnFail = throwOnFail_;
      MemberName = memberName_;
      InspectionComplete = false;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Fail(string message_, bool inspectionOK_ = true)
    {
      inspectionOK_ = true;
      ObjectsMatch = false;
      Message = message_;

      if (ThrowOnFail)
      {
        throw new ObjectInspectorException(Message);
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Skip(string p)
    {
      InspectionComplete = true;
      Message = Message;
      ObjectsMatch = null;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Pass()
    {
      InspectionComplete = true;
      ObjectsMatch = true;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public void AddMemberReport(InspectionReport report)
    {
      _MemberReports.Add(report);
    }



  }


  // ============================================================================================================================
  public class ObjectInspectorException : Exception
  {
    public ObjectInspectorException() { }
    public ObjectInspectorException(string message) : base(message) { }
    public ObjectInspectorException(string message, Exception inner) : base(message, inner) { }
    //protected ObjectInspectorException(
    //System.Runtime.Serialization.SerializationInfo info,
    //System.Runtime.Serialization.StreamingContext context)
    //  : base(info, context) { }
  }
}
