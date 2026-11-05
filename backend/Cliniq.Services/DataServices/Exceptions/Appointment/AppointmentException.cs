using Cliniq.Services.DataServices.Exceptions.Base;

namespace Cliniq.Services.DataServices.Exceptions.Appointment
{
    public class AppointmentException : DataServiceException
    {
        public AppointmentException() : base("An error occurred while processing the appointment.")
        {
        }

        public AppointmentException(string? message) : base(message)
        {
        }

        public AppointmentException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
