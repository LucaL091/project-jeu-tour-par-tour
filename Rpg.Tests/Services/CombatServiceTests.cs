using Xunit;
using Moq;
using Rpg.Core.Services;
using Rpg.Core.Interfaces;
using Rpg.Core.Models;

namespace Rpg.Tests.Services
{
    public class CombatServiceTests
    {
        [Fact]
        public void ProcessTurn_ShouldInflictDamage_WhenBothAlive()
        {
            // Arrange
            var mockLogger = new Mock<IActionLogger>();
            var mockStrategy = new Mock<IDamageStrategy>();
            
            var attackerStats = new Stats("Attacker", 1, 100, 10, 10, 10, 10, 10, 0);
            var defenderStats = new Stats("Defender", 1, 100, 10, 10, 10, 10, 10, 0);
            var attacker = new Hero(attackerStats);
            var defender = new Hero(defenderStats);

            mockStrategy.Setup(s => s.CalculateDamage(attacker, defender)).Returns(10);
            
            var service = new CombatService(mockLogger.Object, mockStrategy.Object);

            // Act
            service.ProcessTurn(attacker, defender);

            // Assert
            Assert.Equal(90, defender.Statistics.HP);
            mockLogger.Verify(l => l.Log(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public void ProcessTurn_ShouldNotInflictDamage_WhenAttackerDead()
        {
             // Arrange
            var mockLogger = new Mock<IActionLogger>();
            var mockStrategy = new Mock<IDamageStrategy>();
            
            var attackerStats = new Stats("Attacker", 1, 100, 10, 10, 10, 10, 10, 0);
            var defenderStats = new Stats("Defender", 1, 100, 10, 10, 10, 10, 10, 0);
            var attacker = new Hero(attackerStats);
            attacker.TakeDamage(100); // Process logic to make hp 0
            
            var defender = new Hero(defenderStats);

            var service = new CombatService(mockLogger.Object, mockStrategy.Object);

            // Act
            service.ProcessTurn(attacker, defender);

            // Assert
            Assert.Equal(100, defender.Statistics.HP); // No damage taken
            mockStrategy.Verify(s => s.CalculateDamage(It.IsAny<Character>(), It.IsAny<Character>()), Times.Never);
        }
    }
}
