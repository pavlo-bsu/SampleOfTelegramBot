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
            Console.ReadLine();
        }
    }
}
