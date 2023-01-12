using System;

namespace ExampleBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var bot = new ExampleBot();
            bot.Run();

            Console.ReadLine();
        }
    }
}