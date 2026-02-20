using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Rpg.Core.Interfaces;
using Rpg.Core.Models;

namespace Rpg.Core.Repositories
{
    public class JsonPersonnageRepository : IPersonnageRepository
    {
        private readonly string _filePath;

        public JsonPersonnageRepository(string filePath = "characters.json")
        {
            _filePath = filePath;
        }

        public void Save(Character character)
        {
            var characters = GetAllInternal();
            var existing = characters.Find(c => c.Name == character.Name);
            if (existing != null) characters.Remove(existing);
            characters.Add(character);
            
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(characters, options);
            File.WriteAllText(_filePath, json);
        }

        public Character GetByName(string name)
        {
            var characters = GetAllInternal();
            return characters.Find(c => c.Name == name);
        }

        public IEnumerable<Character> GetAll()
        {
            return GetAllInternal();
        }

        private List<Character> GetAllInternal()
        {
            if (!File.Exists(_filePath)) return new List<Character>();
            
            string json = File.ReadAllText(_filePath);
            try
            {
                // Note: JsonSerializer might need help with polymorphic types if we had complex inheritance.
                // For Hero/Monster, we might need a custom converter if we want to distinguish them easily.
                // But for now, let's assume we primarily save Heroes.
                return JsonSerializer.Deserialize<List<Hero>>(json)?.Cast<Character>().ToList() ?? new List<Character>();
            }
            catch
            {
                return new List<Character>();
            }
        }
    }
}
