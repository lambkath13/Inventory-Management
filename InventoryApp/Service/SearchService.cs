using InventoryApp.Data;
using InventoryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Services;

public class SearchService : ISearchService
{
    private readonly ApplicationDbContext _db;
    public SearchService(ApplicationDbContext db) => _db = db;

    public async Task<(List<InventoryEntity> Inventories, List<Item> Items)>
        SearchAsync(string query, CancellationToken ct = default)
    {
        query = (query ?? string.Empty).Trim();
        if (query.Length == 0)
            return (new(), new());

        const string cfg = "english";

        var inventories = await _db.Inventories
            .Where(i =>
                EF.Functions.ToTsVector(cfg,
                    (i.Title ?? "") + " " + (i.DescriptionMarkdown ?? ""))
                .Matches(EF.Functions.PlainToTsQuery(cfg, query)))
            .OrderByDescending(i => i.CreatedAt)
            .AsNoTracking()
            .Take(50)
            .ToListAsync(ct);

        var items = await _db.Items
            .Where(i =>
                EF.Functions.ToTsVector(cfg,
                    (i.CustomId ?? "") + " " +
                    (i.String1 ?? "") + " " + (i.String2 ?? "") + " " + (i.String3 ?? "") + " " +
                    (i.Text1 ?? "") + " " + (i.Text2 ?? "") + " " + (i.Text3 ?? ""))
                .Matches(EF.Functions.PlainToTsQuery(cfg, query)))
            .OrderBy(i => i.CustomId)
            .AsNoTracking()
            .Take(50)
            .ToListAsync(ct);

        return (inventories, items);
    }
}
