
namespace Rpg.Core.Models
{
    public struct Stats
    {
        public string Name { get; }
        public int Level { get; }
        public int HP { get; }
        public int MaxHP { get; }
        public int MP { get; }
        public int MaxMP { get; }
        public int Strength { get; }
        public int Intelligence { get; }
        public int Agility { get; }
        public int Defense { get; }
        public int MagicResistance { get; }

        public Stats(string name, int level, int maxHp, int maxMp, int strength, int intelligence, int agility, int defense, int magicResistance, int? currentHp = null, int? currentMp = null)
        {
            Name = name;
            Level = level;
            MaxHP = maxHp;
            HP = currentHp ?? maxHp;
            MaxMP = maxMp;
            MP = currentMp ?? maxMp;
            Strength = strength;
            Intelligence = intelligence;
            Agility = agility;
            Defense = defense;
            MagicResistance = magicResistance;
        }

        // Methods to modify current HP/MP and return a new Stats instance (immutability) could go here if struct is immutable.
        // For simplicity in this context, we might want a mutable approach or helper methods in the character.
        // Given the requirement for composition, this holds the data.
    }
}
