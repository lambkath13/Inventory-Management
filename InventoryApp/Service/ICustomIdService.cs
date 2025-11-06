using InventoryApp.Models;

namespace InventoryApp.Services;

public interface ICustomIdService
{
    Task<string> GenerateAsync(int inventoryId, CancellationToken ct = default);
    bool Validate(CustomIdFormat format, string value);
}
