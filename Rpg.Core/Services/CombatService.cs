using System;
using System.Collections.Generic;
using System.Linq;
using Rpg.Core.Interfaces;
using Rpg.Core.Models;
using Rpg.Core.Models.Items;

namespace Rpg.Core.Services
{
    public class CombatService
    {
        private readonly IActionLogger _logger;
        private readonly IDamageStrategy _damageStrategy;
        private readonly Random _random = new Random();

        public CombatService(IActionLogger logger, IDamageStrategy damageStrategy)
        {
            _logger = logger;
            _damageStrategy = damageStrategy;
        }

        public void ProcessTurn(Character attacker, Character defender)
        {
            if (!attacker.IsAlive || !defender.IsAlive) return;

            _logger.Log($"{attacker.Name} attaque {defender.Name}!");
            
            int damage = _damageStrategy.CalculateDamage(attacker, defender);
            defender.TakeDamage(damage);

            _logger.Log($"{defender.Name} subit {damage} degats. PV restants: {defender.Statistics.HP}");

            if (!defender.IsAlive)
            {
                HandleDefeat(attacker, defender);
            }
        }

        public void ProcessSkill(Character user, Skill skill, Character target)
        {
            if (!user.IsAlive) return;

            if (user.Statistics.MP < skill.ManaCost)
            {
                _logger.Log($"{user.Name} essaie de lancer {skill.Name} mais n'a pas assez de Mana!");
                return;
            }

            user.ConsumeMana(skill.ManaCost);
            skill.Execute(user, target, _logger);

            // Check for defeat if it was an offensive skill
            if (!skill.IsSupport && !target.IsAlive)
            {
                HandleDefeat(user, target);
            }
        }
        
        
        private void HandleDefeat(Character winner, Character loser)
        {
             _logger.Log($"{loser.Name} a ete vaincu!");
            
            if (winner is Hero hero)
            {
                // XP Reward
                int xpGain = loser.Statistics.Level * 20;
                _logger.Log($"{hero.Name} gagne {xpGain} XP!");
                hero.GainExperience(xpGain);

                // Loot Roll
                int roll = _random.Next(1, 101);
                IUsableItem loot = null;

                if (roll >= 95) // 5% Chance for Elixir
                {
                    loot = new Elixir();
                }
                else if (roll >= 70) // 25% Chance for Mana Potion
                {
                    loot = new ManaPotion();
                }
                else if (roll >= 40) // 30% Chance for Health Potion
                {
                    loot = new HealthPotion();
                }

                if (loot != null)
                {
                    hero.Inventory.Add(loot);
                    _logger.Log($"Butin! Vous avez trouve: {loot.Name}");
                }
            }
        }
    }
}
