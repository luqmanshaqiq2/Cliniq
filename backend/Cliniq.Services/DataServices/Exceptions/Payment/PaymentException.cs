using Cliniq.Services.DataServices.Exceptions.Base;

namespace Cliniq.Services.DataServices.Exceptions.Payment
{
    public class PaymentException : DataServiceException
    {
        public PaymentException() : base("An error occurred while processing the payment.")
        {
        }

        public PaymentException(string? message) : base(message)
        {
        }

        public PaymentException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
