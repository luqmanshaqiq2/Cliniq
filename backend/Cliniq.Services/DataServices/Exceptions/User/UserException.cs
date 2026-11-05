using Cliniq.Services.DataServices.Exceptions.Base;

namespace Cliniq.Services.DataServices.Exceptions.User
{
    public class UserException : DataServiceException
    {
        public UserException() : base("An error occurred while processing the user.")
        {
        }

        public UserException(string? message) : base(message)
        {
        }

        public UserException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
