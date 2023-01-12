using System;
using System.Collections.Generic;

namespace BWAPI.NET
{
    public class ConnectedUnitCache
    {
        private readonly Dictionary<Unit, List<Unit>> _connectedUnits = new Dictionary<Unit, List<Unit>>();
        private readonly Func<Unit, Unit> _condition;
        private readonly Game _game;

        private int _lastUpdate = -1;

        public ConnectedUnitCache(Game game, Func<Unit, Unit> condition)
        {
            _game = game;
            _condition = condition;
        }

        /// <summary>
        /// Lazily update connectedUnits. Only users of the calls pay for it, and only
        /// pay once per frame.
        /// Avoids previous O(n^2) implementation which would be costly for
        /// lategame carrier fights
        /// </summary>
        public List<Unit> GetConnected(Unit unit)
        {
            var frame = _game.GetFrameCount();
            if (_lastUpdate < frame)
            {
                foreach (var list in _connectedUnits.Values)
                {
                    list.Clear();
                }

                foreach (var u in _game.GetAllUnits())
                {
                    var owner = _condition(u);
                    if (owner != null)
                    {
                        if (!_connectedUnits.ContainsKey(owner))
                        {
                            _connectedUnits.Add(owner, new List<Unit>());
                        }

                        _connectedUnits[owner].Add(u);
                    }
                }

                _lastUpdate = frame;
            }

            if (!_connectedUnits.ContainsKey(unit))
            {
                return new List<Unit>();
            }

            return _connectedUnits[unit];
        }

        public void Reset()
        {
            _connectedUnits.Clear();
            _lastUpdate = -1;
        }
    }
}