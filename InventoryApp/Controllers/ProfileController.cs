using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Data;

namespace InventoryApp.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public ProfileController(ApplicationDbContext db) => _db = db;

    [HttpGet("my-inventories")]
    public async Task<IActionResult> MyInventories([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "createdAt", [FromQuery] string sortDir = "desc", CancellationToken ct = default)
    {
        var uid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var q = _db.Inventories.AsNoTracking().Where(i => i.OwnerUserId == uid);

        q = sortBy?.ToLower() switch
        {
            "title" => sortDir == "asc" ? q.OrderBy(i => i.Title) : q.OrderByDescending(i => i.Title),
            _       => sortDir == "asc" ? q.OrderBy(i => i.CreatedAt) : q.OrderByDescending(i => i.CreatedAt),
        };

        var total = await q.CountAsync(ct);
        var data  = await q.Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct);
        return Ok(new { total, page, pageSize, items = data });
    }

    [HttpGet("write-access")]
    public async Task<IActionResult> WriteAccess([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "title", [FromQuery] string sortDir = "asc", CancellationToken ct = default)
    {
        var uid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        var q = from a in _db.InventoryAccesses.AsNoTracking()
                join i in _db.Inventories.AsNoTracking() on a.InventoryId equals i.Id
                where a.UserId == uid && a.CanWrite
                select new { i.Id, i.Title, i.DescriptionMarkdown, i.CreatedAt, i.OwnerUserId };

        q = sortBy?.ToLower() switch
        {
            "createdat" => sortDir == "desc" ? q.OrderByDescending(x => x.CreatedAt) : q.OrderBy(x => x.CreatedAt),
            _           => sortDir == "desc" ? q.OrderByDescending(x => x.Title)    : q.OrderBy(x => x.Title),
        };

        var total = await q.CountAsync(ct);
        var data  = await q.Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct);
        return Ok(new { total, page, pageSize, items = data });
    }
}
