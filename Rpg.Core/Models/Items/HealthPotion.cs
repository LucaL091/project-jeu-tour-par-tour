using Rpg.Core.Interfaces;

namespace Rpg.Core.Models.Items
{
    public class HealthPotion : IUsableItem
    {
        public string Name => "Potion de Soin";

        public void Use(Character target)
        {
            target.Heal(50);
        }
    }
}
