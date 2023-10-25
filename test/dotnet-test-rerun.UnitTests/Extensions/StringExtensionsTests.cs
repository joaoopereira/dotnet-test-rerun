using dotnet.test.rerun.Extensions;
using FluentAssertions;
using Xunit;

namespace dotnet_test_rerun.UnitTest.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData(@"c:\test\net11\debug\", "net11")]
    [InlineData(@"c:\test\net11\debug", "net11")]
    [InlineData(@"c:/test/net11/debug/", "net11")]
    [InlineData(@"c:\test\net20\debug\", "net20")]
    [InlineData(@"c:\test\net35\debug\", "net35")]
    [InlineData(@"c:\test\net40\debug\", "net40")]
    [InlineData(@"c:\test\net45\debug\", "net45")]
    [InlineData(@"c:\test\net451\debug\", "net451")]
    [InlineData(@"c:\test\net452\debug\", "net452")]
    [InlineData(@"c:\test\net46\debug\", "net46")]
    [InlineData(@"c:\test\net461\debug\", "net461")]
    [InlineData(@"c:\test\net47\debug\", "net47")]
    [InlineData(@"c:\test\net471\debug\", "net471")]
    [InlineData(@"c:\test\net472\debug\", "net472")]
    [InlineData(@"c:\test\net48\debug\", "net48")]
    [InlineData(@"c:\test\netcoreapp1.0\debug\", "netcoreapp1.0")]
    [InlineData(@"c:\test\netcoreapp1.1\debug\", "netcoreapp1.1")]
    [InlineData(@"c:\test\netcoreapp2.0\debug\", "netcoreapp2.0")]
    [InlineData(@"c:\test\netcoreapp2.1\debug\", "netcoreapp2.1")]
    [InlineData(@"c:\test\netcoreapp2.2\debug\", "netcoreapp2.2")]
    [InlineData(@"c:\test\netcoreapp3.0\debug\", "netcoreapp3.0")]
    [InlineData(@"c:\test\netcoreapp3.1\debug\", "netcoreapp3.1")]
    [InlineData(@"c:\test\net5.0\debug\", "net5.0")]
    [InlineData(@"c:\test\net6.0\debug\", "net6.0")]
    [InlineData(@"c:\test\net7.0\debug\", "net7.0")]
    [InlineData(@"c:\test\net8.0\debug\", "net8.0")]
    [InlineData(@"c:\test\netstandard1.0\debug\", "netstandard1.0")]
    [InlineData(@"c:\test\netstandard1.1\debug\", "netstandard1.1")]
    [InlineData(@"c:\test\netstandard1.2\debug\", "netstandard1.2")]
    [InlineData(@"c:\test\netstandard1.3\debug\", "netstandard1.3")]
    [InlineData(@"c:\test\netstandard1.4\debug\", "netstandard1.4")]
    [InlineData(@"c:\test\netstandard1.5\debug\", "netstandard1.5")]
    [InlineData(@"c:\test\netstandard1.6\debug\", "netstandard1.6")]
    [InlineData(@"c:\test\netstandard2.0\debug\", "netstandard2.0")]
    [InlineData(@"c:\test\netstandard2.1\debug\", "netstandard2.1")]
    [InlineData(@"c:\test\network\debug\", "")]
    [InlineData(@"c:\test\itsnet48\debug\", "")]
    [InlineData(@"c:\test\net48project\debug\", "")]
    public void ShouldIdentifyFrameworks(string text, string expectedFramework)
    {
        text.FetchDotNetVersion().Should().Be(expectedFramework);
    }
}