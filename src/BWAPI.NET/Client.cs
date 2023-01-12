using System;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace BWAPI.NET
{
    public class Client
    {
        private readonly BWClient _bwClient;
        private readonly IClientConnection _clientConnector;
        private MemoryMappedViewAccessor _gameTableViewAccessor;
        private MemoryMappedViewAccessor _gameViewAccessor;
        private bool _connected;

        public Client(BWClient bwClient)
        {
            _bwClient = bwClient;
            _clientConnector = new ClientConnectionW32();
        }

        public bool Connect()
        {
            if (_connected)
            {
                Console.Error.WriteLine("Already connected");
                return true;
            }

            try
            {
                _gameTableViewAccessor = _clientConnector.GetGameTableViewAccessor();
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
                _gameViewAccessor = _clientConnector.GetSharedMemoryViewAccessor(serverProcID);
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
                _clientConnector.ConnectSharedLock(serverProcID);
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
                _clientConnector.WaitForServerData();
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
            _connected = true;
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
                Console.Error.Write("Disconnect called by: ");
                Console.Error.WriteLine(Environment.StackTrace);
            }

            if (!_connected)
            {
                return;
            }

            _clientConnector.Disconnect();

            _gameViewAccessor.Dispose();
            _gameViewAccessor = null;

            _gameTableViewAccessor.Dispose();
            _gameTableViewAccessor = null;

            _connected = false;
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
                _clientConnector.SubmitClientData();
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
                _clientConnector.WaitForServerData();
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

        public bool IsConnected
        {
            get => _connected;
        }

        public MemoryMappedViewAccessor GameTableViewAccessor
        {
            get => _gameTableViewAccessor;
        }

        public MemoryMappedViewAccessor GameViewAccessor
        {
            get => _gameViewAccessor;
        }
    }
}