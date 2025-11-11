using InventoryApp.Data;
using InventoryApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Controllers;

[ApiController]
[Route("api/inventories/{inventoryId:int}/access")]
[Authorize] 
public class AccessController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IAccessService _svc;
    private readonly IAuthorizationService _auth;

    public AccessController(ApplicationDbContext db, IAccessService svc, IAuthorizationService auth)
    {
        _db = db;
        _svc = svc;
        _auth = auth;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        int inventoryId,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortDir = "asc",
        CancellationToken ct = default)
    {
        var can = await _auth.AuthorizeAsync(User, inventoryId, "CanWriteInventory");
        if (!can.Succeeded) return Forbid();

        var q = from a in _db.InventoryAccesses.AsNoTracking()
                where a.InventoryId == inventoryId
                join u in _db.Users.AsNoTracking() on a.UserId equals u.Id
                select new { a.UserId, a.CanWrite, u.DisplayName, u.Email };

        q = (sortBy.ToLower(), sortDir.ToLower()) switch
        {
            ("email", "desc") => q.OrderByDescending(x => x.Email),
            ("email", _) => q.OrderBy(x => x.Email),
            ("name", "desc") => q.OrderByDescending(x => x.DisplayName),
            _ => q.OrderBy(x => x.DisplayName),
        };

        var list = await q.ToListAsync(ct);
        return Ok(list);
    }


    [HttpPost]
    public async Task<IActionResult> Grant(int inventoryId, [FromQuery] string userId, [FromQuery] bool canWrite, CancellationToken ct)
    {
        var res = await _auth.AuthorizeAsync(User, inventoryId, "CanWriteInventory");
        if (!res.Succeeded) return Forbid();

        var a = await _svc.GrantAsync(inventoryId, userId, canWrite, ct);
        return Ok(a);
    }

    [HttpDelete]
    public async Task<IActionResult> Revoke(int inventoryId, [FromQuery] string userId, CancellationToken ct)
    {
        var res = await _auth.AuthorizeAsync(User, inventoryId, "CanWriteInventory");
        if (!res.Succeeded) return Forbid();

        var ok = await _svc.RevokeAsync(inventoryId, userId, ct);
        return ok ? NoContent() : NotFound();
    }
}
