using Rpg.Core.Interfaces;

namespace Rpg.Core.Models.Items
{
    public class Elixir : IUsableItem
    {
        public string Name => "Elixir";
        public string Description => "Restaure entierement les PV et PM.";

        public void Use(Character character)
        {
            character.Heal(character.Statistics.MaxHP);
            character.RestoreMana(character.Statistics.MaxMP);
        }
    }
}
