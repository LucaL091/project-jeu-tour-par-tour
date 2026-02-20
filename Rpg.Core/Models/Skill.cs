using Rpg.Core.Interfaces;

namespace Rpg.Core.Models
{
    public abstract class Skill
    {
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public int ManaCost { get; protected set; }
        public bool IsSupport { get; protected set; } // True for Heal, False for Damage

        public abstract void Execute(Character user, Character target, IActionLogger logger);
    }
}
