namespace XUnitExample;

public class SimpleTest
{
    [Theory(DisplayName = "My overriden test name")]
    [InlineData("Test 1")]
    [InlineData("Test (2")]
    public void Test1(string _)
    {
        Assert.False(true);
    }
}
