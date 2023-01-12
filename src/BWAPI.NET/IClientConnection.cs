using System;
using System.IO.MemoryMappedFiles;

namespace BWAPI.NET
{
    /// <summary>
    /// Client - Server connection abstraction
    /// </summary>
    interface IClientConnection
    {
        void Disconnect();

        MemoryMappedViewAccessor GetGameTableViewAccessor();

        MemoryMappedViewAccessor GetSharedMemoryViewAccessor(int serverProcID);

        void ConnectSharedLock(int serverProcID);

        void WaitForServerData();

        void SubmitClientData();
    }

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