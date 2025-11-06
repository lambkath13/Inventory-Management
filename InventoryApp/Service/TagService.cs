using InventoryApp.Data;
using InventoryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Services;

public class TagService : ITagService
{
    private readonly ApplicationDbContext _db;
    public TagService(ApplicationDbContext db) => _db = db;

    public Task<List<Tag>> AutocompleteAsync(string prefix, int take = 20, CancellationToken ct = default) =>
        _db.Tags.AsNoTracking()
            .Where(t => t.Name.StartsWith(prefix))
            .OrderBy(t => t.Name)
            .Take(take)
            .ToListAsync(ct);

    public async Task<List<(string Tag, int Count)>> CloudAsync(int take = 30, CancellationToken ct = default)
    {
        var data = await _db.InventoryTags
            .AsNoTracking()
            .Join(_db.Inventories.AsNoTracking(),
                  it => it.InventoryId,
                  inv => inv.Id,
                  (it, inv) => it.TagId)
            .GroupBy(tagId => tagId)
            .Select(g => new { TagId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(take)
            .ToListAsync(ct);

        var tagIds = data.Select(d => d.TagId).ToList();
        var tags = await _db.Tags
            .Where(t => tagIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Name, ct);

        return data.Select(d => (tags[d.TagId], d.Count)).ToList();
    }


    public Task<List<Tag>> GetForInventoryAsync(int inventoryId, CancellationToken ct = default) =>
        (from it in _db.InventoryTags.AsNoTracking()
         join t in _db.Tags.AsNoTracking() on it.TagId equals t.Id
         where it.InventoryId == inventoryId
         orderby t.Name
         select t).ToListAsync(ct);

    public async Task AddToInventoryAsync(int inventoryId, string tagName, CancellationToken ct = default)
    {
        var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Name == tagName, ct);
        if (tag is null) { tag = new Tag { Name = tagName }; _db.Tags.Add(tag); await _db.SaveChangesAsync(ct); }

        var exists = await _db.InventoryTags.AnyAsync(it => it.InventoryId == inventoryId && it.TagId == tag.Id, ct);
        if (!exists) { _db.InventoryTags.Add(new InventoryTag { InventoryId = inventoryId, TagId = tag.Id }); await _db.SaveChangesAsync(ct); }
    }

    public async Task RemoveFromInventoryAsync(int inventoryId, string tagName, CancellationToken ct = default)
    {
        var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Name == tagName, ct);
        if (tag is null) return;
        var link = await _db.InventoryTags.FirstOrDefaultAsync(it => it.InventoryId == inventoryId && it.TagId == tag.Id, ct);
        if (link is null) return;
        _db.InventoryTags.Remove(link);
        await _db.SaveChangesAsync(ct);
    }
}
