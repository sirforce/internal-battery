using System;
namespace UpDiddyApi.ApplicationCore.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException() : base() { }
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class MaximumReachedException : Exception
    {
        public MaximumReachedException() : base() { }
        public MaximumReachedException(string message) : base(message) { }
        public MaximumReachedException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}