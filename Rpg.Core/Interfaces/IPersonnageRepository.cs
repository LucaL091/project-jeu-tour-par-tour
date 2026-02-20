using System.Collections.Generic;
using Rpg.Core.Models;

namespace Rpg.Core.Interfaces
{
    public interface IPersonnageRepository
    {
        void Save(Character character);
        Character GetByName(string name);
        IEnumerable<Character> GetAll();
    }
}
