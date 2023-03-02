namespace NUnitTestExample;

public class Tests
{
    [TestCase(1, 1)]
    [TestCase(2, 2)]
    [TestCase(3, 3)]
    [TestCase(4, 4)]
    public void SimpleNumberCompare(int number, int expectedNumber)
    {
        Assert.AreEqual(expectedNumber, number);
    }
    
    [TestCase()]
    public void SimpleStringCompare()
    {
        Assert.AreEqual("value", "value");
    }
}