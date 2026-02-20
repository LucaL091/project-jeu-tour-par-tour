using System.Collections.Generic;
using Rpg.Core.Interfaces;
using Rpg.Core.Models;

namespace Rpg.Core.Services
{
    public class PersonnageService
    {
        private readonly IPersonnageRepository _repository;

        public PersonnageService(IPersonnageRepository repository)
        {
            _repository = repository;
        }

        public void SaveCharacter(Character character)
        {
            _repository.Save(character);
        }

        public IEnumerable<Character> GetAllCharacters()
        {
            return _repository.GetAll();
        }
    }
}
