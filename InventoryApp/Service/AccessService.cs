using InventoryApp.Data;
using InventoryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Services;

public class AccessService : IAccessService
{
    private readonly ApplicationDbContext _db;
    public AccessService(ApplicationDbContext db) => _db = db;

    public Task<List<InventoryAccess>> ListAsync(int inventoryId, CancellationToken ct = default) =>
        _db.InventoryAccesses.AsNoTracking().Where(a => a.InventoryId == inventoryId).ToListAsync(ct);

    public async Task<InventoryAccess> GrantAsync(int inventoryId, string userId, bool canWrite, CancellationToken ct = default)
    {
        var access = await _db.InventoryAccesses.FirstOrDefaultAsync(
            a => a.InventoryId == inventoryId && a.UserId == userId, ct);

        if (access is null)
        {
            access = new InventoryAccess { InventoryId = inventoryId, UserId = userId, CanWrite = canWrite };
            _db.InventoryAccesses.Add(access);
        }
        else access.CanWrite = canWrite;

        await _db.SaveChangesAsync(ct);
        return access;
    }

    public async Task<bool> RevokeAsync(int inventoryId, string userId, CancellationToken ct = default)
    {
        var access = await _db.InventoryAccesses.FirstOrDefaultAsync(
            a => a.InventoryId == inventoryId && a.UserId == userId, ct);
        if (access is null) return false;
        _db.InventoryAccesses.Remove(access);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
