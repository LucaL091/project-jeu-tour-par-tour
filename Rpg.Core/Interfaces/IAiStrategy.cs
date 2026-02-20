using Rpg.Core.Models;

namespace Rpg.Core.Interfaces
{
    public interface IAiStrategy
    {
        void ExecuteAction(Monster source, Character target);
    }
}
