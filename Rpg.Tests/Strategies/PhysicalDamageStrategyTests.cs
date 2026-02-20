using Xunit;
using Rpg.Core.Models;
using Rpg.Core.Strategies;

namespace Rpg.Tests.Strategies
{
    public class PhysicalDamageStrategyTests
    {
        [Fact]
        public void CalculateDamage_ShouldReturnCorrectValue()
        {
            // Arrange
            var attackerStats = new Stats("Attacker", 1, 100, 10, 20, 10, 10, 10, 0); // Strength 20
            var defenderStats = new Stats("Defender", 1, 100, 10, 10, 10, 10, 5, 0);  // Defense 5
            var attacker = new Hero(attackerStats); 
            var defender = new Hero(defenderStats);
            var strategy = new PhysicalDamageStrategy();

            // Act
            int damage = strategy.CalculateDamage(attacker, defender);

            // Assert
            Assert.Equal(15, damage); // 20 - 5 = 15
        }

        [Fact]
        public void CalculateDamage_ShouldNotReturnNegative()
        {
            // Arrange
            var attackerStats = new Stats("Attacker", 1, 100, 10, 5, 10, 10, 10, 0); // Strength 5
            var defenderStats = new Stats("Defender", 1, 100, 10, 10, 10, 10, 10, 0); // Defense 10
            var attacker = new Hero(attackerStats);
            var defender = new Hero(defenderStats);
            var strategy = new PhysicalDamageStrategy();

            // Act
            int damage = strategy.CalculateDamage(attacker, defender);

            // Assert
            Assert.Equal(0, damage);
        }
    }
}
