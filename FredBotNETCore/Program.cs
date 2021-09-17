using System;

namespace FredBotNETCore
{
    internal class Program
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
