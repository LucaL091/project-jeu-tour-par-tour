using System;
using Rpg.Core.Interfaces;
using Rpg.Core.Repositories;
using Rpg.Core.Services;
using Rpg.Core.Strategies;

namespace Rpg.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Dependency Injection Setup
            
            // Repositories
            IPersonnageRepository personnageRepository = new InMemoryPersonnageRepository();

            // Strategies
            IDamageStrategy damageStrategy = new PhysicalDamageStrategy();

            // Loggers
            MemoryLogger logger = new MemoryLogger();

            // Services
            // CombatService needs Logger and DamageStrategy
            CombatService combatService = new CombatService(logger, damageStrategy);

            // PersonnageService needs Repository
            PersonnageService personnageService = new PersonnageService(personnageRepository);

            // 2. UI Initialization
            CombatUI ui = new CombatUI(personnageService, combatService, logger);

            // 3. Start Game
            try 
            {
                ui.StartGame();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
