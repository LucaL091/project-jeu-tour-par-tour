using Rpg.Core.Interfaces;

namespace Rpg.Core.Models.Items
{
    public class ManaPotion : IUsableItem
    {
        public string Name => "Potion de Mana";

        public void Use(Character target)
        {
            target.RestoreMana(20);
        }
    }
}
