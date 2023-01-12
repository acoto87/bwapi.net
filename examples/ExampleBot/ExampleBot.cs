using System;
using BWAPI.NET;

namespace ExampleBot
{
    public class ExampleBot : DefaultBWListener
    {
        private BWClient _bwClient;
        private Game _game;

        public void Run()
        {
            _bwClient = new BWClient(this);
            _bwClient.StartGame();
        }

        public override void OnStart()
        {
            _game = _bwClient.Game;
        }

        public override void OnFrame()
        {
            _game.DrawTextScreen(100, 100, "Hello World!");
        }

        public override void OnUnitComplete(Unit unit)
        {
            if (unit.GetUnitType().IsWorker())
            {
                Unit closestMineral = null;
                int closestDistance = int.MaxValue;
                foreach (Unit mineral in _game.GetMinerals())
                {
                    int distance = unit.GetDistance(mineral);
                    if (distance < closestDistance)
                    {
                        closestMineral = mineral;
                        closestDistance = distance;
                    }
                }
                // Gather the closest mineral
                unit.Gather(closestMineral);
            }
        }
    }
}
