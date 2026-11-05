using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cliniq.DataAccess.Exceptions
{
    public class UnknownDatabaseException : ApplicationException
    {
        public UnknownDatabaseException()
        {
        }

        public UnknownDatabaseException(string? message) : base(message)
        {
        }

        public UnknownDatabaseException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}