
namespace drewCo.Tools.Testers;

[TestClass]
public class EZPathTesters
{

    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanCombineEZPath()
    {
      // TODO: Make those string const.
      var p = new EZPath("my-dir");
      p = p / "dir-2";
      Assert.Equal(Path.Combine("my-dir" + Path.DirectorySeparatorChar + "dir-2"), p);
    }


}