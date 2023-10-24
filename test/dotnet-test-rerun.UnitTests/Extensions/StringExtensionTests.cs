using dotnet.test.rerun.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace dotnet_test_rerun.UnitTest.Extensions
{
    
    public class StringExtensionTests
    {
        [Theory]
        [InlineData("net11", true)]
        [InlineData("net20", true)]
        [InlineData("net35", true)]
        [InlineData("net40", true)]
        [InlineData("net45", true)]
        [InlineData("net451", true)]
        [InlineData("net452", true)]
        [InlineData("net46", true)]
        [InlineData("net461", true)]
        [InlineData("net462", true)]
        [InlineData("net47", true)]
        [InlineData("net471", true)]
        [InlineData("net472", true)]
        [InlineData("net48", true)]
        [InlineData("netcoreapp1.0", true)]
        [InlineData("netcoreapp1.1", true)]
        [InlineData("netcoreapp2.0", true)]
        [InlineData("netcoreapp2.1", true)]
        [InlineData("netcoreapp2.2", true)]
        [InlineData("netcoreapp3.0", true)]
        [InlineData("netcoreapp3.1", true)]
        [InlineData("net5.0", true)]
        [InlineData("net6.0", true)]
        [InlineData("net7.0", true)]
        [InlineData("net8.0", true)]
        [InlineData("netstandard1.0", true)]
        [InlineData("netstandard1.1", true)]
        [InlineData("netstandard1.2", true)]
        [InlineData("netstandard1.3", true)]
        [InlineData("netstandard1.4", true)]
        [InlineData("netstandard1.5", true)]
        [InlineData("netstandard1.6", true)]
        [InlineData("netstandard2.0", true)]
        [InlineData("netstandard2.1", true)]
        [InlineData("network", false)]
        [InlineData("itsnet48", false)]
        [InlineData("net48project", false)]
        public void ShouldIdentifyFrameworks(string text, bool isFramework)
        {
            Assert.Equal(isFramework, text.IsFramework());
        }
    }
}
