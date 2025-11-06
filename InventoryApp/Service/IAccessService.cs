using InventoryApp.Models;

namespace InventoryApp.Services;

public interface IAccessService
{
    Task<List<InventoryAccess>> ListAsync(int inventoryId, CancellationToken ct = default);
    Task<InventoryAccess> GrantAsync(int inventoryId, string userId, bool canWrite, CancellationToken ct = default);
    Task<bool> RevokeAsync(int inventoryId, string userId, CancellationToken ct = default);
}
