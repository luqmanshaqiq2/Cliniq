namespace Cliniq.API.Exceptions;

public class BaseWebException : Exception
{
    public BaseWebException(
        string message,
        int statusCode,
        string errorCode,
        Exception? innerException = null)
        : base(message, innerException)
    {
        if (statusCode is < 400 or > 599)
        {
            throw new ArgumentOutOfRangeException(nameof(statusCode));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }

    public int StatusCode { get; }

    public string ErrorCode { get; }
}
