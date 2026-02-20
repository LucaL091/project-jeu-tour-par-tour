using System;
using System.Collections.Generic;
using System.Linq;
using Rpg.Core.Models;
using Rpg.Core.Models.Items;
using Rpg.Core.Models.Skills;
using Rpg.Core.Services;
using Rpg.Core.Strategies;
using Rpg.Core.Interfaces;
using Rpg.Core.Factories;

namespace Rpg.Client
{
    public enum BattleResult
    {
        Victory,
        Defeat,
        Fled
    }

    public class CombatUI
    {
        private readonly PersonnageService _personnageService;
        private readonly CombatService _combatService;
        private readonly MemoryLogger _logger;
        private readonly MonsterFactory _monsterFactory;
        private readonly Random _random = new Random();

        public CombatUI(PersonnageService personnageService, CombatService combatService, MemoryLogger logger)
        {
            _personnageService = personnageService;
            _combatService = combatService;
            _logger = logger;
            _monsterFactory = new MonsterFactory(combatService);
        }

        public void StartGame()
        {
            Console.CursorVisible = false;
            Console.Clear();
            PrintHeader();

            string[] diffOptions = { "Facile (Ennemis 0.8x stats)", "Normal (Standard)", "Difficile (Ennemis 1.5x stats)" };
            int diffIndex = GetMenuSelection("Chosir la difficulte :", diffOptions);
            Difficulty difficulty = (Difficulty)diffIndex;

            Hero hero = CreateHero();
            _personnageService.SaveCharacter(hero);

            Console.WriteLine($"\nBienvenue, {hero.Name}! Preparez vous au combat.");
            System.Threading.Thread.Sleep(1000);

            int floor = 1;
            while (hero.IsAlive)
            {
                Monster monster = CreateMonster(floor, difficulty);
                // Console.Clear(); // Handled by RenderCombatFrame
                // DrawBorder($"FLOOR {floor}"); // Handled by RenderCombatFrame
                _logger.Log($"--- ETAGE {floor} ---");
                _logger.Log($"Un {monster.Name} sauvage apparait!");
                // System.Threading.Thread.Sleep(1000); // Removed delay

                BattleResult result = GameLoop(hero, monster);

                if (result == BattleResult.Defeat)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nPerdu. Appuyez sur une touche pour quitter.");
                    Console.ResetColor();
                    Console.ReadKey(true);
                    break;
                }
                else if (result == BattleResult.Fled)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\nVous avez fui le combat ! En route vers le prochain etage...");
                    Console.ResetColor();
                    System.Threading.Thread.Sleep(1500);
                }
                else // Victory
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nVictoire! Appuyez sur Entree pour continuer au prochain etage...");
                    Console.ResetColor();
                    while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
                }
                
                floor++;
            }
        }

        private void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
  _____  _____   _____   _____                 _           _ 
 |  __ \|  __ \ / ____| |  __ \               | |         | |
 | |__) | |__) | |  __  | |__) | __ ___  _   _| | ___  ___| |_ 
 |  _  /|  ___/| | |_ | |  ___/ '__/ _ \| | | | |/ _ \/ __| __|
 | | \ \| |    | |__| | | |   | | | (_) | |_| | |  __/ (__| |_ 
 |_|  \_\_|     \_____| |_|   |_|  \___/ \__,_|_|\___|\___|\__|
                                                               
");
            Console.ResetColor();
        }

        private void DrawBorder(string text)
        {
            string border = new string('=', text.Length + 4);
            Console.WriteLine(border);
            Console.WriteLine($"| {text} |");
            Console.WriteLine(border);
        }

        private void DrawProgressBar(string label, int current, int max, ConsoleColor color)
        {
            Console.Write($"{label}: ");
            Console.ForegroundColor = color;
            Console.Write("[");
            
            int totalWidth = 20;
            double percent = (double)current / max;
            int filled = (int)(totalWidth * percent);

            for (int i = 0; i < totalWidth; i++)
            {
                if (i < filled) Console.Write("█");
                else Console.Write("░");
            }
            Console.Write($"] {current}/{max}");
            Console.ResetColor();
            Console.WriteLine();
        }

        private Hero CreateHero()
        {
            Console.Write("Entrez le nom de votre héros : ");
            string name = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(name))
            {
                 Console.Write("Le nom ne peut pas être vide. Entrez le nom de votre héros : ");
                 name = Console.ReadLine();
            }

            Hero hero = null;
            while (hero == null)
            {
                string[] classes = { 
                    "Guerrier (Robuste/Fort)", 
                    "Mage (Magique/Intelligent)", 
                    "Archer (Agile/Distance)", 
                    "Voleur (Rapide/Mortel)", 
                    "Paladin (Sacre/Resistant)" 
                };
                int choice = GetMenuSelection("Choisissez votre classe:", classes, idx => {
                    Stats s = GetClassStats(idx, name);
                    DrawProgressBar("PV", s.HP, s.MaxHP, ConsoleColor.Red);
                    DrawProgressBar("PM", s.MP, s.MaxMP, ConsoleColor.Blue);
                });

                Stats stats = GetClassStats(choice, name);
                hero = new Hero(stats);

                // Skills
                if (choice == 1) // Mage gets spells
                {
                    hero.Skills.Add(new Fireball());
                    hero.Skills.Add(new Heal());
                }
                else if (choice == 4) // Paladin gets Heal
                {
                    hero.Skills.Add(new Heal());
                }

                List<Weapon> availableWeapons = new List<Weapon>();
                switch (choice)
                {
                    case 0: // Warrior
                        availableWeapons.Add(new Weapon("Epee a deux mains (Phys +8, PV +10)", 8, "Sword", 10));
                        availableWeapons.Add(new Weapon("Hache de guerre (Phys +6, PV +20)", 6, "Axe", 20));
                        break;
                    case 1: // Mage
                        availableWeapons.Add(new Weapon("Baton (Magie +5, PM +20)", 5, "Staff", 0, 20));
                        availableWeapons.Add(new Weapon("Dague enchantee (Phys +3, PM +10)", 3, "Dagger", 0, 10));
                        break;
                    case 2: // Archer
                        availableWeapons.Add(new Weapon("Arc long (Phys +6, Agi +3, PV +10)", 6, "Bow", 10, 0, 3));
                        availableWeapons.Add(new Weapon("Arbalete (Phys +7, PV +5, Def +2)", 7, "Bow", 5, 0, 0, 2));
                        break;
                    case 3: // Rogue
                        availableWeapons.Add(new Weapon("Dagues jumelles (Phys +5, Agi +5)", 5, "Dagger", 0, 0, 5));
                        availableWeapons.Add(new Weapon("Epee courte (Phys +4, PV +10, Def +2)", 4, "Sword", 10, 0, 0, 2));
                        break;
                    case 4: // Paladin
                        availableWeapons.Add(new Weapon("Marteau de guerre (Phys +7, PV +15)", 7, "Hammer", 15));
                        availableWeapons.Add(new Weapon("Masse & Bouclier (Phys +4, PV +30)", 4, "Mace", 30));
                        break;
                    default:
                        availableWeapons.Add(new Weapon("Epee rouillee (Phys +2)", 2, "Sword"));
                        break;
                }

                List<string> weaponOptions = availableWeapons.Select(w => w.Name).ToList();
                weaponOptions.Add("Retour (Changer de classe)");

                int weaponChoice = GetMenuSelection("Choisissez votre arme de depart:", weaponOptions.ToArray(), idx => {
                    int projHP = hero.Statistics.HP;
                    int projMaxHP = hero.Statistics.MaxHP;
                    int projMP = hero.Statistics.MP;
                    int projMaxMP = hero.Statistics.MaxMP;
                    Weapon w = null;

                    if (idx < availableWeapons.Count)
                    {
                        w = availableWeapons[idx];
                        projHP += w.HpBonus;
                        projMaxHP += w.HpBonus;
                        projMP += w.MpBonus;
                        projMaxMP += w.MpBonus;
                    }

                    DrawProgressBar("PV", projHP, projMaxHP, ConsoleColor.Red);
                    DrawProgressBar("PM", projMP, projMaxMP, ConsoleColor.Blue);
                    
                    if (w != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        string type = (choice == 1 && idx == 0) ? "Magie" : "Physique";
                        Console.WriteLine($"Apercu: +{w.DamageBonus} {type}");
                        if (w.HpBonus > 0) Console.WriteLine($"Bonus PV: +{w.HpBonus}");
                        if (w.MpBonus > 0) Console.WriteLine($"Bonus PM: +{w.MpBonus}");
                        if (w.AgiBonus > 0) Console.WriteLine($"Bonus Agilite: +{w.AgiBonus}");
                        if (w.DefBonus > 0) Console.WriteLine($"Bonus Defense: +{w.DefBonus}");
                        Console.ResetColor();
                    }
                });

                if (weaponChoice == weaponOptions.Count - 1)
                {
                    hero = null; // Back to start of creation loop
                    continue;
                }

                Weapon startingWeapon = availableWeapons[weaponChoice];
                hero.EquipWeapon(startingWeapon);
            }

            hero.Inventory.Add(new HealthPotion());
            hero.Inventory.Add(new HealthPotion());
            hero.Inventory.Add(new ManaPotion());

            return hero;
        }

        private Stats GetClassStats(int choice, string name)
        {
            switch (choice)
            {
                case 0: return new Stats(name, 1, 120, 20, 10, 5, 8, 8, 2);
                case 1: return new Stats(name, 1, 70, 60, 4, 15, 10, 3, 8);
                case 2: return new Stats(name, 1, 90, 30, 8, 8, 15, 5, 4);
                case 3: return new Stats(name, 1, 80, 40, 7, 10, 18, 4, 5);
                case 4: return new Stats(name, 1, 140, 40, 9, 8, 5, 12, 6);
                default: return new Stats(name, 1, 100, 50, 10, 10, 10, 5, 5);
            }
        }

        private Monster CreateMonster(int floor, Difficulty difficulty)
        {
            return _monsterFactory.CreateRandomMonster(floor, difficulty);
        }

        private int GetMenuSelection(string title, string[] options, Action<int> onHighlight = null)
        {
            int selectedIndex = 0;
            while (true)
            {
                Console.Clear();
                Console.Write("\x1b[3J\x1b[2J\x1b[H"); // Robust clear
                
                if (!string.IsNullOrEmpty(title))
                {
                    Console.WriteLine(title);
                    Console.WriteLine(new string('-', title.Length));
                }

                if (onHighlight != null)
                {
                    onHighlight(selectedIndex);
                    Console.WriteLine();
                }

                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($"> {options[i]}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {options[i]}");
                    }
                }

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    selectedIndex--;
                    if (selectedIndex < 0) selectedIndex = options.Length - 1;
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    selectedIndex++;
                    if (selectedIndex >= options.Length) selectedIndex = 0;
                }
                else if (char.IsDigit(keyInfo.KeyChar))
                {
                    int val = int.Parse(keyInfo.KeyChar.ToString());
                    if (val >= 1 && val <= options.Length)
                    {
                        return val - 1;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    return selectedIndex;
                }
            }
        }

        private BattleResult GameLoop(Hero hero, Monster monster)
        {
            // Initial render
            RenderCombatFrame(hero, monster);

            while (hero.IsAlive && monster.IsAlive)
            {
                string[] actions = { "Attaquer", "Competences", "Objets", "Fuir" };
                
                int selectedAction = CombatMenu(hero, monster, actions, "Choisissez une action:");

                if (selectedAction == 0) // Attack
                {
                    _combatService.ProcessTurn(hero, monster);
                    RenderCombatFrame(hero, monster); // Show attack result
                    System.Threading.Thread.Sleep(1000); // Pause for impact
                }
                else if (selectedAction == 1) // Skills
                {
                    if (hero.Skills.Count == 0)
                    {
                        ShowMessage("Vous n'avez pas de competence!");
                        continue;
                    }

                    List<string> skillNames = new List<string>();
                    foreach (var s in hero.Skills) skillNames.Add($"{s.Name} ({s.ManaCost} MP)");
                    skillNames.Add("Retour");

                    int skillIdx = CombatMenu(hero, monster, skillNames.ToArray(), "Choisissez une competence :");
                    if (skillIdx == skillNames.Count - 1) continue; // Back

                    Skill skill = hero.Skills[skillIdx];
                    Character target = skill.IsSupport ? (Character)hero : monster;
                    _combatService.ProcessSkill(hero, skill, target);
                    
                    RenderCombatFrame(hero, monster); // Show skill result
                    System.Threading.Thread.Sleep(1000); // Pause for impact
                }
                else if (selectedAction == 2) // Items
                {
                    if (hero.Inventory.Count == 0)
                    {
                        ShowMessage("Inventaire vide!");
                        continue;
                    }

                    List<string> itemNames = new List<string>();
                    foreach (var i in hero.Inventory) itemNames.Add(i.Name);
                    itemNames.Add("Retour");

                    int itemIdx = CombatMenu(hero, monster, itemNames.ToArray(), "Inventaire :");
                    if (itemIdx == itemNames.Count - 1) continue; // Back

                    IUsableItem item = hero.Inventory[itemIdx];
                    hero.Inventory.RemoveAt(itemIdx);
                    Console.WriteLine($"\nVous utilisez {item.Name}!");
                    item.Use(hero);
                    
                    RenderCombatFrame(hero, monster); // Show item result
                    System.Threading.Thread.Sleep(1000); // Pause for impact
                }
                else if (selectedAction == 3) // Flee
                {
                    int fleeChance = 60 + (hero.Statistics.Agility - monster.Statistics.Agility) * 5;
                    fleeChance = Math.Max(10, Math.Min(95, fleeChance));
                    
                    if (_random.Next(1, 101) <= fleeChance)
                    {
                        _logger.Log($"\n{hero.Name} a reussi a s'enfuir !");
                        return BattleResult.Fled;
                    }
                    else
                    {
                        _logger.Log($"\n{hero.Name} tente de fuir mais echoue !");
                        RenderCombatFrame(hero, monster);
                        System.Threading.Thread.Sleep(1000);
                    }
                }

                if (!monster.IsAlive) return BattleResult.Victory;

                // Monster Turn
                _logger.Log("\n--- Tour Ennemi ---");
                RenderCombatFrame(hero, monster); 
                System.Threading.Thread.Sleep(500); // Short pause before enemy acts

                monster.ExecuteTurn(hero);
                RenderCombatFrame(hero, monster); // Show enemy attack result
                System.Threading.Thread.Sleep(1000); // Pause for impact

                if (!hero.IsAlive) return BattleResult.Defeat;
            }
            return monster.IsAlive ? BattleResult.Defeat : BattleResult.Victory;
        }

        private void RenderCombatFrame(Hero hero, Monster monster)
        {
            Console.Clear();
            Console.Write("\x1b[3J"); // Clear scrollback
            Console.Write("\x1b[2J"); // Clear screen
            Console.Write("\x1b[H");  // Cursor home
            try { Console.SetCursorPosition(0, 0); } catch { }
            
            // Compact Stats
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"=== COMBAT: {hero.Name} vs {monster.Name} ===");
            Console.ResetColor();

            DrawProgressBar("HP", hero.Statistics.HP, hero.Statistics.MaxHP, ConsoleColor.Red);
            DrawProgressBar("MP", hero.Statistics.MP, hero.Statistics.MaxMP, ConsoleColor.Blue);
            
            Console.WriteLine(); 
            Console.Write($"VS {monster.Name}: ");
            DrawProgressBar("HP", monster.Statistics.HP, monster.Statistics.MaxHP, ConsoleColor.DarkRed);
            
            // Fixed Log Area (5 lines) to stop jumping
            Console.WriteLine(new string('-', 40));
            var logs = _logger.GetLogs();
            for (int i = 0; i < 5; i++)
            {
                if (i < logs.Count) Console.WriteLine(logs[i]);
                else Console.WriteLine(" "); // Keep layout stable
            }
            Console.WriteLine(new string('-', 40));
        }

        private void ShowMessage(string message)
        {
            _logger.Log(message);
        }

        private int CombatMenu(Hero hero, Monster monster, string[] options, string menuTitle)
        {
            int selectedIndex = 0;
            while (true)
            {
                RenderCombatFrame(hero, monster);
                
                Console.WriteLine($"\n{menuTitle}");

                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($"> {options[i]}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {options[i]}");
                    }
                }

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    selectedIndex--;
                    if (selectedIndex < 0) selectedIndex = options.Length - 1;
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    selectedIndex++;
                    if (selectedIndex >= options.Length) selectedIndex = 0;
                }
                else if (char.IsDigit(keyInfo.KeyChar))
                {
                    int val = int.Parse(keyInfo.KeyChar.ToString());
                    if (val >= 1 && val <= options.Length)
                    {
                        return val - 1;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    return selectedIndex;
                }
            }
        }
    }
}
