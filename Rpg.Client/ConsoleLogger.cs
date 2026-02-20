using System;
using Rpg.Core.Interfaces;

namespace Rpg.Client
{
    public class ConsoleLogger : IActionLogger
    {
        public void Log(string message)
        {
            // Simple color coding based on message content
            if (message.Contains("attacks"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else if (message.Contains("damage"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (message.Contains("defeated"))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
            }
            else if (message.Contains("heal"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
            }

            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
