using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace BWAPI.NET
{
    /// <summary>
    /// Default Windows BWAPI pipe connection with shared memory.
    /// </summary>
    internal class ClientConnectionW32 : IClientConnection
    {
        private FileStream _pipeFileStream;
        private MemoryMappedFile _gameTableMemoryMappedFile;
        private MemoryMappedFile _gameMemoryMappedFile;

        public void Disconnect()
        {
            try
            {
                _gameTableMemoryMappedFile?.Dispose();
                _gameMemoryMappedFile?.Dispose();
                _pipeFileStream?.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

            _gameTableMemoryMappedFile = null;
            _gameMemoryMappedFile = null;
            _pipeFileStream = null;
        }

        public MemoryMappedViewAccessor GetGameTableViewAccessor()
        {
            _gameTableMemoryMappedFile = MemoryMappedFile.OpenExisting("Local\\bwapi_shared_memory_game_list", MemoryMappedFileRights.ReadWrite, HandleInheritability.None);
            return _gameTableMemoryMappedFile.CreateViewAccessor(0, GameTable.Size, MemoryMappedFileAccess.ReadWrite);
        }

        public MemoryMappedViewAccessor GetSharedMemoryViewAccessor(int serverProcID)
        {
            string sharedMemoryName = "Local\\bwapi_shared_memory_" + serverProcID;

            try
            {
                _gameMemoryMappedFile = MemoryMappedFile.OpenExisting(sharedMemoryName, MemoryMappedFileRights.ReadWrite, HandleInheritability.None);
                return _gameMemoryMappedFile.CreateViewAccessor(0, ClientData.GameData_.Size, MemoryMappedFileAccess.ReadWrite);
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