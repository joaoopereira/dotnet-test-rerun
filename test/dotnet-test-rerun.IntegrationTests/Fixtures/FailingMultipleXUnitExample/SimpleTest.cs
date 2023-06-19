namespace FailingXUnitExample;

public class SimpleTest
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(4, 4)]
    public void SimpleNumberCompare(int number, int expectedNumber)
    {
        Assert.Equal(expectedNumber, number);
    }
    
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 3)]
    [InlineData(4, 6)]
    public void SimpleFailedNumberCompare(int number, int expectedNumber)
    {
        Assert.Equal(number, expectedNumber);
    }
}