using Rpg.Core.Models;

namespace Rpg.Core.Interfaces
{
    public interface IUsableItem
    {
        string Name { get; }
        void Use(Character target);
    }
}
