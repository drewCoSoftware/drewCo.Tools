using drewCo.Tools;


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif


namespace drewCo.Tools.Testers
{
  // ============================================================================================================================
  [TestClass]
  public class DTOMapperTesters
  {

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test case was provided to solve a bug where members on an anonymous type could not be copied to
    /// a concrete one.
    /// </summary>
    [TestMethod]
    public void CanCopyMembersFromAnonymousTypeToConcreteType()
    {
      {
        var concrete = new ListType1();
        Assert.IsNull(concrete.Name, "The name should be null!");

        // Does this work for a simple type, like string.... ??
        const string TEST_NAME = "MyTestName";
        var anonInstance = new { Name = TEST_NAME };

        DTOMapper.CopyMembers(anonInstance, concrete);
        Assert.AreEqual(TEST_NAME, concrete.Name, "The name data should have been copied!");
      }

      // Let's try something that is more complex.....
      {
        var concrete = new TypeWithCompositeMembers();
        Assert.IsNull(concrete.Nested, "Nested data should be null!");
        
        var nested = new NestingType() { Number = 123 };
        var anon = new { Nested = nested };
        DTOMapper.CopyMembers(anon, concrete);

        Assert.IsNotNull(concrete.Nested, "Nested data should not be null after 'CopyMembers'!");
      }


    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test case was provided to solve a bug where copying lists in DTOMapper would choke if the list items were
    /// deemed incompatible.  Technically, we should just copy each of the elemets like anything else in DTO mapper...
    /// </summary>
    [TestMethod]
    public void CanCopyListsFromDifferentNamespaces()
    {

      const string NAME_1 = "myName";
      const int NUMBER_1 = 123;

      ListHost_T1 host1 = new ListHost_T1();
      host1.List.Add(new ListType1()
      {
        Name = NAME_1,
        Number = NUMBER_1,
      });

      ListHost_T2 host2 = new ListHost_T2();
      DTOMapper.CopyMembers(host1, host2);

      Assert.AreEqual(1, host2.List.Count, "There should be one item!");
      var item = host2.List[0];
      Assert.AreEqual(NAME_1, item.Name);
      Assert.AreEqual(NUMBER_1, item.Number);

    }




    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// </summary>
    [TestMethod]
    public void CanCreateInstanceWithNestedData()
    {

      const int TEST_1 = 123;
      const int TEST_2 = 345;

      object anon = new { Number = TEST_1, Nested = new { Number = TEST_2 } };
      NestingType data = (NestingType)DTOMapper.CreateFrom(typeof(NestingType), anon);

      // Make sure the props match.
      Assert.AreEqual(TEST_1, data.Number);
      Assert.AreEqual(TEST_2, data.Nested.Number);
      Assert.IsNull(data.Nested.Nested, "Nested 2x instance should be null!");

    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test case was provided to solve a problem where a target list that isn't directly writable having a source value
    /// of null would cause a nullref during property copy operations.  This can happen when we have an observable collection that
    /// is initialized internally to a class, and we try to copy a null list into it.
    /// </summary>
    [TestMethod]
    public void CanCopyTypeWithNullListToNonWritableList()
    {
      ListHost3 from = new ListHost3();
      ListHost3 to = new ListHost3(new List<int>() { 1, 2, 3 });

      // NOTE: Because the target item isn't directly settable, the best we can do is clear it out.
      DTOMapper.CopyMembers(from, to);
      Assert.AreEqual(0, to.IntList.Count, "The target list should be empty!");

      // Show that the item will remain null.  We basically wrote this to avoid a crash.
      to = new ListHost3();
      DTOMapper.CopyMembers(from, to);
      Assert.IsNull(to.IntList);

    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that the DTOMapper can create new instances of items to be copied.
    /// This is a convenience feature.
    /// </summary>
    [TestMethod]
    public void CanCreateNewCopy()
    {
      const string TEST_NAME = "some name!";
      const int TEST_COUNT = 254;

      SomeType source = new SomeType()
      {
        Name = TEST_NAME,
        Count = TEST_COUNT,
      };

      SomeType copy = DTOMapper.CreateCopy(source);
      Assert.IsNotNull(copy);

      Assert.AreEqual(source.Name, copy.Name);
      Assert.AreEqual(source.Count, copy.Count);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that lists can be copied between items, even if they are dissimilar types.  For example, we can copy between
    /// IList instances, regardless of their exact type.
    /// </summary>
    [TestMethod]
    public void CanCopyLists()
    {
      string[] names = new[]
      {
        "Frank",
        "Larry",
        "David",
        "Chet",
        "Marla"
      };

      ListHost1 source = new ListHost1();
      for (int i = 0; i < names.Length; i++)
      {
        source.IntList.Add(i);
        source.Names.Add(names[i]);

        source.Types.Add(new SomeType()
        {
          Count = i + 100,
          Name = names[i],
        });
      }


      ListHost2 comp = new ListHost2();
      DTOMapper.CopyMembers(source, comp);

      drewCo.UnitTesting.ObjectInspector inspector = new UnitTesting.ObjectInspector();
      var compIntResult = inspector.CompareLists(source.IntList, comp.IntList);
      Assert.IsTrue(compIntResult.Item1, compIntResult.Item2);

      var compNameResult = inspector.CompareLists(source.Names, comp.Names);
      Assert.IsTrue(compNameResult.Item1, compNameResult.Item2);

      var compTypeResult = inspector.CompareLists(source.Types, comp.Types);
      Assert.IsTrue(compTypeResult.Item1, compTypeResult.Item2);

    }

  }


  // ============================================================================================================================
  public class SomeType
  {
    public string Name { get; set; }
    public int Count { get; set; }
  }


  // ============================================================================================================================
  public class ListHost1
  {
    public List<int> IntList { get; set; }
    public List<string> Names { get; set; }
    public List<SomeType> Types { get; set; }

    // --------------------------------------------------------------------------------------------------------------------------
    public ListHost1()
    {
      IntList = new List<int>();
      Names = new List<string>();
      Types = new List<SomeType>();
    }
  }

  // ============================================================================================================================
  public class ListHost2
  {
    public List<int> IntList { get; set; }
    public ObservableCollection<string> Names { get; set; }
    public ObservableCollection<SomeType> Types { get; set; }

    // --------------------------------------------------------------------------------------------------------------------------
    public ListHost2()
    {
      IntList = new List<int>();
      Names = new ObservableCollection<string>();

      // This is left null on purpose to make sure the mapper will create new instances as needed.
      //Types = new ObservableCollection<SomeType>();
    }

  }

  // ============================================================================================================================
  public class ListHost3
  {
    // --------------------------------------------------------------------------------------------------------------------------
    public ListHost3() { }

    // --------------------------------------------------------------------------------------------------------------------------
    public ListHost3(IList<int> ints)
    {
      _IntList = new List<int>(ints);
    }
    private List<int> _IntList;
    public List<int> IntList { get { return _IntList; } }
  }


}
