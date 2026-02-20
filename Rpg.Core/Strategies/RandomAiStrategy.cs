using System;
using Rpg.Core.Interfaces;
using Rpg.Core.Models;
using Rpg.Core.Services;

namespace Rpg.Core.Strategies
{
    public class RandomAiStrategy : IAiStrategy
    {
        private readonly CombatService _combatService;

        public RandomAiStrategy(CombatService combatService)
        {
            _combatService = combatService;
        }

        public void ExecuteAction(Monster source, Character target)
        {
            // Simple logic: Trigger an attack using the combat service
            // In a real scenario, this could choose between attack, skill, item, etc.
            // For now, it just attacks the target.
            _combatService.ProcessTurn(source, target);
        }
    }
}
