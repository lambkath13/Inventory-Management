using InventoryApp.Data;
using InventoryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Services;

public class InventoryService : IInventoryService
{
    private readonly ApplicationDbContext _db;
    public InventoryService(ApplicationDbContext db) => _db = db;

    public async Task<List<InventoryEntity>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Inventories.AsNoTracking().ToListAsync(ct);

    public async Task<InventoryEntity?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _db.Inventories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<InventoryEntity> CreateAsync(InventoryEntity entity, CancellationToken ct = default)
    {
        entity.CreatedAt = entity.UpdatedAt = DateTime.UtcNow;
        _db.Inventories.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<InventoryEntity?> UpdateAsync(int id, InventoryEntity updated, CancellationToken ct = default)
    {
        var entity = await _db.Inventories.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null) return null;

        entity.Title = updated.Title;
        entity.DescriptionMarkdown = updated.DescriptionMarkdown;
        entity.Category = updated.Category;
        entity.IsPublic = updated.IsPublic;


        entity.CustomString1Name = updated.CustomString1Name;
        entity.CustomString2Name = updated.CustomString2Name;
        entity.CustomString3Name = updated.CustomString3Name;
        entity.CustomString1State = updated.CustomString1State;
        entity.CustomString2State = updated.CustomString2State;
        entity.CustomString3State = updated.CustomString3State;

        entity.CustomInt1Name = updated.CustomInt1Name;
        entity.CustomInt2Name = updated.CustomInt2Name;
        entity.CustomInt3Name = updated.CustomInt3Name;
        entity.CustomInt1State = updated.CustomInt1State;
        entity.CustomInt2State = updated.CustomInt2State;
        entity.CustomInt3State = updated.CustomInt3State;

        entity.CustomBool1Name = updated.CustomBool1Name;
        entity.CustomBool2Name = updated.CustomBool2Name;
        entity.CustomBool3Name = updated.CustomBool3Name;
        entity.CustomBool1State = updated.CustomBool1State;
        entity.CustomBool2State = updated.CustomBool2State;
        entity.CustomBool3State = updated.CustomBool3State;

        entity.CustomText1Name = updated.CustomText1Name;
        entity.CustomText2Name = updated.CustomText2Name;
        entity.CustomText3Name = updated.CustomText3Name;
        entity.CustomText1State = updated.CustomText1State;
        entity.CustomText2State = updated.CustomText2State;
        entity.CustomText3State = updated.CustomText3State;

        entity.CustomLink1Name = updated.CustomLink1Name;
        entity.CustomLink2Name = updated.CustomLink2Name;
        entity.CustomLink3Name = updated.CustomLink3Name;
        entity.CustomLink1State = updated.CustomLink1State;
        entity.CustomLink2State = updated.CustomLink2State;
        entity.CustomLink3State = updated.CustomLink3State;
        entity.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _db.SaveChangesAsync(ct);
            return entity;
        }
        catch (DbUpdateConcurrencyException)
        {
            return null;
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Inventories.FindAsync(new object?[] { id }, ct);
        if (entity == null) return false;
        _db.Inventories.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<InventoryEntity>> GetLatestAsync(int take, CancellationToken ct = default)
    {
        return await _db.Inventories
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<List<InventoryEntity>> GetTopAsync(int take, CancellationToken ct = default)
    {
        return await _db.Inventories
            .AsNoTracking()
            .OrderByDescending(x => x.UpdatedAt)
            .Take(take)
            .ToListAsync(ct);
    }
}
