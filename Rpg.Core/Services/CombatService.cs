using System;
using System.Collections.Generic;
using System.Linq;
using Rpg.Core.Interfaces;
using Rpg.Core.Models;
using Rpg.Core.Models.Items;

namespace Rpg.Core.Services
{
    /// <summary>
    /// Service gérant la logique des combats au tour par tour.
    /// Utilise le pattern Strategy pour les dégâts et le pattern Observateur pour les notifications.
    /// </summary>
    public class CombatService
    {
        // Liste des observateurs (UI, Loggers, etc.) qui recevront les messages de combat
        private readonly List<ICombatObserver> _observers = new List<ICombatObserver>();
        
        // Stratégie de calcul des dégâts injectée (Physique par défaut)
        private readonly IDamageStrategy _damageStrategy;
        private readonly Random _random = new Random();

        public CombatService(IDamageStrategy damageStrategy)
        {
            _damageStrategy = damageStrategy;
        }

        /// <summary>
        /// Ajoute un observateur pour recevoir les notifications de combat.
        /// </summary>
        public void Attach(ICombatObserver observer)
        {
            _observers.Add(observer);
        }

        /// <summary>
        /// Notifie tous les observateurs avec un message.
        /// </summary>
        private void Notify(string message)
        {
            foreach (var observer in _observers)
            {
                observer.OnAction(message);
            }
        }

        /// <summary>
        /// Exécute un tour d'attaque standard entre un attaquant et un défenseur.
        /// </summary>
        public void ProcessTurn(Character attacker, Character defender)
        {
            if (!attacker.IsAlive || !defender.IsAlive) return;

            Notify($"{attacker.Name} attaque {defender.Name}!");
            
            // Délégation du calcul des dégâts à la stratégie injectée
            int damage = _damageStrategy.CalculateDamage(attacker, defender);
            defender.TakeDamage(damage);

            Notify($"{defender.Name} subit {damage} degats. PV restants: {defender.Statistics.HP}");

            // Gestion de la mort du défenseur
            if (!defender.IsAlive)
            {
                HandleDefeat(attacker, defender);
            }
        }

        /// <summary>
        /// Exécute une compétence spéciale (sort ou aptitude).
        /// </summary>
        public void ProcessSkill(Character user, Skill skill, Character target)
        {
            if (!user.IsAlive) return;

            // Vérification du mana
            if (user.Statistics.MP < skill.ManaCost)
            {
                Notify($"{user.Name} essaie de lancer {skill.Name} mais n'a pas assez de Mana!");
                return;
            }

            user.ConsumeMana(skill.ManaCost);
            // La compétence s'exécute d'elle même en utilisant un forwarder pour les logs
            skill.Execute(user, target, new ObserverForwarder(_observers));

            // Si la compétence était offensive, on vérifie si la cible est morte
            if (!skill.IsSupport && !target.IsAlive)
            {
                HandleDefeat(user, target);
            }
        }
        
        /// <summary>
        /// Gère les récompenses (XP) et le butin lorsqu'un personnage est vaincu.
        /// </summary>
        private void HandleDefeat(Character winner, Character loser)
        {
            Notify($"{loser.Name} a ete vaincu!");
            
            if (winner is Hero hero)
            {
                // Gain d'expérience basé sur le niveau de l'adversaire
                int xpGain = loser.Statistics.Level * 20;
                Notify($"{hero.Name} gagne {xpGain} XP!");
                hero.GainExperience(xpGain);

                // Tirage aléatoire pour le butin (Loot)
                int roll = _random.Next(1, 101);
                IUsableItem loot = null;

                if (roll >= 95) // 5% de chance pour un Élixir
                {
                    loot = new Elixir();
                }
                else if (roll >= 70) // 25% de chance pour une Potion de Mana
                {
                    loot = new ManaPotion();
                }
                else if (roll >= 40) // 30% de chance pour une Potion de Vie
                {
                    loot = new HealthPotion();
                }

                if (loot != null)
                {
                    hero.Inventory.Add(loot);
                    Notify($"Butin! Vous avez trouve: {loot.Name}");
                }
            }
        }

        /// <summary>
        /// Classe interne permettant de transférer les notifications aux observateurs
        /// du service lors de l'exécution d'une compétence externe.
        /// </summary>
        private class ObserverForwarder : ICombatObserver
        {
            private readonly IEnumerable<ICombatObserver> _observers;
            public ObserverForwarder(IEnumerable<ICombatObserver> observers) => _observers = observers;
            public void OnAction(string message)
            {
                foreach (var obs in _observers) obs.OnAction(message);
            }
        }
    }
}
