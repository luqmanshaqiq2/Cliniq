using Cliniq.Services.DataServices.Exceptions.Base;

namespace Cliniq.Services.DataServices.Exceptions.MedicalRecord
{
    public class MedicalRecordException : DataServiceException
    {
        public MedicalRecordException() : base("An error occurred while processing the medical record.")
        {
        }

        public MedicalRecordException(string? message) : base(message)
        {
        }

        public MedicalRecordException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
