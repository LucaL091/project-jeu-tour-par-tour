using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Rpg.Core.Factories;
using Rpg.Core.Interfaces;
using Rpg.Core.Models;
using Rpg.Core.Models.Items;
using Rpg.Core.Services;
using Rpg.Core.Models.Skills;
using System.Collections.Generic;
using Rpg.Core.Strategies;
using Rpg.Wpf.Commands;

namespace Rpg.Wpf.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged, ICombatObserver
    {
        private readonly PersonnageService _personnageService;
        private readonly CombatService _combatService;
        private readonly MonsterFactory _monsterFactory;
        
        private Hero _hero;
        private Monster _monster;
        private int _floor;
        private bool _isGameRunning;
        private bool _isPlayerTurn;

        // Character Creation
        private string _playerName = "Heros";
        private int _selectedClassIndex;
        private int _selectedDifficultyIndex = 1; // Normal default
        private Stats _previewStats; // For UI Preview
        private Weapon _selectedWeapon;
        private ObservableCollection<Weapon> _availableWeapons = new ObservableCollection<Weapon>();

        // Menus
        private bool _showSkills;
        private bool _showInventory;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> GameLogs { get; } = new ObservableCollection<string>();
        
        public ObservableCollection<string> Classes { get; } = new ObservableCollection<string>
        {
            "Guerrier (Robuste/Fort)", 
            "Mage (Magique/Intelligent)", 
            "Archer (Agile/Distance)", 
            "Voleur (Rapide/Mortel)", 
            "Paladin (Sacre/Resistant)" 
        };

        public ObservableCollection<string> Difficulties { get; } = new ObservableCollection<string>
        {
            "Facile", "Normal", "Difficile"
        };

        public ObservableCollection<Weapon> AvailableWeapons
        {
            get => _availableWeapons;
            set { _availableWeapons = value; OnPropertyChanged(); }
        }

        public string PlayerName
        {
            get => _playerName;
            set { _playerName = value; OnPropertyChanged(); }
        }

        public int SelectedClassIndex
        {
            get => _selectedClassIndex;
            set 
            { 
                _selectedClassIndex = value; 
                OnPropertyChanged(); 
                UpdateAvailableWeapons();
                UpdatePreviewStats(); 
            }
        }

        public int SelectedDifficultyIndex
        {
            get => _selectedDifficultyIndex;
            set { _selectedDifficultyIndex = value; OnPropertyChanged(); }
        }
        
        public Stats PreviewStats
        {
            get => _previewStats;
            set { _previewStats = value; OnPropertyChanged(); }
        }

        public Weapon SelectedWeapon
        {
            get => _selectedWeapon;
            set 
            { 
                _selectedWeapon = value; 
                OnPropertyChanged(); 
                UpdatePreviewStats(); 
            }
        }

        public bool ShowSkills
        {
            get => _showSkills;
            set { _showSkills = value; OnPropertyChanged(); }
        }

        public bool ShowInventory
        {
            get => _showInventory;
            set { _showInventory = value; OnPropertyChanged(); }
        }

        public Hero Hero
        {
            get => _hero;
            set { _hero = value; OnPropertyChanged(); }
        }

        public Monster Monster
        {
            get => _monster;
            set { _monster = value; OnPropertyChanged(); }
        }

        public int Floor
        {
            get => _floor;
            set { _floor = value; OnPropertyChanged(); }
        }

        public bool IsGameRunning
        {
            get => _isGameRunning;
            set { _isGameRunning = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowStartMenu)); }
        }

        public bool ShowStartMenu => !IsGameRunning;

        public bool IsPlayerTurn
        {
            get => _isPlayerTurn;
            set 
            { 
                _isPlayerTurn = value; 
                OnPropertyChanged(); 
                
                // Force UI to re-evaluate Command CanExecute
                Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
                {
                    CommandManager.InvalidateRequerySuggested();
                }));
            }
        }

        public ICommand StartGameCommand { get; }
        public ICommand AttackCommand { get; }
        public ICommand UseItemCommand { get; } // Now just toggles menu
        public ICommand UseSkillCommand { get; } // Now just toggles menu
        public ICommand FleeCommand { get; }
        public ICommand SelectSkillCommand { get; }
        public ICommand SelectItemCommand { get; }

        public MainViewModel()
        {
            // Initialize Services
            var repo = new Rpg.Core.Repositories.JsonPersonnageRepository();
            _personnageService = new PersonnageService(repo);
            var damageStrategy = new PhysicalDamageStrategy();
            _combatService = new CombatService(damageStrategy);
            _combatService.Attach(this); // Register as observer
            _monsterFactory = new MonsterFactory(_combatService);

            // Initialize Commands
            // Initialize Commands
            StartGameCommand = new RelayCommand(StartGame);
            AttackCommand = new RelayCommand(async _ => await AttackAsync(), _ => IsGameRunning && IsPlayerTurn && Monster != null && Monster.IsAlive);
            
            // Toggle Menus
            UseSkillCommand = new RelayCommand(_ => { ShowSkills = !ShowSkills; ShowInventory = false; }, _ => IsGameRunning && IsPlayerTurn);
            UseItemCommand = new RelayCommand(_ => { ShowInventory = !ShowInventory; ShowSkills = false; }, _ => IsGameRunning && IsPlayerTurn);
            
            FleeCommand = new RelayCommand(async _ => await FleeAsync(), _ => IsGameRunning && IsPlayerTurn);

            // Selection Commands
            SelectSkillCommand = new RelayCommand(async param => await UseSkillAsync(param as Skill), param => IsGameRunning && IsPlayerTurn && param is Skill);
            // Selection Commands
            SelectSkillCommand = new RelayCommand(async param => await UseSkillAsync(param as Skill), param => IsGameRunning && IsPlayerTurn && param is Skill);
            SelectItemCommand = new RelayCommand(async param => await UseItemAsync(param as IUsableItem), param => IsGameRunning && IsPlayerTurn && param is IUsableItem);
            
            UpdateAvailableWeapons(); // Init weapons
            UpdatePreviewStats(); // Init preview
        }

        private void UpdateAvailableWeapons()
        {
            AvailableWeapons.Clear();
            switch (SelectedClassIndex)
            {
                case 0: // Warrior
                    AvailableWeapons.Add(new Weapon("Epee a deux mains (Phys +8, PV +10)", 8, "Sword", 10));
                    AvailableWeapons.Add(new Weapon("Hache de guerre (Phys +6, PV +20)", 6, "Axe", 20));
                    break;
                case 1: // Mage
                    AvailableWeapons.Add(new Weapon("Baton (Magie +5, PM +20)", 5, "Staff", 0, 20));
                    AvailableWeapons.Add(new Weapon("Dague enchantee (Phys +3, PM +10)", 3, "Dagger", 0, 10));
                    break;
                case 2: // Archer
                    AvailableWeapons.Add(new Weapon("Arc long (Phys +6, Agi +3, PV +10)", 6, "Bow", 10, 0, 3));
                    AvailableWeapons.Add(new Weapon("Arbalete (Phys +7, PV +5, Def +2)", 7, "Bow", 5, 0, 0, 2));
                    break;
                case 3: // Rogue
                    AvailableWeapons.Add(new Weapon("Dagues jumelles (Phys +5, Agi +5)", 5, "Dagger", 0, 0, 5));
                    AvailableWeapons.Add(new Weapon("Epee courte (Phys +4, PV +10, Def +2)", 4, "Sword", 10, 0, 0, 2));
                    break;
                case 4: // Paladin
                    AvailableWeapons.Add(new Weapon("Marteau de guerre (Phys +7, PV +15)", 7, "Hammer", 15));
                    AvailableWeapons.Add(new Weapon("Masse & Bouclier (Phys +4, PV +30)", 4, "Mace", 30));
                    break;
            }
            SelectedWeapon = AvailableWeapons.FirstOrDefault();
        }

        private void UpdatePreviewStats()
        {
            var baseStats = GetClassStats(SelectedClassIndex, PlayerName ?? "Heros");
            if (SelectedWeapon != null)
            {
                // Apply visual preview of bonuses (simplified)
                // Note: Real stats application happens in StartGame/EquipWeapon
                // We create a new stats struct to show valid totals
                 PreviewStats = new Stats(
                    baseStats.Name,
                    baseStats.Level,
                    baseStats.MaxHP + SelectedWeapon.HpBonus,
                    baseStats.MaxMP + SelectedWeapon.MpBonus,
                    baseStats.Strength + SelectedWeapon.DamageBonus,
                    baseStats.Intelligence,
                    baseStats.Agility + SelectedWeapon.AgiBonus,
                    baseStats.Defense + SelectedWeapon.DefBonus,
                    baseStats.MagicResistance
                );
                // Since Stats is a struct and we want to show current HP/MP as full
                // We re-set current to max
                // (Assuming Stats constructor sets Current=Max)
            }
            else
            {
                PreviewStats = baseStats;
            }
        }

        private void StartGame(object parameter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(PlayerName)) PlayerName = "Heros";

                // Create Stats based on selection
                Stats stats = GetClassStats(SelectedClassIndex, PlayerName);
                Hero = new Hero(stats);

                // Add Class Specifics (Skills/Weapons) - Simplified logic from CombatUI
                if (SelectedClassIndex == 1) // Mage
                {
                    Hero.Skills.Add(new Fireball());
                    Hero.Skills.Add(new Heal());
                }
                else if (SelectedClassIndex == 4) // Paladin
                {
                    Hero.Skills.Add(new Heal());
                }

                // Equip selected weapon
                if (SelectedWeapon != null)
                {
                    Hero.EquipWeapon(SelectedWeapon);
                }
                else
                {
                    // Fallback
                    Hero.EquipWeapon(new Weapon("Epee simple", 5, "Sword"));
                }

                Hero.Inventory.Add(new HealthPotion());
                Hero.Inventory.Add(new HealthPotion());
                Hero.Inventory.Add(new ManaPotion());
                
                Floor = 1;
                IsGameRunning = true;
                GameLogs.Clear();
                OnAction("Bienvenue dans le donjon !");
                
                StartNextFloor();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du démarrage du jeu : {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Erreur Critique", MessageBoxButton.OK, MessageBoxImage.Error);
                IsGameRunning = false;
            }
        }

        private void StartNextFloor()
        {
            if (!Hero.IsAlive) return;
            
            // Save Progress automatically
            _personnageService.SaveCharacter(Hero);

            Monster = _monsterFactory.CreateRandomMonster(Floor, (Difficulty)SelectedDifficultyIndex);
            OnAction($"--- ETAGE {Floor} ---");
            OnAction($"Un {Monster.Name} sauvage apparait !");
            IsPlayerTurn = true;
        }

        private async Task AttackAsync()
        {
            IsPlayerTurn = false;
            
            // Player Turn
            _combatService.ProcessTurn(Hero, Monster);
            RefreshStats();

            if (!Monster.IsAlive)
            {
                OnAction($"{Monster.Name} est vaincu !");
                await Task.Delay(1000);
                Floor++;
                StartNextFloor();
                return;
            }

            // Enemy Turn
            await Task.Delay(1000); // Simulate thinking time
            OnAction("--- Tour Ennemi ---");
            Monster.ExecuteTurn(Hero);
            RefreshStats();

            if (!Hero.IsAlive)
            {
                OnAction("Vous etes mort...");
                IsGameRunning = false;
                MessageBox.Show("Game Over!", "Perdu", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                IsPlayerTurn = true;
            }
        }

        private async Task UseItemAsync(IUsableItem item)
        {
             ShowInventory = false;
             IsPlayerTurn = false;
             
             if (item != null)
             {
                 OnAction($"Vous utilisez {item.Name}!");
                 item.Use(Hero);
                 Hero.Inventory.Remove(item);
                 RefreshStats();
             }

             // Enemy Turn
             await Task.Delay(1000);
             Monster.ExecuteTurn(Hero);
             RefreshStats();
             
             if (!Hero.IsAlive)
             {
                  OnAction("Vous etes mort...");
                  IsGameRunning = false;
             }
             else
             {
                 IsPlayerTurn = true;
             }
        }

        private async Task UseSkillAsync(Skill skill)
        {
             ShowSkills = false;
             
             if (skill == null) return;

             if (Hero.Statistics.MP < skill.ManaCost)
             {
                 OnAction("Pas assez de mana !");
                 return;
             }

             IsPlayerTurn = false;
             _combatService.ProcessSkill(Hero, skill, skill.IsSupport ? (Character)Hero : Monster);
             RefreshStats();

             if (!skill.IsSupport && !Monster.IsAlive)
             {
                 OnAction($"{Monster.Name} est vaincu !");
                 await Task.Delay(1000);
                 Floor++;
                 StartNextFloor();
                 return;
             }

             // Enemy Turn
             await Task.Delay(1000);
             Monster.ExecuteTurn(Hero);
             RefreshStats();
             
             if (!Hero.IsAlive)
             {
                  OnAction("Vous etes mort...");
                  IsGameRunning = false;
             }
             else
             {
                 IsPlayerTurn = true;
             }
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

        private async Task FleeAsync()
        {
            OnAction($"{Hero.Name} tente de fuir...");
            IsPlayerTurn = false;
            await Task.Delay(1000);
            
            // Simple 50% chance
            if (new Random().Next(0, 2) == 0)
            {
                 OnAction("Fuite réussie !");
                 IsGameRunning = false;
                 MessageBox.Show("Vous avez fui lachement !", "Fuite", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                 OnAction("Echec de la fuite !");
                 await Task.Delay(500);
                 Monster.ExecuteTurn(Hero);
                 RefreshStats();
                 
                 if (!Hero.IsAlive)
                 {
                    OnAction("Vous etes mort...");
                    IsGameRunning = false;
                 }
                 else
                 {
                     IsPlayerTurn = true;
                 }
            }
        }

        // Helper to force UI update on deep properties
        private void RefreshStats()
        {
            OnPropertyChanged(nameof(Hero));
            OnPropertyChanged(nameof(Monster));
        }

        public void OnAction(string message)
        {
            // Ensure UI thread access
            Application.Current.Dispatcher.Invoke(() =>
            {
                GameLogs.Insert(0, message); // Add to top
            });
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
