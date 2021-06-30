using System;
using System.Threading.Tasks;

namespace Pavlo.SampleOfTelegramBot
{
    class Program
    {
        static TBot bot;
        static async Task Main(string[] args)
        {
            bot = new TBot(@"d:\k.txt");
            await bot.Initialize();
            Console.WriteLine(DateTime.Now+$" Start listening for @{bot.BotUser.Username}!");
            Console.Title = $"Telegram bot @{bot.BotUser.Username}";

            do
            {
            } while (Console.ReadLine().ToLowerInvariant() != "exit");
        }
    }
}
