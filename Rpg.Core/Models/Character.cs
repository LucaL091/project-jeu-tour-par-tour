using System;
using System.Collections.Generic;
using System.Linq;
using Rpg.Core.Interfaces;
using Rpg.Core.Models.Items;

namespace Rpg.Core.Models
{
    /// <summary>
    /// Classe de base abstraite représentant tout personnage (Héros ou Monstre).
    /// Gère les statistiques, l'inventaire, les compétences et la progression.
    /// </summary>
    public abstract class Character
    {
        /// <summary>
        /// Statistiques actuelles du personnage (HP, MP, Force, etc.).
        /// </summary>
        public Stats Statistics { get; protected set; }

        protected Character(Stats stats)
        {
            Statistics = stats;
            Skills = new List<Skill>();
            Inventory = new List<IUsableItem>();
        }

        public List<Skill> Skills { get; private set; }
        public List<IUsableItem> Inventory { get; private set; }
        public Weapon EquippedWeapon { get; private set; }
        public int Experience { get; private set; }
        
        /// <summary>
        /// Expérience requise pour le prochain niveau (Liveau * 100).
        /// </summary>
        public int ExpToNextLevel => Statistics.Level * 100;

        public string Name => Statistics.Name;
        public bool IsAlive => Statistics.HP > 0;

        /// <summary>
        /// Réduit les points de vie du personnage.
        /// </summary>
        public virtual void TakeDamage(int amount)
        {
            var newHp = Math.Max(0, Statistics.HP - amount);
            // La struct Stats est immuable, on crée une nouvelle instance
            Statistics = new Stats(
                Statistics.Name, 
                Statistics.Level, 
                Statistics.MaxHP, 
                Statistics.MaxMP, 
                Statistics.Strength, 
                Statistics.Intelligence, 
                Statistics.Agility, 
                Statistics.Defense, 
                Statistics.MagicResistance,
                newHp,
                Statistics.MP
            );
        }

        /// <summary>
        /// Équipe une arme et met à jour les bonus de statistiques.
        /// </summary>
        public void EquipWeapon(Weapon weapon)
        {
            // On retire les bonus de l'ancienne arme avant d'équiper la nouvelle
            if (EquippedWeapon != null) ApplyWeaponBonuses(EquippedWeapon, -1);
            
            EquippedWeapon = weapon;
            
            // On applique les nouveaux bonus
            if (EquippedWeapon != null) ApplyWeaponBonuses(EquippedWeapon, 1);
        }

        /// <summary>
        /// Ajoute ou retire les bonus d'une arme aux statistiques.
        /// </summary>
        private void ApplyWeaponBonuses(Weapon w, int multiplier)
        {
            Statistics = new Stats(
                Statistics.Name,
                Statistics.Level,
                Statistics.MaxHP + (w.HpBonus * multiplier),
                Statistics.MaxMP + (w.MpBonus * multiplier),
                Statistics.Strength,
                Statistics.Intelligence,
                Statistics.Agility + (w.AgiBonus * multiplier),
                Statistics.Defense + (w.DefBonus * multiplier),
                Statistics.MagicResistance,
                Statistics.HP + (w.HpBonus * multiplier),
                Statistics.MP + (w.MpBonus * multiplier)
            );
        }

        /// <summary>
        /// Soigne le personnage du montant spécifié, sans dépasser le max.
        /// </summary>
        public virtual void Heal(int amount)
        {
            int newHp = Math.Min(Statistics.MaxHP, Statistics.HP + amount);
             Statistics = new Stats(
                Statistics.Name, 
                Statistics.Level, 
                Statistics.MaxHP, 
                Statistics.MaxMP, 
                Statistics.Strength, 
                Statistics.Intelligence, 
                Statistics.Agility, 
                Statistics.Defense, 
                Statistics.MagicResistance,
                newHp,
                Statistics.MP
            );
        }

        /// <summary>
        /// Consomme des points de mana pour lancer un sort.
        /// </summary>
        public virtual void ConsumeMana(int amount)
        {
            int newMp = Math.Max(0, Statistics.MP - amount);
            Statistics = new Stats(
                Statistics.Name, 
                Statistics.Level, 
                Statistics.MaxHP, 
                Statistics.MaxMP, 
                Statistics.Strength, 
                Statistics.Intelligence, 
                Statistics.Agility, 
                Statistics.Defense, 
                Statistics.MagicResistance,
                Statistics.HP,
                newMp
            );
        }

        public virtual void RestoreMana(int amount)
        {
             int newMp = Math.Min(Statistics.MaxMP, Statistics.MP + amount);
             Statistics = new Stats(
                Statistics.Name, 
                Statistics.Level, 
                Statistics.MaxHP, 
                Statistics.MaxMP, 
                Statistics.Strength, 
                Statistics.Intelligence, 
                Statistics.Agility, 
                Statistics.Defense, 
                Statistics.MagicResistance,
                Statistics.HP,
                newMp
            );
        }

        /// <summary>
        /// Ajoute de l'expérience et gère la montée de niveau.
        /// </summary>
        public void GainExperience(int amount)
        {
            Experience += amount;
            if (Experience >= ExpToNextLevel)
            {
                LevelUp();
            }
        }

        /// <summary>
        /// Augmente le niveau et améliore les statistiques de base.
        /// </summary>
        private void LevelUp()
        {
            Experience -= ExpToNextLevel; // Report du surplus d'expérience
            
            // Gain automatique : +1 Level, +10 HP/MP, +2 Str/Int/Agi
            Statistics = new Stats(
                Statistics.Name,
                Statistics.Level + 1,
                Statistics.MaxHP + 10,
                Statistics.MaxMP + 10,
                Statistics.Strength + 2,
                Statistics.Intelligence + 2,
                Statistics.Agility + 2,
                Statistics.Defense + 1,
                Statistics.MagicResistance + 1
            );
            
            // Soin total à chaque montée de niveau
            Heal(Statistics.MaxHP);
            RestoreMana(Statistics.MaxMP);

            OnLevelUp();
        }

        protected virtual void OnLevelUp() { }

        /// <summary>
        /// Calcule la valeur totale d'attaque physique (Force + bonus arme).
        /// </summary>
        public int GetPhysicalAttack()
        {
            int baseStr = Statistics.Strength;
            int weaponBonus = EquippedWeapon?.DamageBonus ?? 0;
            return baseStr + weaponBonus;
        }
    }
}
