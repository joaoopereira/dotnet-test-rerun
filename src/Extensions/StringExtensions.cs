using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace dotnet.test.rerun.Extensions
{
    public static class StringExtensions
    {
        private static Regex frameworkRegex = new Regex("^(?:netstandard|netcoreapp|net)[1-9][0-9]?(?:\\.[0-9])?$|^(?:net48[0-9]?|net4[5-7][0-9]?|net[1-3]\\.[5-9])$", RegexOptions.Compiled,TimeSpan.FromMilliseconds(50));
        public static bool IsFramework(this string text)
        {
            return frameworkRegex.IsMatch(text);
        }
    }
}
