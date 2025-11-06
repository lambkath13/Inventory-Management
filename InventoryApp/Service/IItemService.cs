using InventoryApp.Models;

namespace InventoryApp.Services;

public interface IItemService
{
    Task<List<Item>> GetAllAsync(int inventoryId, CancellationToken ct = default);
    Task<Item?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Item> CreateAsync(Item item, CancellationToken ct = default);
    Task<Item?> UpdateAsync(int id, Item updated, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
