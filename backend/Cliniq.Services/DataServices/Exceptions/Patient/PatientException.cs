using Cliniq.Services.DataServices.Exceptions.Base;

namespace Cliniq.Services.DataServices.Exceptions.Patient
{
    public class PatientException : DataServiceException
    {
        public PatientException() : base("An error occurred while processing the patient.")
        {
        }

        public PatientException(string? message) : base(message)
        {
        }

        public PatientException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
