using System;
using Rpg.Core.Interfaces;
using Rpg.Core.Models;

namespace Rpg.Core.Strategies
{
    public class PhysicalDamageStrategy : IDamageStrategy
    {
        public int CalculateDamage(Character attacker, Character defender)
        {
            // Formula: (Strength + WeaponBonus) - EnemyDefense
            int damage = attacker.GetPhysicalAttack() - defender.Statistics.Defense;
            return Math.Max(1, damage); // Ensure every hit deals at least 1 damage
        }
    }
}
