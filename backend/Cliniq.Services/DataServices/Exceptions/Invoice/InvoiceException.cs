using Cliniq.Services.DataServices.Exceptions.Base;

namespace Cliniq.Services.DataServices.Exceptions.Invoice
{
    public class InvoiceException : DataServiceException
    {
        public InvoiceException() : base("An error occurred while processing the invoice.")
        {
        }

        public InvoiceException(string? message) : base(message)
        {
        }

        public InvoiceException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
