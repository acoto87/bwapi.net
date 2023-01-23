using System;

namespace ExampleBot
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Hello, Bot!");

            var bot = new ExampleBot();
            bot.Run();
        }
    }
}