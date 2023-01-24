using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace BWAPI.NET
{
    internal class Client
    {
        private readonly BWClient _bwClient;
        private MemoryMappedFile _gameTableMemoryMappedFile;
        private MemoryMappedFile _gameMemoryMappedFile;
        private FileStream _pipeFileStream;
        private MemoryMappedViewAccessor _gameTableViewAccessor;
        private MemoryMappedViewAccessor _gameViewAccessor;
        private MemoryMappedViewStream _gameTableViewStream;
        private MemoryMappedViewStream _gameViewStream;
        private bool _isConnected;

        public Client(BWClient bwClient)
        {
            _bwClient = bwClient;
        }

        public bool Connect()
        {
            if (_isConnected)
            {
                Console.WriteLine("Already connected");
                return true;
            }

            try
            {
                _gameTableViewAccessor = GetGameTableViewAccessor();
                _gameTableViewStream = GetGameTableViewStream();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Game table mapping not found.");

                if (_bwClient.Configuration.DebugConnection)
                {
                    Console.Error.WriteLine(e.Message);
                    Console.Error.WriteLine(e.StackTrace);
                }

                return false;
            }

            GameTable gameTable;
            try
            {
                gameTable = new GameTable(_gameTableViewAccessor);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Unable to map Game table.");

                if (_bwClient.Configuration.DebugConnection)
                {
                    Console.Error.WriteLine(e.Message);
                    Console.Error.WriteLine(e.StackTrace);
                }

                return false;
            }

            int serverProcID = -1;
            int gameTableIndex = -1;

            int oldest = int.MaxValue;
            for (int i = 0; i < GameTable.MaxGameInstances; i++)
            {
                GameInstance gameInstance = gameTable.GameInstances[i];
                Console.WriteLine("{0} | {1} | {2} | {3}", i, gameInstance.ServerProcessID, gameInstance.IsConnected, gameInstance.LastKeepAliveTime);
                if (gameInstance.ServerProcessID != 0 && !gameInstance.IsConnected)
                {
                    if (gameTableIndex == -1 || gameInstance.LastKeepAliveTime < oldest)
                    {
                        oldest = gameInstance.LastKeepAliveTime;
                        gameTableIndex = i;
                    }
                }
            }

            if (gameTableIndex != -1)
            {
                serverProcID = gameTable.GameInstances[gameTableIndex].ServerProcessID;
            }

            if (serverProcID == -1)
            {
                Console.Error.WriteLine("No server proc ID");
                return false;
            }

            try
            {
                _gameViewAccessor = GetGameViewAccessor(serverProcID);
                _gameViewStream = GetGameViewStream(serverProcID);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Unable to open shared memory mapping: " + e.Message);

                if (_bwClient.Configuration.DebugConnection)
                {
                    Console.Error.WriteLine(e.Message);
                    Console.Error.WriteLine(e.StackTrace);
                }

                _gameTableViewAccessor.Dispose();
                _gameTableViewAccessor = null;
                return false;
            }

            try
            {
                ConnectSharedLock(serverProcID);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Unable to map pipe file stream.");

                if (_bwClient.Configuration.DebugConnection)
                {
                    Console.Error.WriteLine(e.Message);
                    Console.Error.WriteLine(e.StackTrace);
                }

                _gameViewAccessor.Dispose();
                _gameViewAccessor = null;

                _gameTableViewAccessor.Dispose();
                _gameTableViewAccessor = null;

                return false;
            }

            Console.WriteLine("Connected");

            try
            {
                WaitForServerData();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("There was an error waiting for server data.");

                if (_bwClient.Configuration.DebugConnection)
                {
                    Console.Error.WriteLine(e.Message);
                    Console.Error.WriteLine(e.StackTrace);
                }

                Disconnect();
                return false;
            }

            Console.WriteLine("Connection successful");
            _isConnected = true;
            return true;
        }

        public void Reconnect()
        {
            while (!Connect())
            {
                Thread.Sleep(1000);
            }
        }

        public void Disconnect()
        {
            if (_bwClient.Configuration.DebugConnection)
            {
                Console.Write("Disconnect called by: ");
                Console.WriteLine(Environment.StackTrace);
            }

            if (!_isConnected)
            {
                return;
            }

            _gameViewAccessor?.Dispose();
            _gameTableViewAccessor?.Dispose();
            _gameViewStream?.Dispose();
            _gameTableViewStream?.Dispose();
            _gameTableMemoryMappedFile?.Dispose();
            _gameMemoryMappedFile?.Dispose();
            _pipeFileStream?.Dispose();

            _gameViewAccessor = null;
            _gameTableViewAccessor = null;
            _gameViewStream = null;
            _gameTableViewStream = null;
            _gameTableMemoryMappedFile = null;
            _gameMemoryMappedFile = null;
            _pipeFileStream = null;

            _isConnected = false;
        }

        public void SendFrameReceiveFrame()
        {
            PerformanceMetrics metrics = _bwClient.PerformanceMetrics;

            // Tell BWAPI that we are done with the current frame
            metrics.FrameDurationReceiveToSend.StopTiming();

            if (_bwClient.DoTime)
            {
                metrics.CommunicationSendToReceive.StartTiming();
                metrics.CommunicationSendToSent.StartTiming();
            }

            try
            {
                SubmitClientData();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("failed, disconnecting");

                if (_bwClient.Configuration.DebugConnection)
                {
                    Console.Error.WriteLine(e.Message);
                    Console.Error.WriteLine(e.StackTrace);
                }

                Disconnect();
            }

            metrics.CommunicationSendToSent.StopTiming();
            metrics.FrameDurationReceiveToSent.StopTiming();

            if (_bwClient.DoTime)
            {
                int eventCount = _bwClient.ClientData.GameData.GetEventCount();
                metrics.NumberOfEvents.Record(eventCount);
                metrics.NumberOfEventsTimesDurationReceiveToSent.Record(eventCount * metrics.FrameDurationReceiveToSent.RunningTotal.Last);
            }

            // Listen for BWAPI to indicate that a new frame is ready
            if (_bwClient.DoTime)
            {
                metrics.CommunicationListenToReceive.StartTiming();
            }

            try
            {
                WaitForServerData();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("failed, disconnecting");

                if (_bwClient.Configuration.DebugConnection)
                {
                    Console.Error.WriteLine(e.Message);
                    Console.Error.WriteLine(e.StackTrace);
                }

                Disconnect();
                return;
            }

            metrics.CommunicationListenToReceive.StopTiming();
            metrics.CommunicationSendToReceive.StopTiming();

            if (_bwClient.DoTime)
            {
                metrics.FrameDurationReceiveToSend.StartTiming();
                metrics.FrameDurationReceiveToSent.StartTiming();
            }

            metrics.FrameDurationReceiveToReceive.StopTiming();

            if (_bwClient.DoTime)
            {
                metrics.FrameDurationReceiveToReceive.StartTiming();
            }
        }

        private MemoryMappedViewAccessor GetGameTableViewAccessor()
        {
            _gameTableMemoryMappedFile ??= MemoryMappedFile.OpenExisting("Local\\bwapi_shared_memory_game_list", MemoryMappedFileRights.ReadWrite, HandleInheritability.None);
            return _gameTableMemoryMappedFile.CreateViewAccessor(0, GameTable.Size, MemoryMappedFileAccess.ReadWrite);
        }

        private MemoryMappedViewAccessor GetGameViewAccessor(int serverProcID)
        {
            string sharedMemoryName = "Local\\bwapi_shared_memory_" + serverProcID;

            try
            {
                _gameMemoryMappedFile ??= MemoryMappedFile.OpenExisting(sharedMemoryName, MemoryMappedFileRights.ReadWrite, HandleInheritability.None);
                return _gameMemoryMappedFile.CreateViewAccessor(0, ClientData.GameData_.Size, MemoryMappedFileAccess.ReadWrite);
            }
            catch (Exception e)
            {
                throw new SharedMemoryConnectionException(sharedMemoryName, e);
            }
        }

        private MemoryMappedViewStream GetGameTableViewStream()
        {
            _gameTableMemoryMappedFile ??= MemoryMappedFile.OpenExisting("Local\\bwapi_shared_memory_game_list", MemoryMappedFileRights.ReadWrite, HandleInheritability.None);
            return _gameTableMemoryMappedFile.CreateViewStream(0, GameTable.Size, MemoryMappedFileAccess.ReadWrite);
        }

        private MemoryMappedViewStream GetGameViewStream(int serverProcID)
        {
            string sharedMemoryName = "Local\\bwapi_shared_memory_" + serverProcID;

            try
            {
                _gameMemoryMappedFile ??= MemoryMappedFile.OpenExisting(sharedMemoryName, MemoryMappedFileRights.ReadWrite, HandleInheritability.None);
                return _gameMemoryMappedFile.CreateViewStream(0, ClientData.GameData_.Size, MemoryMappedFileAccess.ReadWrite);
            }
            catch (Exception e)
            {
                throw new SharedMemoryConnectionException(sharedMemoryName, e);
            }
        }

        private void ConnectSharedLock(int serverProcID)
        {
            string communicationPipe = "\\\\.\\pipe\\bwapi_pipe_" + serverProcID;

            try
            {
                _pipeFileStream ??= new FileStream(communicationPipe, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 0);
            }
            catch (FileNotFoundException e)
            {
                throw new SharedLockConnectionException("Unable to open communications pipe: " + communicationPipe, e);
            }
        }

        private void WaitForServerData()
        {
            while (_pipeFileStream.ReadByte() != 2) { }
        }

        private void SubmitClientData()
        {
            _pipeFileStream.WriteByte(1);
        }

        public bool IsConnected
        {
            get => _isConnected;
        }

        public MemoryMappedViewAccessor GameTableViewAccessor
        {
            get => _gameTableViewAccessor;
        }

        public MemoryMappedViewAccessor GameViewAccessor
        {
            get => _gameViewAccessor;
        }

        public MemoryMappedViewStream GameTableViewStream
        {
            get => _gameTableViewStream;
        }

        public MemoryMappedViewStream GameViewStream
        {
            get => _gameViewStream;
        }
    }
}