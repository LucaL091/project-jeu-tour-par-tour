using System;
using Rpg.Core.Interfaces;
using Rpg.Core.Models;

namespace Rpg.Core.Models.Items
{
    public class Potion : IUsableItem
    {
        public string Name => "Potion de Soin";
        public int HealAmount { get; }

        public Potion(int healAmount)
        {
            HealAmount = healAmount;
        }

        public void Use(Character target)
        {
            if (target != null && target.IsAlive)
            {
                // We need a way to Heal. Since Character has TakeDamage, it should probably have Heal.
                // Or we access Stats. 
                // Given previous complication with Stats immutability/mutability being a bit vague in my implementation,
                // I will update Character to have a Heal method.
                target.Heal(HealAmount);
            }
        }
    }
}
