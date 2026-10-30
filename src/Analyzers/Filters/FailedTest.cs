namespace dotnet.test.rerun.Analyzers.Filters;

/// <summary>
/// A failed test as read from a TRX file.
/// </summary>
/// <param name="FullyQualifiedName">ClassName.MethodName from the test definition (NUnit and MSTest include
/// parameters, xUnit does not), or the result's test name when there is no definition.</param>
/// <param name="TestName">The test name of the result, which includes parameters for parameterised tests.</param>
/// <param name="AdapterTypeName">The executor that ran the test, which identifies the test framework.</param>
public sealed record FailedTest(string FullyQualifiedName, string TestName, string AdapterTypeName);
