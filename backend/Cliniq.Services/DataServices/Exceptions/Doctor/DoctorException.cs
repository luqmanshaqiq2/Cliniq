using Cliniq.Services.DataServices.Exceptions.Base;

namespace Cliniq.Services.DataServices.Exceptions.Doctor
{
    public class DoctorException : DataServiceException
    {
        public DoctorException() : base("An error occurred while processing the doctor.")
        {
        }

        public DoctorException(string? message) : base(message)
        {
        }

        public DoctorException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
