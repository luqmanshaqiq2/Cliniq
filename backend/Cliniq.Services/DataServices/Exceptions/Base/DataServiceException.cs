using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cliniq.Services.DataServices.Exceptions.Base
{
    public class DataServiceException : ApplicationException
    {
         public DataServiceException() : this(string.Empty)
        {
        }

        public DataServiceException(string? message) : this(message, null)
        {
        }

        public DataServiceException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

    }
}