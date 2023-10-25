using System.Text.RegularExpressions;

namespace dotnet.test.rerun.Extensions;

public static class StringExtensions
{
    public static string FetchDotNetVersion(this string path)
    {
        string pattern = @"(\\|/)(net|netcoreapp|netfx|netstandard)\d+(\.\d+)*";
        Match match = Regex.Match(path, pattern);
        int firstPositionAfterMatch = match.Index + match.Length;

        if (match.Success && 
            (path.IsTheEnd(firstPositionAfterMatch) || 
             path.NextPositionIsCharacter(firstPositionAfterMatch, '\\') ||
             path.NextPositionIsCharacter(firstPositionAfterMatch, '/')))
            return match.Value
                .Replace("\\", "")
                .Replace("/", "");

        return string.Empty;
    }

    private static bool IsTheEnd(this string value, int pos)
        => value.Length == pos;

    private static bool NextPositionIsCharacter(this string value, int pos, char character)
        => value.ElementAt(pos) == character;
}