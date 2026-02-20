using Rpg.Core.Interfaces;
using Rpg.Core.Models;

namespace Rpg.Core.Services
{
    public class InventaireService
    {
        private readonly IActionLogger _logger;

        public InventaireService(IActionLogger logger)
        {
            _logger = logger;
        }

        public void UseItem(IUsableItem item, Character target)
        {
            if (item == null) return;
            
            _logger.Log($"Using {item.Name} on {target.Name}.");
            item.Use(target);
        }
    }
}
