using System.Collections.Generic;
using System.Text;

namespace BWAPI.NET
{
    /**
     * Collects various performance metrics.
     */
    public class PerformanceMetrics
    {
        private BWClientConfiguration _configuration;
        private readonly List<PerformanceMetric> _performanceMetrics = new List<PerformanceMetric>();

        public PerformanceMetrics(BWClientConfiguration configuration)
        {
            _configuration = configuration;

            Reset();
        }

        /**
         * Clears all tracked data and starts counting from a blank slate.
         */
        public void Reset()
        {
            _performanceMetrics.Clear();

            FrameDurationReceiveToSend = new PerformanceMetric("Frame duration: After receiving 'frame ready' -> before sending 'frame done'", 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 85);
            FrameDurationReceiveToSent = new PerformanceMetric("Frame duration: After receiving 'frame ready' -> after sending 'frame done'", 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 85);
            FrameDurationReceiveToReceive = new PerformanceMetric("Frame duration: After receiving 'frame ready' -> receiving next 'frame ready'", 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 85);
            CommunicationSendToReceive = new PerformanceMetric("BWAPI duration: Before sending 'frame done' -> After receiving 'frame ready'", 1, 3, 5, 10, 15, 20, 30);
            CommunicationSendToSent = new PerformanceMetric("BWAPI duration: Before sending 'frame done' -> After sending 'frame done'", 1, 3, 5, 10, 15, 20, 30);
            CommunicationListenToReceive = new PerformanceMetric("BWAPI duration: Before listening for 'frame ready' -> After receiving 'frame ready'", 1, 3, 5, 10, 15, 20, 30);
            CopyingToBuffer = new PerformanceMetric("Copying frame to buffer", 5, 10, 15, 20, 25, 30);
            IntentionallyBlocking = new PerformanceMetric("Time holding frame until buffer frees capacity", 0);
            FrameBufferSize = new PerformanceMetric("Frames already buffered when enqueuing a new frame", 0, 1);
            FramesBehind = new PerformanceMetric("Frames behind real-time when handling events", 0, 1);
            FlushSideEffects = new PerformanceMetric("Time flushing side effects", 1, 3, 5);
            BotResponse = new PerformanceMetric("Duration of bot event handlers", 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 85);
            BotIdle = new PerformanceMetric("Time bot spent idle", double.MaxValue);
            ClientIdle = new PerformanceMetric("Time client spent waiting for bot", _configuration.MaxFrameDurationMs);
            ExcessSleep = new PerformanceMetric("Excess duration of client sleep", 1, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 85);
            NumberOfEvents = new PerformanceMetric("Number of events received from BWAPI", 1, 2, 3, 4, 5, 6, 8, 10, 15, 20);
            NumberOfEventsTimesDurationReceiveToSent = new PerformanceMetric("Number of events received from BWAPI, multiplied by the receive-to-sent duration of that frame", 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 85);

            _performanceMetrics.Add(FrameDurationReceiveToSend);
            _performanceMetrics.Add(FrameDurationReceiveToSent);
            _performanceMetrics.Add(FrameDurationReceiveToReceive);
            _performanceMetrics.Add(CommunicationSendToReceive);
            _performanceMetrics.Add(CommunicationSendToSent);
            _performanceMetrics.Add(CommunicationListenToReceive);
            _performanceMetrics.Add(CopyingToBuffer);
            _performanceMetrics.Add(IntentionallyBlocking);
            _performanceMetrics.Add(FrameBufferSize);
            _performanceMetrics.Add(FramesBehind);
            _performanceMetrics.Add(FlushSideEffects);
            _performanceMetrics.Add(BotResponse);
            _performanceMetrics.Add(BotIdle);
            _performanceMetrics.Add(ClientIdle);
            _performanceMetrics.Add(ExcessSleep);
            _performanceMetrics.Add(NumberOfEvents);
            _performanceMetrics.Add(NumberOfEventsTimesDurationReceiveToSent);
        }

        public void AddMetric(PerformanceMetric performanceMetric)
        {
            _performanceMetrics.Add(performanceMetric);
        }

        public override string ToString()
        {
            var outputBuilder = new StringBuilder();
            outputBuilder.Append("Performance metrics:");
            foreach (var metric in _performanceMetrics)
            {
                outputBuilder.Append("\n");
                outputBuilder.Append(metric.ToString());
            }
            return outputBuilder.ToString();
        }

        /**
         * Duration of the frame cycle steps measured by BWAPI,
         * from receiving a frame to BWAPI
         * to sending commands back
         * *exclusive* of the time spent sending commands back.
         */
        public PerformanceMetric FrameDurationReceiveToSend { get; private set; }

        /**
         * Duration of the frame cycle steps measured by BWAPI,
         * from receiving a frame to BWAPI
         * to sending commands back
         * *inclusive* of the time spent sending commands back.
         */
        public PerformanceMetric FrameDurationReceiveToSent { get; private set; }

        /**
         * Duration of a frame cycle originating at
         * the time when JBWAPI observes a new frame in shared memory.
         */
        public PerformanceMetric FrameDurationReceiveToReceive { get; private set; }

        /**
         * Time spent copying game data from system pipe shared memory to a frame buffer.
         * Applicable only in asynchronous mode.
         */
        public PerformanceMetric CopyingToBuffer { get; private set; }

        /**
         * Time spent intentionally blocking on bot operation due to a full frame buffer.
         * Applicable only in asynchronous mode.
         */
        public PerformanceMetric IntentionallyBlocking { get; private set; }

        /**
         * Number of frames backed up in the frame buffer, after enqueuing each frame (and not including the newest frame).
         * Applicable only in asynchronous mode.
         */
        public PerformanceMetric FrameBufferSize { get; private set; }

        /**
         * Number of frames behind real-time the bot is at the time it handles events.
         * Applicable only in asynchronous mode.
         */
        public PerformanceMetric FramesBehind { get; private set; }

        /**
         * Time spent applying bot commands to the live frame.
         */
        public PerformanceMetric FlushSideEffects { get; private set; }

        /**
         * Time spent waiting for bot event handlers to complete for a single frame.
         */
        public PerformanceMetric BotResponse { get; private set; }

        /**
         * Time spent waiting for a response from BWAPI,
         * inclusive of the time spent sending the signal to BWAPI
         * and the time spent waiting for and receiving it.
         */
        public PerformanceMetric CommunicationSendToReceive { get; private set; }

        /**
         * Time spent sending the "frame complete" signal to BWAPI.
         * Significant durations would indicate something blocking writes to shared memory.
         */
        public PerformanceMetric CommunicationSendToSent { get; private set; }

        /**
         * Time spent waiting for a "frame ready" signal from BWAPI.
         * This time likely additional response time spent by other bots and StarCraft itself.
         */
        public PerformanceMetric CommunicationListenToReceive { get; private set; }

        /**
         * Time bot spends idle.
         * Applicable only in asynchronous mode.
         */
        public PerformanceMetric BotIdle { get; private set; }

        /**
         * Time the main thread spends idle, waiting for the bot to finish processing frames.
         * Applicable only in asynchronous mode.
         */
        public PerformanceMetric ClientIdle { get; private set; }

        /**
         * Time the main thread spends oversleeping its timeout target, potentially causing overtime frames.
         * Applicable only in asynchronous mode.
         */
        public PerformanceMetric ExcessSleep { get; private set; }

        /**
         * The number of events sent by BWAPI each frame.
         * Helps detect use of broken BWAPI 4.4 tournament modules, with respect to:
         * - https://github.com/bwapi/bwapi/issues/860
         * - https://github.com/davechurchill/StarcraftAITournamentManager/issues/42
         */
        public PerformanceMetric NumberOfEvents { get; private set; }

        /**
         * The number of events sent by BWAPI each frame,
         * multiplied by the duration of time spent on that frame (receive-to-sent).
         * Helps detect use of broken BWAPI 4.4 tournament modules, with respect to:
         * - https://github.com/bwapi/bwapi/issues/860
         * - https://github.com/davechurchill/StarcraftAITournamentManager/issues/42
         */
        public PerformanceMetric NumberOfEventsTimesDurationReceiveToSent { get; private set; }
    }
}