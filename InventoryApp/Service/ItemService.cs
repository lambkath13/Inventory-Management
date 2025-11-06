using System.ComponentModel.DataAnnotations;
using InventoryApp.Data;
using InventoryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Services;

public class ItemService : IItemService
{
    private readonly ApplicationDbContext _db;
    private readonly ICustomIdService _idService;

    public ItemService(ApplicationDbContext db, ICustomIdService idService)
    {
        _db = db;
        _idService = idService;
    }

    public async Task<List<Item>> GetAllAsync(int inventoryId, CancellationToken ct = default) =>
        await _db.Items
            .AsNoTracking()
            .Where(x => x.InventoryId == inventoryId)
            .ToListAsync(ct);

    public async Task<Item?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _db.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Item> CreateAsync(Item item, CancellationToken ct = default)
    {
        item.CustomId = await _idService.GenerateAsync(item.InventoryId, ct);
        item.CreatedAt = item.UpdatedAt = DateTime.UtcNow;

        _db.Items.Add(item);
        await _db.SaveChangesAsync(ct);
        return item;
    }

    public async Task<Item?> UpdateAsync(int id, Item updated, CancellationToken ct = default)
    {
        var entity = await _db.Items.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null) return null;

        if (!string.IsNullOrWhiteSpace(updated.CustomId) &&
            updated.CustomId != entity.CustomId)
        {
            var fmt = await _db.CustomIdFormats
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.InventoryId == entity.InventoryId, ct);

            if (fmt != null && !_idService.Validate(fmt, updated.CustomId))
                throw new ValidationException("CustomId does not match inventory format.");

            entity.CustomId = updated.CustomId;
        }

        entity.UpdatedAt = DateTime.UtcNow;

        entity.String1 = updated.String1; entity.String2 = updated.String2; entity.String3 = updated.String3;
        entity.Int1    = updated.Int1;    entity.Int2    = updated.Int2;    entity.Int3    = updated.Int3;
        entity.Bool1   = updated.Bool1;   entity.Bool2   = updated.Bool2;   entity.Bool3   = updated.Bool3;
        entity.Text1   = updated.Text1;   entity.Text2   = updated.Text2;   entity.Text3   = updated.Text3;
        entity.Link1   = updated.Link1;   entity.Link2   = updated.Link2;   entity.Link3   = updated.Link3;

        try
        {
            await _db.SaveChangesAsync(ct);
            return entity;
        }
        catch (DbUpdateConcurrencyException)
        {
            return null;
        }
        catch (DbUpdateException ex) when (
            ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true)
        {
            throw new InvalidOperationException("Duplicate CustomId in this inventory.");
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Items.FindAsync(new object?[] { id }, ct);
        if (entity == null) return false;

        _db.Items.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
