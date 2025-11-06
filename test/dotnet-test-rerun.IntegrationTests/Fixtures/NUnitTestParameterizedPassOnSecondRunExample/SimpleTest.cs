namespace NUnitTestExample;

public class Tests
{
    private static readonly string runCounterFile = Path.Combine(Path.GetTempPath(), "nunit_parameterized_test_run_counter_2_2.txt");
    private static readonly string stringParamCounterFile = Path.Combine(Path.GetTempPath(), "nunit_string_param_counter.txt");
    
    [TestCase(1, 1)]
    [TestCase(2, 2)]
    [TestCase(3, 3)]
    [TestCase(4, 4)]
    public void DataDrivenTest(int value, int expected)
    {
        // Only the test case with value=2 fails on first run, passes on second run
        if (value == 2)
        {
            int runCount = 0;
            if (File.Exists(runCounterFile))
            {
                string content = File.ReadAllText(runCounterFile);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    runCount = int.Parse(content);
                }
            }
            runCount++;
            File.WriteAllText(runCounterFile, runCount.ToString());
            
            if (runCount == 1)
            {
                Assert.Fail("Intentional failure for test case with value=2 on first run");
            }
        }
        
        Assert.That(value, Is.EqualTo(expected));
    }
    
    [TestCase("Value A", "Type X", "Category 1")]
    [TestCase("Value B", "Type Y", "All pays")]
    [TestCase("Value C", "Type Z", "Employees in columns")]
    public void StringParamTest(string param1, string param2, string param3)
    {
        // Only the test case with param3="All pays" fails on first run, passes on second run
        if (param3 == "All pays")
        {
            int runCount = 0;
            if (File.Exists(stringParamCounterFile))
            {
                string content = File.ReadAllText(stringParamCounterFile);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    runCount = int.Parse(content);
                }
            }
            runCount++;
            File.WriteAllText(stringParamCounterFile, runCount.ToString());
            
            if (runCount == 1)
            {
                Assert.Fail("Intentional failure for test case with param3='All pays' on first run");
            }
        }
        
        Assert.That(param1, Is.Not.Null);
        Assert.That(param2, Is.Not.Null);
        Assert.That(param3, Is.Not.Null);
    }
    
    [Test]
    public void RegularTest()
    {
        // This test should always pass
        Assert.That(1, Is.EqualTo(1));
    }
}
