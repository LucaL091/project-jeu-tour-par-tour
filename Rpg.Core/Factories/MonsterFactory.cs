using System;
using Rpg.Core.Interfaces;
using Rpg.Core.Models;
using Rpg.Core.Services;
using Rpg.Core.Strategies;

namespace Rpg.Core.Factories
{
    /// <summary>
    /// Usine (Factory) responsable de la création dynamique des monstres.
    /// Gère le scaling des statistiques en fonction de l'étage et de la difficulté.
    /// </summary>
    public class MonsterFactory
    {
        private readonly CombatService _combatService;
        private readonly Random _random = new Random();

        public MonsterFactory(CombatService combatService)
        {
            _combatService = combatService;
        }

        /// <summary>
        /// Crée un monstre aléatoire dont les statistiques sont adaptées au niveau actuel.
        /// </summary>
        /// <param name="floor">L'étage actuel du donjon (influence le scaling).</param>
        /// <param name="difficulty">Le niveau de difficulté choisi par le joueur.</param>
        public Monster CreateRandomMonster(int floor, Difficulty difficulty)
        {
            // 1. Définition des multiplicateurs de difficulté
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
                    xpMult = 1.5; 
                    break;
            }

            // 2. Sélection d'un modèle (Template) de monstre via un tirage pondéré
            int roll = _random.Next(1, 101); // 1-100
            string name;
            int baseHp;
            int baseStr;
            int baseDef;
            int baseAgi;

            if (roll <= 40) // 40% Slime (Faible)
            {
                name = "Slime";
                baseHp = 30;
                baseStr = 10;
                baseDef = 0;
                baseAgi = 2;
            }
            else if (roll <= 75) // 35% Gobelin (Standard)
            {
                name = "Gobelin";
                baseHp = 50;
                baseStr = 15;
                baseDef = 2;
                baseAgi = 5;
            }
            else if (roll <= 90) // 15% Squelette (Fragile mais tape fort)
            {
                name = "Squelette";
                baseHp = 40;
                baseStr = 18; 
                baseDef = 1;
                baseAgi = 6;
            }
            else // 10% Orque (Tank)
            {
                name = "Orque";
                baseHp = 80;
                baseStr = 16;
                baseDef = 4;
                baseAgi = 1;
            }

            // 3. Calcul du Scaling (Progression avec l'étage)
            // Formule : (Base + Étage * Facteur) * Multiplicateur Difficulté
            int maxHp = (int)((baseHp + (floor * 10)) * hpMult);
            int str = (int)((baseStr + floor) * strMult);
            int def = baseDef + (floor / 2); 
            int agi = baseAgi + (floor / 3);

            // Création de l'objet Stats avec les valeurs calculées
            Stats stats = new Stats($"{name} Lvl {floor}", floor, maxHp, 10, str, 0, agi, def, 0);
            
            // On injecte le service de combat à la stratégie d'IA pour qu'elle puisse agir
            IAiStrategy ai = new RandomAiStrategy(_combatService);
            return new Monster(stats, ai);
        }
    }
}
