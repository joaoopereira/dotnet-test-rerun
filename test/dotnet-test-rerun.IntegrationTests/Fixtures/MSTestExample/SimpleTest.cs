namespace MSTestExample;

[TestClass]
public class UnitTest1
{
    [DataTestMethod]
    [DataRow(1, 1)]
    [DataRow(2, 2)]
    [DataRow(3, 3)]
    [DataRow(4, 4)]
    public void SimpleNumberCompare(int number, int expectedNumber)
    {
        Assert.AreEqual(expectedNumber, number);
    }
    
    [TestMethod]
    public void SimpleStringCompare()
    {
        Assert.AreEqual("value", "value");
    }
}