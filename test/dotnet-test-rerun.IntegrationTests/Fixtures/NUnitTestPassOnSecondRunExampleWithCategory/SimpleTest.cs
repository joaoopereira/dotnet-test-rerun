using NUnit.Framework;

namespace NUnitTestExample;

public class Tests
{
    private int number = 2;
    
    [Test, Order(1)]
    [Category("FirstCategory")]
    public void SimpleNumberCompare()
    {
        number = 3;
        Assert.AreEqual(3, number);
    }
    
    [Test, Order(2)]
    [Category("SecondCategory")]
    public void SecondSimpleNumberCompare()
    {
        Assert.AreEqual(2, number);
    }
}