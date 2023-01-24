using System;

namespace BWAPI.NET
{
    public class SharedMemoryConnectionException : Exception
    {
        public SharedMemoryConnectionException(string message)
            : base(message)
        {
        }

        public SharedMemoryConnectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}