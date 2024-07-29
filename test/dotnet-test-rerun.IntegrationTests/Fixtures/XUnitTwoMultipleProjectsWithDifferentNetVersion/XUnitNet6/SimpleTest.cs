namespace XUnitTestPassOnSecondRunExample;

public class SimpleTest
{
    [Fact]
    public void AssertTwoNumbersAreEqual()
    {
        Assert.Equal(1, 1);
    }
}