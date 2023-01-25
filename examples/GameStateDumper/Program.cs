using BWAPI.NET;

namespace GameStateDumper
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Hello, Bot!");

            var bot = new GameStateDumperBot();
            bot.Run();
        }
    }

    public sealed class GameStateDumperBot : DefaultBWListener
    {
        private const string DumpDirectory = "Dump";

        private BWClient _bwClient;
        private Game _game;

        public void Run()
        {
            if (!Directory.Exists(DumpDirectory))
            {
                Directory.CreateDirectory(DumpDirectory);
            }

            _bwClient = new BWClient(this);
            _bwClient.StartGame();
        }

        public override void OnStart()
        {
            _game = _bwClient.Game;

            var fileName = $"{_game.MapFileName()}_frame{_game.GetFrameCount()}_buffer.bin";
            var filePath = Path.Combine(DumpDirectory, fileName);

            Console.Write("Dumping game state buffer to: {0}...", filePath);

            _bwClient.DumpGameStateBuffer(filePath);

            Console.WriteLine("Done");

            Console.Write("Leaving game now...");

            _game.LeaveGame();

            Console.WriteLine("you can close the game.");
        }
    }
}