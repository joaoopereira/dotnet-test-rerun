namespace dotnet.test.rerun.Execution;

public static class FilterBatches
{
    /// <summary>
    /// Keeps each rerun filter well under the Windows command-line limit (32,767 characters)
    /// and the sizes VSTest parses quickly.
    /// </summary>
    public const int MaxFilterLength = 8000;

    /// <summary>
    /// Joins the filter clauses with " | " into as few filters as possible, each at most
    /// <paramref name="maxLength"/> characters. A single clause longer than that gets its own filter.
    /// </summary>
    public static IReadOnlyList<string> Split(IEnumerable<string> clauses, int maxLength = MaxFilterLength)
    {
        const string separator = " | ";
        var batches = new List<string>();
        var current = new List<string>();
        var currentLength = 0;

        foreach (var clause in clauses)
        {
            var addedLength = current.Count == 0 ? clause.Length : separator.Length + clause.Length;
            if (current.Count > 0 && currentLength + addedLength > maxLength)
            {
                batches.Add(string.Join(separator, current));
                current.Clear();
                currentLength = 0;
                addedLength = clause.Length;
            }

            current.Add(clause);
            currentLength += addedLength;
        }

        if (current.Count > 0)
            batches.Add(string.Join(separator, current));

        return batches;
    }
}
