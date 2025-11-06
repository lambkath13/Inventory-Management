using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Data;
using Microsoft.AspNetCore.Authorization;

namespace InventoryApp.Controllers;

[Authorize]
[ApiController]
[Route("api/inventories/{inventoryId:int}/stats")]
public class StatsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public StatsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get(int inventoryId, CancellationToken ct)
    {
        var items = _db.Items.AsNoTracking().Where(i => i.InventoryId == inventoryId);

        var count = await items.CountAsync(ct);

        var nums = await items.Select(i => new { i.Int1, i.Int2, i.Int3 }).ToListAsync(ct);
        (double? avg, int? min, int? max) Agg(IEnumerable<int?> src)
        {
            var vals = src.Where(v => v.HasValue).Select(v => v!.Value).ToList();
            if (vals.Count == 0) return (null, null, null);
            return (vals.Average(), vals.Min(), vals.Max());
        }
        var int1 = Agg(nums.Select(x => x.Int1));
        var int2 = Agg(nums.Select(x => x.Int2));
        var int3 = Agg(nums.Select(x => x.Int3));

        async Task<List<object>> TopStr(Func<dynamic,string?> sel)
        {
            var q = await items
                .Select(i => sel(i))
                .Where(s => s != null && s != "")
                .GroupBy(s => s!)
                .Select(g => new { value = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count).ThenBy(x => x.value)
                .Take(10).ToListAsync(ct);
            return q.Select(x => (object)x).ToList();
        }

        var topString1 = await TopStr(i => i.String1);
        var topString2 = await TopStr(i => i.String2);
        var topString3 = await TopStr(i => i.String3);

        return Ok(new
        {
            itemsCount = count,
            numbers = new {
                int1 = new { avg = int1.avg, min = int1.min, max = int1.max, range = int1.max - int1.min },
                int2 = new { avg = int2.avg, min = int2.min, max = int2.max, range = int2.max - int2.min },
                int3 = new { avg = int3.avg, min = int3.min, max = int3.max, range = int3.max - int3.min },
            },
            strings = new {
                string1Top = topString1,
                string2Top = topString2,
                string3Top = topString3
            }
        });
    }
}
