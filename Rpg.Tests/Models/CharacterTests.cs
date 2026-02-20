using Xunit;
using Rpg.Core.Models;

namespace Rpg.Tests.Models
{
    public class CharacterTests
    {
        [Fact]
        public void IsAlive_ShouldBeTrue_WhenHpAboveZero()
        {
            var stats = new Stats("Hero", 1, 100, 10, 10, 10, 10, 10, 0);
            var hero = new Hero(stats);

            Assert.True(hero.IsAlive);
        }

        [Fact]
        public void IsAlive_ShouldBeFalse_WhenHpIsZero()
        {
            var stats = new Stats("Hero", 1, 100, 10, 10, 10, 10, 10, 0);
            var hero = new Hero(stats);

            hero.TakeDamage(100);

            Assert.False(hero.IsAlive);
        }

        [Fact]
        public void Heal_ShouldIncreaseHp_NotExceedingMax()
        {
            var stats = new Stats("Hero", 1, 100, 10, 10, 10, 10, 10, 0);
            var hero = new Hero(stats);
            
            hero.TakeDamage(50);
            Assert.Equal(50, hero.Statistics.HP);

            hero.Heal(20);
            Assert.Equal(70, hero.Statistics.HP);

            hero.Heal(100);
            Assert.Equal(100, hero.Statistics.HP); // Should be capped at MaxHP
        }
    }
}
