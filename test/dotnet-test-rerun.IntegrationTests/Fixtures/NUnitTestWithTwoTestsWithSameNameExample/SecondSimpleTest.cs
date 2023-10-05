namespace NUnitTestExample;

public class SecondSimpleTest
{
    private int number = 2;
    
    [Test]
    public void SecondSimpleNumberCompare()
    {
        Assert.AreEqual(2, number);
    }
}