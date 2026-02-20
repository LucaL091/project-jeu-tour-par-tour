using Rpg.Core.Models;

namespace Rpg.Core.Interfaces
{
    public interface IDamageStrategy
    {
        int CalculateDamage(Character attacker, Character defender);
    }
}
