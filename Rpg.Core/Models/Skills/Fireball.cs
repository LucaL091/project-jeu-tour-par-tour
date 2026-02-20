using Rpg.Core.Interfaces;

namespace Rpg.Core.Models.Skills
{
    public class Fireball : Skill
    {
        public Fireball()
        {
            Name = "Boule de Feu";
            Description = "Inflige des degats magiques a une cible.";
            ManaCost = 20;
            IsSupport = false;
        }

        public override void Execute(Character user, Character target, ICombatObserver observer)
        {
            // Simple damage formula: Intelligence * 2 + Level * 2
            int damage = (user.Statistics.Intelligence * 2) + (user.Statistics.Level * 2);
            // Apply magic resistance reduction if needed
            int reduction = target.Statistics.MagicResistance / 2;
            int finalDamage = System.Math.Max(1, damage - reduction);

            observer.OnAction($"{user.Name} casts Fireball on {target.Name}!");
            target.TakeDamage(finalDamage);
            observer.OnAction($"{target.Name} takes {finalDamage} fire damage.");
        }
    }
}
