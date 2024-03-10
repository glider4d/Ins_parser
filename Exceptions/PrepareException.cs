public class PrepareException : Exception
{
    public PrepareException()
    {
    }

    public PrepareException(string message)
        : base(message)
    {
    }

    public PrepareException(string message, Exception inner)
        : base(message, inner)
    {
    }
}