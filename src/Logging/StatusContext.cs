namespace dotnet.test.rerun.Logging;

/// <summary>
/// Wrapper for the SpectreConsole StatusContext object so we do not pollute all classes with Spectre imports
/// </summary>
public class StatusContext
{
    private Spectre.Console.StatusContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatusContext"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public StatusContext(Spectre.Console.StatusContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// Statuses the specified status.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentNullException">context</exception>
    public StatusContext Status(string status)
    {
        if (context == null)
        {
            throw new ArgumentNullException("context");
        }

        context.Status = status;
        return this;
    }
}