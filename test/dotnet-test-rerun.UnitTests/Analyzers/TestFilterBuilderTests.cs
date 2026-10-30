using dotnet.test.rerun.Analyzers.Filters;
using AwesomeAssertions;
using Xunit;

namespace dotnet_test_rerun.UnitTest.Analyzers;

public class TestFilterBuilderTests
{
    private const string NUnit = "executor://nunit3testexecutor/";
    private const string XUnit = "executor://xunit/VsTestRunner2/netcoreapp";
    private const string MSTest = "executor://mstestadapter/v2";

    private readonly TestFilterBuilder Builder = new();

    [Theory]
    // no parameters
    [InlineData(NUnit, "Ns.Class.Method", "Method", @"FullyQualifiedName=Ns.Class.Method")]
    [InlineData(XUnit, "Ns.Class.Method", "Ns.Class.Method", @"FullyQualifiedName=Ns.Class.Method")]
    [InlineData(MSTest, "Ns.Class.Method", "Method", @"FullyQualifiedName=Ns.Class.Method")]
    // numeric parameters
    [InlineData(NUnit, "Ns.Class.Method(1,2)", "Method(1,2)", @"FullyQualifiedName~Ns.Class.Method\(1,2\)")]
    [InlineData(XUnit, "Ns.Class.Method", "Ns.Class.Method(a: 1, b: 2)", @"FullyQualifiedName=Ns.Class.Method&DisplayName~a: 1, b: 2")]
    [InlineData(MSTest, "Ns.Class.Method", "Method (1,2)", @"FullyQualifiedName=Ns.Class.Method")]
    // string parameters with spaces and quotes
    [InlineData(NUnit, "Tests.Addresses.TestAddress(\"20a Manor Gardens NE10 8UZ\")", "TestAddress(\"20a Manor Gardens NE10 8UZ\")",
        @"FullyQualifiedName~Tests.Addresses.TestAddress\(%2220a Manor Gardens NE10 8UZ%22\)")]
    [InlineData(XUnit, "Ns.Class.Method", "Ns.Class.Method(value: \"a b\")", @"FullyQualifiedName=Ns.Class.Method&DisplayName~value: %22a b%22")]
    // empty parentheses
    [InlineData(NUnit, "Ns.Class.Method()", "Method()", @"FullyQualifiedName=Ns.Class.Method")]
    [InlineData(XUnit, "Ns.Class.Method", "Ns.Class.Method()", @"FullyQualifiedName=Ns.Class.Method")]
    // names with spaces
    [InlineData(XUnit, "Ns.Class.My Method", "Ns.Class.My Method", @"FullyQualifiedName=Ns.Class.My\ Method")]
    // generic methods
    [InlineData(XUnit, "Ns.Class.Method<T>", "Ns.Class.Method<T>(value: 1)", @"FullyQualifiedName=Ns.Class.Method<T>&DisplayName~value: 1")]
    // nested classes
    [InlineData(NUnit, "Ns.Outer+Inner.Method", "Method", @"FullyQualifiedName=Ns.Outer+Inner.Method")]
    // unknown framework falls back to the default strategy
    [InlineData("executor://other", "Ns.Class.Method", "Ns.Class.Method(1)", @"FullyQualifiedName=Ns.Class.Method&DisplayName~1")]
    public void BuildFilter(string adapter, string fullyQualifiedName, string testName, string expected)
        => Builder.BuildFilter(new FailedTest(fullyQualifiedName, testName, adapter)).Should().Be(expected);

    [Fact]
    public void ClassFilter_EscapesSpecialCharacters()
        => VsTestFilter.ClassFilter("Ns.Class{T}").Should().Be(@"FullyQualifiedName~Ns.Class\{T\}");
}
