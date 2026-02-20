using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Rpg.Core.Interfaces;
using Rpg.Core.Models;

namespace Rpg.Core.Repositories
{
    /// <summary>
    /// Implémentation du dépôt (Repository) utilisant un fichier JSON pour la persistance.
    /// </summary>
    public class JsonPersonnageRepository : IPersonnageRepository
    {
        private readonly string _filePath;

        /// <summary>
        /// Initialise le dépôt avec le chemin du fichier de sauvegarde.
        /// </summary>
        /// <param name="filePath">Nom du fichier JSON (par défaut characters.json).</param>
        public JsonPersonnageRepository(string filePath = "characters.json")
        {
            _filePath = filePath;
        }

        /// <summary>
        /// Sauvegarde ou met à jour un personnage dans le fichier JSON.
        /// </summary>
        public void Save(Character character)
        {
            var characters = GetAllInternal();
            // On cherche si un personnage du même nom existe déjà pour le remplacer
            var existing = characters.Find(c => c.Name == character.Name);
            if (existing != null) characters.Remove(existing);
            characters.Add(character);
            
            // Sérialisation avec indentation pour la lisibilité
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

        /// <summary>
        /// Charge tous les personnages depuis le fichier JSON.
        /// </summary>
        private List<Character> GetAllInternal()
        {
            if (!File.Exists(_filePath)) return new List<Character>();
            
            string json = File.ReadAllText(_filePath);
            try
            {
                // Désérialisation en liste de Hero. 
                // Note : En cas de polymorphisme complexe (Monstres sauvés aussi), 
                // il faudrait un convertisseur JSON personnalisé (JsonConverter).
                return JsonSerializer.Deserialize<List<Hero>>(json)?.Cast<Character>().ToList() ?? new List<Character>();
            }
            catch (Exception)
            {
                // En cas d'erreur de lecture/format, on retourne une liste vide pour ne pas faire planter le jeu
                return new List<Character>();
            }
        }
    }
}
