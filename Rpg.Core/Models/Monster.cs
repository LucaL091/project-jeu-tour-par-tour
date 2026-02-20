using System;
using Rpg.Core.Interfaces; // Assuming interface will be here

namespace Rpg.Core.Models
{
    public class Monster : Character
    {
        public IAiStrategy AiStrategy { get; private set; }

        public Monster(Stats stats, IAiStrategy aiStrategy) : base(stats)
        {
            AiStrategy = aiStrategy;
        }

        public void ExecuteTurn(Character target)
        {
            // AI Logic to determine action
            AiStrategy.ExecuteAction(this, target);
        }
    }
}
