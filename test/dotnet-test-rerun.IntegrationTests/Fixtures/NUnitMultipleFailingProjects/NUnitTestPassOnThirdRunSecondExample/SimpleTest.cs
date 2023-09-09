namespace NUnitTestPassOnSecondRunSecondExample;


public class Tests
{
    private int firstNumber = 2;
    private int secondNumber = 2;
    
    [Test, Order(1)]
    public void NUnitTestPassOnThirdRunSecondExample_FirstNumberCompare()
    {
        firstNumber = 3;
        Assert.AreEqual(3, firstNumber);
    }
    
    [Test, Order(2)]
    public void NUnitTestPassOnThirdRunSecondExample_SecondSimpleNumberCompare()
    {
        secondNumber = 4;
        Assert.AreEqual(2, firstNumber);
        Assert.AreEqual(4, secondNumber);
    }
    
    [Test, Order(3)]
    public void NUnitTestPassOnThirdRunSecondExample_ThirdSimpleNumberCompare()
    {
        Assert.AreEqual(2, secondNumber);
    }
}