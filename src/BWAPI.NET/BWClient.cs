using System;

namespace BWAPI.NET
{
    /// <summary>
    /// Client class to connect to the game with.
    /// </summary>
    public class BWClient
    {
        private static readonly int SupportedBWAPIVersion = 10003;

        private readonly IBWEventListener _eventListener;
        private BWClientConfiguration _configuration;
        private PerformanceMetrics _performanceMetrics;
        private Client _client;
        private ClientData _clientData;
        private Game _game;
        private bool _gameOver;

        public BWClient(IBWEventListener eventListener)
        {
            _eventListener = eventListener ?? throw new ArgumentNullException(nameof(eventListener));
        }

        /// <summary>
        /// Start the game with default settings.
        /// </summary>
        public void StartGame()
        {
            StartGame(BWClientConfiguration.Default);
        }

        /// <summary>
        /// Start the game.
        /// </summary>
        /// <param name="autoContinue">autoContinue automatically continue playing the next game(s). false by default</param>
        public void StartGame(bool autoContinue)
        {
            StartGame(new BWClientConfiguration
            {
                AutoContinue = autoContinue
            });
        }

        /// <summary>
        /// Start the game.
        /// </summary>
        /// <param name="gameConfiguration">gameConfiguration Settings for playing games with this client.</param>
        /// <exception cref="ArgumentException">Throws argument exception when gameConfiguration.AsyncUnsafe is true but gameConfiguration.Async is false.</exception>
        public void StartGame(BWClientConfiguration gameConfiguration)
        {
            _configuration = gameConfiguration;

            _performanceMetrics = new PerformanceMetrics(_configuration);

            _client ??= new Client(this);
            _client.Reconnect();

            _clientData = new ClientData(_client.GameViewAccessor);

            if (SupportedBWAPIVersion != _clientData.GameData.GetClientVersion())
            {
                Console.Error.WriteLine("Error: Client and Server are not compatible!");
                Console.Error.WriteLine("Client version: {0}", SupportedBWAPIVersion);
                Console.Error.WriteLine("Server version: {0}", _clientData.GameData.GetClientVersion());

                _client.Disconnect();
                return;
            }

            _game = new Game(_clientData);

            do
            {
                ClientData.GameData_ gameData = _clientData.GameData;

                while (!gameData.IsInGame())
                {
                    if (!_client.IsConnected)
                    {
                        return;
                    }

                    _client.SendFrameReceiveFrame();

                    if (gameData.IsInGame())
                    {
                        _performanceMetrics = new PerformanceMetrics(_configuration);
                        _gameOver = false;
                    }
                }

                while (gameData.IsInGame())
                {
                    Log("Main: onFrame synchronous start");
                    HandleEvents();
                    Log("Main: onFrame synchronous end");

                    _performanceMetrics.FlushSideEffects.Time(() => _game.sideEffects.FlushTo(gameData));
                    _performanceMetrics.FrameDurationReceiveToSend.StopTiming();

                    _client.SendFrameReceiveFrame();

                    if (!_client.IsConnected)
                    {
                        Console.WriteLine("Reconnecting...");
                        _client.Reconnect();
                    }
                }

                _gameOver = true;
            } while (_configuration.AutoContinue);
        }

        public void Log(string message)
        {
            if (_configuration.LogVerbosely)
            {
                Console.WriteLine(message);
            }
        }

        private void HandleEvents()
        {
            ClientData.GameData_ gameData = _clientData.GameData;

            // Populate _gameOver before invoking event handlers (in case the bot throws)
            for (int i = 0; i < gameData.GetEventCount(); i++)
            {
                _gameOver = _gameOver || gameData.GetEvents(i).GetEventType() == EventType.MatchEnd;
            }

            _performanceMetrics.BotResponse.TimeIf(!_gameOver && (gameData.GetFrameCount() > 0 || !_configuration.UnlimitedFrameZero), () =>
            {
                for (int i = 0; i < gameData.GetEventCount(); i++)
                {
                    HandleEvent(gameData.GetEvents(i));
                }
            });
        }

        private void HandleEvent(ClientData.Event e)
        {
            Unit u;
            int frames = _game.GetFrameCount();
            switch (e.GetEventType())
            {
                case EventType.MatchStart:
                    _game.Init();
                    _eventListener.OnStart();
                    break;
                case EventType.MatchEnd:
                    _eventListener.OnEnd(e.GetV1() != 0);
                    break;
                case EventType.MatchFrame:
                    _game.OnFrame(frames);
                    _eventListener.OnFrame();
                    break;
                case EventType.SendText:
                    _eventListener.OnSendText(_game.ClientData.GameData.GetEventStrings(e.GetV1()));
                    break;
                case EventType.ReceiveText:
                    _eventListener.OnReceiveText(_game.GetPlayer(e.GetV1()), _game.ClientData.GameData.GetEventStrings(e.GetV2()));
                    break;
                case EventType.PlayerLeft:
                    _eventListener.OnPlayerLeft(_game.GetPlayer(e.GetV1()));
                    break;
                case EventType.NukeDetect:
                    _eventListener.OnNukeDetect(new Position(e.GetV1(), e.GetV2()));
                    break;
                case EventType.SaveGame:
                    _eventListener.OnSaveGame(_game.ClientData.GameData.GetEventStrings(e.GetV1()));
                    break;
                case EventType.UnitDiscover:
                    _game.UnitCreate(e.GetV1());
                    u = _game.GetUnit(e.GetV1());
                    u.UpdatePosition(frames);
                    _eventListener.OnUnitDiscover(u);
                    break;
                case EventType.UnitEvade:
                    u = _game.GetUnit(e.GetV1());
                    u.UpdatePosition(frames);
                    _eventListener.OnUnitEvade(u);
                    break;
                case EventType.UnitShow:
                    _game.UnitShow(e.GetV1());
                    u = _game.GetUnit(e.GetV1());
                    u.UpdatePosition(frames);
                    _eventListener.OnUnitShow(u);
                    break;
                case EventType.UnitHide:
                    _game.UnitHide(e.GetV1());
                    u = _game.GetUnit(e.GetV1());
                    _eventListener.OnUnitHide(u);
                    break;
                case EventType.UnitCreate:
                    _game.UnitCreate(e.GetV1());
                    u = _game.GetUnit(e.GetV1());
                    u.UpdatePosition(frames);
                    _eventListener.OnUnitCreate(u);
                    break;
                case EventType.UnitDestroy:
                    _game.UnitHide(e.GetV1());
                    u = _game.GetUnit(e.GetV1());
                    _eventListener.OnUnitDestroy(u);
                    break;
                case EventType.UnitMorph:
                    u = _game.GetUnit(e.GetV1());
                    u.UpdatePosition(frames);
                    _eventListener.OnUnitMorph(u);
                    break;
                case EventType.UnitRenegade:
                    u = _game.GetUnit(e.GetV1());
                    _eventListener.OnUnitRenegade(u);
                    break;
                case EventType.UnitComplete:
                    _game.UnitCreate(e.GetV1());
                    u = _game.GetUnit(e.GetV1());
                    _eventListener.OnUnitComplete(u);
                    break;
            }
        }

        /// <summary>
        /// Get the <seealso cref="Game"/> instance of the currently running game.
        /// When running in asynchronous mode, this is the game from the bot's perspective, e.g. potentially a previous frame.
        /// </summary>
        /// <returns></returns>
        public Game Game
        {
            get => _game;
        }

        public PerformanceMetrics PerformanceMetrics
        {
            get => _performanceMetrics;
        }

        public BWClientConfiguration Configuration
        {
            get => _configuration;
        }

        public ClientData ClientData
        {
            get => _clientData;
        }

        public IBWEventListener EventListener
        {
            get => _eventListener;
        }

        /// <summary>
        /// Gets whether the current frame should be subject to timing.
        /// </summary>
        public bool DoTime
        {
            get => !_configuration.UnlimitedFrameZero || (_client.IsConnected && _clientData.GameData.GetFrameCount() > 0);
        }

        /// <summary>
        /// This tracks the size of the frame buffer except when the game is paused (which results in multiple frames arriving with the same count).
        /// </summary>
        /// <returns>The number of frames between the one exposed to the bot and the most recent received by JBWAPI.</returns>
        public int FramesBehind
        {
            get => _clientData != null ? Math.Max(0, _clientData.GameData.GetFrameCount() - _game.GetFrameCount()) : 0;
        }

        internal Client Client
        {
            get => _client;
        }
    }
}