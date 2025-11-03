namespace NUnitTestExample;

public class Tests
{
    private static readonly string runCounterFile = Path.Combine(Path.GetTempPath(), "nunit_parameterized_test_run_counter_2_2.txt");
    
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
    
    [Test]
    public void RegularTest()
    {
        // This test should always pass
        Assert.That(1, Is.EqualTo(1));
    }
}
