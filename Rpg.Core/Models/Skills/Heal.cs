using Rpg.Core.Interfaces;

namespace Rpg.Core.Models.Skills
{
    public class Heal : Skill
    {
        public Heal()
        {
            Name = "Soin";
            Description = "Restaure des PV a une cible.";
            ManaCost = 15;
            IsSupport = true;
        }

        public override void Execute(Character user, Character target, ICombatObserver observer)
        {
            // Simple calculation: Intelligence * 3
            int amount = user.Statistics.Intelligence * 3;
            
            observer.OnAction($"{user.Name} casts Heal on {target.Name}!");
            target.Heal(amount);
            observer.OnAction($"{target.Name} recovers {amount} HP.");
        }
    }
}
