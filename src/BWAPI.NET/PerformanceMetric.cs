using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BWAPI.NET
{
    /// <summary>
    /// Aggregates labeled time series data.
    /// </summary>
    public class PerformanceMetric
    {
        private readonly string _name;

        private long _timeStarted;
        private int _interrupted;

        private readonly TRunningTotal _runningTotal = new TRunningTotal();
        private readonly List<Threshold> _thresholds = new List<Threshold>();

        public PerformanceMetric(string name, params double[] thresholds)
        {
            _name = name;
            foreach (var threshold in thresholds)
            {
                _thresholds.Add(new Threshold(threshold));
            }
        }

        /// <summary>
        /// Records the duration of a function call.
        /// </summary>
        /// <param name="action">The function to time.</param>
        public void Time(Action action)
        {
            StartTiming();
            action();
            StopTiming();
        }

        /// <summary>
        /// Calls a function; but only records the duration if a condition is met.
        /// </summary>
        /// <param name="condition">Whether to record the function call duration.</param>
        /// <param name="runnable">The function to call.</param>
        public void TimeIf(bool condition, Action action)
        {
            if (condition)
            {
                Time(action);
            }
            else
            {
                action();
            }
        }

        /**
         * Manually start timing.
         * The next call to stopTiming() will record the duration in fractional milliseconds.
         */
        public void StartTiming()
        {
            if (_timeStarted > 0)
            {
                ++_interrupted;
            }

            _timeStarted = Stopwatch.GetTimestamp();
        }


        /**
          * Manually stop timing.
          * If paired with a previous call to startTiming(), records the measured time between the calls in fractional milliseconds.
          */
        public void StopTiming()
        {
            if (_timeStarted <= 0)
            {
                return;
            }

            // Use nanosecond resolution timer, but record in units of milliseconds.
            var timeEnded = Stopwatch.GetTimestamp();
            var timeDiff = timeEnded - _timeStarted;
            _timeStarted = 0;

            Record(timeDiff / 1000000d);
        }

        /**
         * Manually records a specific value.
         */
        public void Record(double value)
        {
            _runningTotal.Record(value);

            foreach (var threshold in _thresholds)
            {
                threshold.Record(value);
            }
        }

        /**
         * @return A pretty-printed description of the recorded values.
         */
        public override string ToString()
        {
            if (_runningTotal.Samples <= 0)
            {
                return _name + ": No samples.";
            }

            var output = $"{_name}:\n{_runningTotal.Samples:###,###.#} samples averaging {_runningTotal.Mean:###,###.#} [{_runningTotal.Min:###,###.#} - {_runningTotal.Max:###,###.#}]";

            foreach (var threshold in _thresholds)
            {
                output += threshold.ToString();
            }

            if (_interrupted > 0)
            {
                output += $"\n\tInterrupted {_interrupted} times";
            }

            return output;
        }

        public TRunningTotal RunningTotal
        {
            get => _runningTotal;
        }

        public int Interrupted
        {
            get => _interrupted;
        }

        public class TRunningTotal
        {
            private int _samples;
            private double _last;
            private double _mean;
            private double _min = double.MaxValue;
            private double _max = double.MinValue;

            public void Record(double value)
            {
                _last = value;
                _min = Math.Min(_min, value);
                _max = Math.Max(_max, value);
                _mean = (_mean * _samples + value) / (_samples + 1d);
                _samples++;
            }

            public double Samples
            {
                get => _samples;
            }

            public double Last
            {
                get => _last;
            }

            public double Mean
            {
                get => _mean;
            }

            public double Min
            {
                get => _min;
            }

            public double Max
            {
                get => _max;
            }
        }

        public class Threshold
        {
            private readonly double _threshold;

            private readonly TRunningTotal _runningTotal;

            public Threshold(double value)
            {
                _threshold = value;
                _runningTotal = new TRunningTotal();
            }

            public void Record(double value)
            {
                if (value >= _threshold)
                {
                    _runningTotal.Record(value);
                }
            }

            public override string ToString()
            {
                return _runningTotal.Samples > 0
                    ? $"\n>={_threshold:###,###.#}: {_runningTotal.Samples} samples averaging {_runningTotal.Mean:###,###.#}"
                    : "";
            }
        }
    }
}