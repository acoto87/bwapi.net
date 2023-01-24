using System;

namespace BWAPI.NET
{
    public class SharedLockConnectionException : Exception
    {
        public SharedLockConnectionException(string message)
            : base(message)
        {
        }

        public SharedLockConnectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}