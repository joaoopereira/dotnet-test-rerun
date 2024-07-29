namespace XUnitTestPassOnSecondRunExample;

public class SimpleTest
{
    private static int _state = 0;  // Shared state between tests  

    [Fact]
    public void TestIncreaseState()
    {
        _state += 1;
        Assert.Equal(1, _state);
    }

    [Fact]
    public void TestDecreaseState()
    {
        _state -= 1;
        Assert.Equal(-1, _state);
    }
}