public class ContentException : Exception
{
    public ContentException()
    {
    }

    public ContentException(string message)
        : base(message)
    {
    }

    public ContentException(string message, Exception inner)
        : base(message, inner)
    {
    }
}