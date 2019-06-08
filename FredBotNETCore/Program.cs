using Discord;
using System;
using System.Threading.Tasks;

namespace FredBotNETCore
{
    class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                new FredBot().RunAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}