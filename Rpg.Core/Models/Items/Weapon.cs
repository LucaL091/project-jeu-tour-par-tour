using Rpg.Core.Interfaces;

namespace Rpg.Core.Models.Items
{
    public class Weapon
    {
        public string Name { get; }
        public int DamageBonus { get; }
        public string Type { get; } // e.g., "Sword", "Axe", "Staff"
        public int HpBonus { get; }
        public int MpBonus { get; }
        public int AgiBonus { get; }
        public int DefBonus { get; }

        public Weapon(string name, int damageBonus, string type, int hpBonus = 0, int mpBonus = 0, int agiBonus = 0, int defBonus = 0)
        {
            Name = name;
            DamageBonus = damageBonus;
            Type = type;
            HpBonus = hpBonus;
            MpBonus = mpBonus;
            AgiBonus = agiBonus;
            DefBonus = defBonus;
        }
    }
}
