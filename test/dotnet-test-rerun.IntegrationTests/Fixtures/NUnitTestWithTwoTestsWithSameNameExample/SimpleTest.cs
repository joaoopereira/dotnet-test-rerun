namespace NUnitTestExample;

public class SimpleTest
{
    private int number = 2;
    
    [Test, Order(1)]
    public void SimpleNumberCompare()
    {
        number = 3;
        Assert.AreEqual(3, number);
    }
    
    [Test, Order(2)]
    public void SecondSimpleNumberCompare()
    {
        Assert.AreEqual(2, number);
    }
}