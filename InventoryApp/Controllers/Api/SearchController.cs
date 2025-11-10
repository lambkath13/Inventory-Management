using InventoryApp.Data;
using InventoryApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Controllers.Api;

[ApiController]
[Route("api/search")]
public class SearchApiController : ControllerBase
{
    private readonly ISearchService _search;
    private readonly ApplicationDbContext _db;

    public SearchApiController(ISearchService search, ApplicationDbContext db)
    {
        _search = search;
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Query cannot be empty.");

        var (inventories, items) = await _search.SearchAsync(q, ct);
        return Ok(new { inventories, items });
    }

    [HttpGet("by-tag")]
    [AllowAnonymous]
    public async Task<IActionResult> ByTag(
        [FromQuery] string tag,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return Ok(new { total = 0, page, pageSize, items = Array.Empty<object>() });

        var norm = tag.Trim();

        var baseQuery =
            from t in _db.Tags.AsNoTracking()
            join it in _db.InventoryTags.AsNoTracking() on t.Id equals it.TagId
            join inv in _db.Inventories.AsNoTracking() on it.InventoryId equals inv.Id
            where EF.Functions.ILike(t.Name, norm) && inv.IsPublic
            select new { inv.Id, inv.Title, inv.DescriptionMarkdown, inv.CreatedAt };

        var total = await baseQuery.CountAsync(ct);
        var items = await baseQuery
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Ok(new { total, page, pageSize, items });
    }
}
