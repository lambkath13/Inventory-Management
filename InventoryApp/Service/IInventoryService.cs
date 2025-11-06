using InventoryApp.Models;

namespace InventoryApp.Services;

public interface IInventoryService
{
    Task<List<InventoryEntity>> GetAllAsync(CancellationToken ct = default);
    Task<InventoryEntity?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<InventoryEntity> CreateAsync(InventoryEntity entity, CancellationToken ct = default);
    Task<InventoryEntity?> UpdateAsync(int id, InventoryEntity updated, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<List<InventoryEntity>> GetLatestAsync(int take, CancellationToken ct = default);
    Task<List<InventoryEntity>> GetTopAsync(int take, CancellationToken ct = default);

}
