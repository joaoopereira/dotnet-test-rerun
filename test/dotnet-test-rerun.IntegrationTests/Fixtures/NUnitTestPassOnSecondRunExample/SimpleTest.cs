namespace NUnitTestExample;

public class Tests
{
    private int number = 2;
    
    [Test, Order(1)]
    public void SimpleNumberCompare()
    {
        number = 3;
        Assert.That(number, Is.EqualTo(3));
    }
    
    [Test, Order(2)]
    public void SecondSimpleNumberCompare()
    {
        Assert.That(number, Is.EqualTo(2));
    }
}