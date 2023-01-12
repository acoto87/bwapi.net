namespace BWAPI.NET
{
    /// <summary>
    /// Manages invocation of bot event handlers
    /// </summary>
    public class BotWrapper
    {
        private readonly BWClient _bwClient;
        private bool _gameOver;

        public BotWrapper(BWClient bwClient)
        {
            _bwClient = bwClient;
        }

        /// <summary>
        /// Resets the BotWrapper for a new botGame.
        /// </summary>
        public void StartNewGame()
        {
            _gameOver = false;
        }

        /// <summary>
        /// Handles the arrival of a new frame from BWAPI
        /// </summary>
        public void OnFrame()
        {
            _bwClient.Log("Main: onFrame synchronous start");
            HandleEvents();
            _bwClient.Log("Main: onFrame synchronous end");
        }

        /// <summary>
        /// Allows an asynchronous bot time to finish operation
        /// </summary>
        public void EndGame()
        {
            _gameOver = true;
        }

        private void HandleEvents()
        {
            ClientData.TGameData gameData = _bwClient.ClientData.GameData;

            // Populate gameOver before invoking event handlers (in case the bot throws)
            for (int i = 0; i < gameData.GetEventCount(); i++)
            {
                _gameOver = _gameOver || gameData.GetEvents(i).GetEventType() == EventType.MatchEnd;
            }

            _bwClient.PerformanceMetrics.BotResponse.TimeIf(!_gameOver && (gameData.GetFrameCount() > 0 || !_bwClient.Configuration.UnlimitedFrameZero), () =>
            {
                for (int i = 0; i < gameData.GetEventCount(); i++)
                {
                    EventHandler.Operation(_bwClient.EventListener, _bwClient.Game, gameData.GetEvents(i));
                }
            });
        }
    }
}