namespace dotnet.test.rerun.Enums;

public enum RerunOutcome
{
    /// <summary>All tests passed on the first run.</summary>
    Passed,
    /// <summary>Some tests failed, then passed when rerun.</summary>
    PassedAfterRerun,
    /// <summary>Tests still fail after all attempts, or failed without failed tests to rerun.</summary>
    Failed,
    /// <summary>Too many tests failed to rerun them (--rerunMaxFailedTests / --rerunFailedThreshold).</summary>
    ThresholdExceeded,
    /// <summary>Tests failed but no TRX file was found to know which ones.</summary>
    NoTestResults
}
