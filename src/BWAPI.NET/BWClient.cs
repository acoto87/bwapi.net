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
        private BotWrapper _botWrapper;
        private Client _client;
        private ClientData _clientData;
        private Game _game;

        public BWClient(IBWEventListener eventListener)
        {
            if (eventListener == null)
            {
                throw new ArgumentNullException(nameof(eventListener));
            }

            _eventListener = eventListener;
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

            _botWrapper = new BotWrapper(this);
            _game = new Game(_clientData);

            do
            {
                ClientData.TGameData gameData = _clientData.GameData;
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
                        _botWrapper.StartNewGame();
                    }
                }
                while (gameData.IsInGame())
                {
                    _botWrapper.OnFrame();
                    _performanceMetrics.FlushSideEffects.Time(() => _game.sideEffects.FlushTo(gameData));
                    _performanceMetrics.FrameDurationReceiveToSend.StopTiming();

                    _client.SendFrameReceiveFrame();
                    if (!_client.IsConnected)
                    {
                        Console.WriteLine("Reconnecting...");
                        _client.Reconnect();
                    }
                }
                _botWrapper.EndGame();
            } while (_configuration.AutoContinue);
        }

        public void Log(string message)
        {
            if (_configuration.LogVerbosely)
            {
                Console.WriteLine(message);
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
            get => _botWrapper == null ? 0 : Math.Max(0, _clientData.GameData.GetFrameCount() - _game.GetFrameCount());
        }
    }
}