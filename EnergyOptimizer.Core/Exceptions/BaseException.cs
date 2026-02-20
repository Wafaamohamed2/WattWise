using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Exceptions
{
    public class BaseException : Exception
    {
        public int StatusCode { get; }
        public BaseException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public class NotFoundException : BaseException
    {
        public NotFoundException(string message) : base(message, 404) { }
    }

    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message) : base(message, 401) { }
    }

    public class BadRequestException : BaseException
    {
        public BadRequestException(string message) : base(message, 400) { }
    }
}