using System.Collections.Generic;
using System.Linq;
using Rpg.Core.Interfaces;
using Rpg.Core.Models;

namespace Rpg.Core.Repositories
{
    public class InMemoryPersonnageRepository : IPersonnageRepository
    {
        private readonly List<Character> _characters = new List<Character>();

        public void Save(Character character)
        {
            if (!_characters.Contains(character))
            {
                _characters.Add(character);
            }
            // In a real DB, update logic would go here. 
            // Since it's in-memory object reference, updates to the object persist.
        }

        public Character GetByName(string name)
        {
            return _characters.FirstOrDefault(c => c.Name == name);
        }

        public IEnumerable<Character> GetAll()
        {
            return _characters;
        }
    }
}
