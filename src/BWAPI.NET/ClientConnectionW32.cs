using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace BWAPI.NET
{
    /// <summary>
    /// Default Windows BWAPI pipe connection with shared memory.
    /// </summary>
    class ClientConnectionW32 : IClientConnection
    {
        private FileStream _pipeFileStream;

        public void Disconnect()
        {
            if (_pipeFileStream != null)
            {
                try
                {
                    _pipeFileStream.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }

                _pipeFileStream = null;
            }
        }

        public MemoryMappedViewAccessor GetGameTableViewAccessor()
        {
            var sharedMemoryMappedFile = MemoryMappedFile.OpenExisting("Local\\bwapi_shared_memory_game_list", MemoryMappedFileRights.ReadWrite, HandleInheritability.None);
            return sharedMemoryMappedFile.CreateViewAccessor(0, GameTable.Size, MemoryMappedFileAccess.ReadWrite);
        }

        public MemoryMappedViewAccessor GetSharedMemoryViewAccessor(int serverProcID)
        {
            string sharedMemoryName = "Local\\bwapi_shared_memory_" + serverProcID;

            try
            {
                // TODO: Dipose gameTable?
                var sharedMemoryMappedFile = MemoryMappedFile.OpenExisting(sharedMemoryName, MemoryMappedFileRights.ReadWrite, HandleInheritability.None);
                return sharedMemoryMappedFile.CreateViewAccessor(0, ClientData.TGameData.Size, MemoryMappedFileAccess.ReadWrite);
            }
            catch (Exception e)
            {
                throw new SharedMemoryConnectionException(sharedMemoryName, e);
            }
        }

        public void ConnectSharedLock(int serverProcID)
        {
            string communicationPipe = "\\\\.\\pipe\\bwapi_pipe_" + serverProcID;

            try
            {
                _pipeFileStream = new FileStream(communicationPipe, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 0);
            }
            catch (FileNotFoundException e)
            {
                throw new SharedLockConnectionException("Unable to open communications pipe: " + communicationPipe, e);
            }
        }

        public void WaitForServerData()
        {
            while (_pipeFileStream.ReadByte() != 2) { }
        }

        public void SubmitClientData()
        {
            _pipeFileStream.WriteByte(1);
        }
    }
}