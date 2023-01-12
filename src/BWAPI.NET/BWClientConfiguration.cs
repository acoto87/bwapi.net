namespace BWAPI.NET
{
    /// <summary>
    /// Configuration for constructing a BWClient.
    /// </summary>
    public struct BWClientConfiguration
    {
        public static readonly BWClientConfiguration Default = new BWClientConfiguration()
        {
            UnlimitedFrameZero = true,
            MaxFrameDurationMs = 40
        };

        /// <summary>
        /// Set to `true` for more explicit error messages (which might spam the terminal).
        /// </summary>
        public bool DebugConnection { get; set; }

        /// <summary>
        /// When true, restarts the client loop when a game ends, allowing the client to play multiple games without restarting.
        /// </summary>
        public bool AutoContinue { get; set; }

        /// <summary>
        /// Most bot tournaments allow bots to take an indefinite amount of time on frame #0 (the first frame of the game) to analyze the map and load data,
        /// as the bot has no prior access to BWAPI or game information.
        ///
        /// This flag indicates that taking arbitrarily long on frame zero is acceptable.
        /// Performance metrics omit the frame as an outlier.
        /// Asynchronous operation will block until the bot's event handlers are complete.
        /// </summary>
        public bool UnlimitedFrameZero { get; set; }

        /// <summary>
        /// The maximum amount of time the bot is supposed to spend on a single frame.
        /// In asynchronous mode, JBWAPI will attempt to let the bot use up to this much time to process all frames before returning control to BWAPI.
        /// In synchronous mode, JBWAPI is not empowered to prevent the bot to exceed this amount, but will record overruns in performance metrics.
        /// Real-time human play typically uses the "fastest" game speed, which has 42.86ms (42,860ns) between frames.
        /// </summary>
        public int MaxFrameDurationMs { get; set; }

        /// <summary>
        /// Toggles verbose logging, particularly of synchronization steps.
        /// </summary>
        public bool LogVerbosely { get; set; }
    }
}