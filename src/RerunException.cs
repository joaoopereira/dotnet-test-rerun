namespace dotnet.test.rerun
{
    public class RerunException : Exception
    {
        public RerunException()
        { }

        public RerunException(string? message) : base(message)
        {
        }

        public RerunException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}