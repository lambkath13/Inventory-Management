using InventoryApp.Models;

namespace InventoryApp.Services;

public interface ITagService
{
    Task<List<Tag>> AutocompleteAsync(string prefix, int take = 20, CancellationToken ct = default);
    Task<List<(string Tag, int Count)>> CloudAsync(int take = 30, CancellationToken ct = default);
    Task<List<Tag>> GetForInventoryAsync(int inventoryId, CancellationToken ct = default);
    Task AddToInventoryAsync(int inventoryId, string tagName, CancellationToken ct = default);
    Task RemoveFromInventoryAsync(int inventoryId, string tagName, CancellationToken ct = default);
}
