using System;
using System.Collections.Generic;
using System.Linq;
using Rpg.Core.Interfaces;
using Rpg.Core.Models.Items;

namespace Rpg.Core.Models
{
    public abstract class Character
    {
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
        public int ExpToNextLevel => Statistics.Level * 100; // Simple formula

        public string Name => Statistics.Name;
        public bool IsAlive => Statistics.HP > 0;

        public virtual void TakeDamage(int amount)
        {
            var newHp = Math.Max(0, Statistics.HP - amount);
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

        public void EquipWeapon(Weapon weapon)
        {
            // Remove old bonuses if any
            if (EquippedWeapon != null) ApplyWeaponBonuses(EquippedWeapon, -1);
            
            EquippedWeapon = weapon;
            
            // Apply new bonuses
            if (EquippedWeapon != null) ApplyWeaponBonuses(EquippedWeapon, 1);
        }

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
                Statistics.HP + (w.HpBonus * multiplier), // Also heal/damage current HP by the same amount
                Statistics.MP + (w.MpBonus * multiplier)
            );
        }

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

        public void GainExperience(int amount)
        {
            Experience += amount;
            if (Experience >= ExpToNextLevel)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            Experience -= ExpToNextLevel; // Carry over excess XP
            
            // Simple Level Up: +1 Level, +10 HP/MP, +2 Str/Int/Agi
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
            
            // Full Heal on Level Up!
            Heal(Statistics.MaxHP);
            RestoreMana(Statistics.MaxMP);

            OnLevelUp();
        }

        protected virtual void OnLevelUp() { }

        public int GetPhysicalAttack()
        {
            int baseStr = Statistics.Strength;
            int weaponBonus = EquippedWeapon?.DamageBonus ?? 0;
            return baseStr + weaponBonus;
        }
    }
}
