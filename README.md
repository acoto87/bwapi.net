# BWAPI.NET

A pure .NET BWAPI 4.4.0 client implementation. It follows the idea of using memory mapped files to communicate with Starcraft launched by [BWAPI](https://bwapi.github.io/).

> _This project is a port to .NET of [JBWAPI](https://github.com/JavaBWAPI/JBWAPI) which is a pure Java BWAPI 4.4.0 client implementation._

## Capabilities

* Write AIs for Starcraft: Broodwar by controlling individual units.
* Read all relevant aspects of the game state.
* Get comprehensive information on the unit types, upgrades, technologies, weapons, and more.
* Study and research real-time AI algorithms in a robust commercial RTS environment.

## Quick Start

1. Installation
    * Install [.NET SDK](https://dotnet.microsoft.com/en-us/download)
    * Install **StarCraft: Brood War**
    * Update **StarCraft: Brood War to 1.16.1**
    * Install [BWAPI](https://bwapi.github.io/)
2. Create a bot project
    * Run `dotnet new console -o MyBot`
    * Run `cd MyBot` to change directy into `MyBot` folder
    * Run `dotnet add MyBot.csproj package BWAPI.NET` to add the reference to the nuget package generated from this repository
    * Copy and paste example bot below into `Program.cs` or develop your own bot
    * Run `dotnet run` (At this point you should see _"Game table mapping not found."_ printed each second)
3. Run StarCraft through **Chaoslauncher**
    * Run _Chaoslauncher.exe_ as administrator
        * Chaoslauncher is found in Chaoslauncher directory of [BWAPI](https://bwapi.github.io/) install directory
    * Check the _BWAPI Injector x.x.x [RELEASE]_
    * Click Start
        * Make sure the version is set to Starcraft 1.16.1, not ICCup 1.16.1
4. Run a game against Blizzard's AI
    * Go to **Single Player** -> **Expansion**
    * Select any user and click **OK**
    * Click **Play Custom**, select a map, and start a game
5. Run a game against yourself
    * Run _Chaoslauncher - MultiInstance.exe_ as administrator
    * Start
        * Go to **Multiplayer** -> **Expansion** -> **Local PC**
        * Select any user and click **OK**
        * Click **Create Game**, select a map, and click **OK**
    * Start – Uncheck _BWAPI Injector x.x.x [RELEASE]_ to let a human play, leave alone to make AI play itself
        * Go to **Multiplayer** -> **Expansion** -> **Local PC**
        * Select any user and click **OK**
        * Join the existing game created by the other client

## Bot Example

```csharp
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
            _game.DrawTextScreen(100, 100, "Hello Bot!");
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

                unit.Gather(closestMineral);
            }
        }

        public static void Main()
        {
            var bot = new ExampleBot();
            bot.Run();
        }
    }
}

```

## Legal

[Starcraft](https://www.blizzard.com/games/sc/) and [Starcraft: Broodwar](https://www.blizzard.com/games/sc/) are trademarks of [Blizzard Entertainment](https://www.blizzard.com/). [BWAPI.NET](https://github.com/acoto87/bwapi.net) through [BWAPI](https://bwapi.github.io/) is a third party "hack" that violates the End User License Agreement (EULA). It is strongly recommended to purchase a legitimate copy of Starcraft: Broodwar from Blizzard Entertainment before using [BWAPI.NET](https://github.com/acoto87/bwapi.net) and/or [BWAPI](https://bwapi.github.io/).