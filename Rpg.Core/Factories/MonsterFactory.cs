using System;
using Rpg.Core.Interfaces;
using Rpg.Core.Models;
using Rpg.Core.Services;
using Rpg.Core.Strategies;

namespace Rpg.Core.Factories
{
    public class MonsterFactory
    {
        private readonly CombatService _combatService;
        private readonly Random _random = new Random();

        public MonsterFactory(CombatService combatService)
        {
            _combatService = combatService;
        }

        public Monster CreateRandomMonster(int floor, Difficulty difficulty)
        {
            // 1. Difficulty Multipliers
            double hpMult = 1.0;
            double strMult = 1.0;
            double xpMult = 1.0;

            switch (difficulty)
            {
                case Difficulty.Easy:
                    hpMult = 0.8;
                    strMult = 0.8;
                    xpMult = 0.8;
                    break;
                case Difficulty.Normal:
                    hpMult = 1.0;
                    strMult = 1.0;
                    xpMult = 1.0;
                    break;
                case Difficulty.Hard:
                    hpMult = 1.5;
                    strMult = 1.3;
                    xpMult = 1.5; // Reward for hard mode
                    break;
            }

            // 2. Templates
            int roll = _random.Next(1, 101); // 1-100
            string name;
            int baseHp;
            int baseStr;
            int baseDef;
            int baseAgi;

            // Simple weighted spawn logic
            if (roll <= 40) // 40% Slime (Weak)
            {
                name = "Slime";
                baseHp = 30;
                baseStr = 10;
                baseDef = 0;
                baseAgi = 2;
            }
            else if (roll <= 75) // 35% Goblin (Standard)
            {
                name = "Gobelin";
                baseHp = 50;
                baseStr = 15;
                baseDef = 2;
                baseAgi = 5;
            }
            else if (roll <= 90) // 15% Skeleton (Glass Cannon)
            {
                name = "Squelette";
                baseHp = 40;
                baseStr = 18; // Hits hard
                baseDef = 1;
                baseAgi = 6;
            }
            else // 10% Orc (Tank)
            {
                name = "Orque";
                baseHp = 80;
                baseStr = 16;
                baseDef = 4;
                baseAgi = 1;
            }

            // 3. Scaling Calculation
            // HP = (Base + Floor*Scaling) * Difficulty
            int maxHp = (int)((baseHp + (floor * 10)) * hpMult);
            int str = (int)((baseStr + floor) * strMult);
            int def = baseDef + (floor / 2); 
            int agi = baseAgi + (floor / 3);

            // Create Stats
            Stats stats = new Stats($"{name} Lvl {floor}", floor, maxHp, 10, str, 0, agi, def, 0);
            
            // Adjust XP reward logic if needed, but for now Standard gives XP based on Level * 20 in CombatService
            // We could modify CombatService to accept a multiplier, but keeping it simple is fine.
            
            IAiStrategy ai = new RandomAiStrategy(_combatService);
            return new Monster(stats, ai);
        }
    }
}
