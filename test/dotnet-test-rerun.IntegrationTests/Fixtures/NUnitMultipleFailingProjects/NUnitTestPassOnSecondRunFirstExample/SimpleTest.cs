namespace NUnitTestPassOnSecondRunFirstExample;


public class Tests
{
    private int number = 2;
    
    [Test, Order(1)]
    public void NUnitTestPassOnSecondRunFirstExample_FirstNumberCompare()
    {
        number = 3;
        Assert.AreEqual(3, number);
    }
    
    [Test, Order(2)]
    public void NUnitTestPassOnSecondRunFirstExample_SecondNumberCompare()
    {
        Assert.AreEqual(2, number);
    }
}