using Rpg.Core.Models;
using Rpg.Core.Interfaces;

namespace Rpg.Core.Factories
{
    public class CharacterFactory
    {
        public static Hero CreateHero(string name)
        {
            // Simplified factory logic
            var stats = new Stats(name, 1, 100, 50, 10, 5, 5, 5, 0); // Default stats
            return new Hero(stats);
        }

        public static Monster CreateMonster(string type, IAiStrategy aiStrategy)
        {
            // Simplified logic
            var stats = new Stats(type, 1, 50, 0, 8, 2, 4, 2, 0);
            return new Monster(stats, aiStrategy);
        }
    }
}
