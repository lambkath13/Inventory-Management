using InventoryApp.Models;

namespace InventoryApp.Services;

public interface ISearchService
{
    Task<(List<InventoryEntity> Inventories, List<Item> Items)> SearchAsync(string query, CancellationToken ct = default);
}
