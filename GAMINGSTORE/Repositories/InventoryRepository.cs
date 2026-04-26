using GAMINGSTORE.Models;
using GAMINGSTORE.Data;

namespace GAMINGSTORE.Repositories
{
    public interface IInventoryRepository
    {
        Task<Inventory> GetByProductIdAsync(int productId);
        Task<int> GetAvailableStockAsync(int productId);
        Task UpdateStockAsync(int productId, int quantity);
        Task AddAsync(Inventory inventory);
        Task<bool> HasSufficientStockAsync(int productId, int quantity);
    }

    public class InventoryRepository : IInventoryRepository
    {
        private readonly ApplicationDbContext _context;

        public InventoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Inventory> GetByProductIdAsync(int productId)
        {
            return await Task.FromResult(_context.Inventories
                .FirstOrDefault(i => i.ProductId == productId));
        }

        public async Task<int> GetAvailableStockAsync(int productId)
        {
            var inventory = await GetByProductIdAsync(productId);
            if (inventory == null) return 0;
            return inventory.StockQuantity - inventory.ReservedQuantity;
        }

        public async Task UpdateStockAsync(int productId, int quantity)
        {
            var inventory = await GetByProductIdAsync(productId);
            if (inventory != null)
            {
                inventory.StockQuantity -= quantity;
                inventory.LastSoldDate = DateTime.UtcNow;
                _context.Inventories.Update(inventory);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddAsync(Inventory inventory)
        {
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasSufficientStockAsync(int productId, int quantity)
        {
            var available = await GetAvailableStockAsync(productId);
            return available >= quantity;
        }
    }
}
