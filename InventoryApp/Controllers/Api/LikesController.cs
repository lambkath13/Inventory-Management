using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Data;
using InventoryApp.Models;

namespace InventoryApp.Controllers;

[ApiController]
[Route("api/items/{itemId:int}/likes")]
[Authorize] 
public class LikesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public LikesController(ApplicationDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Like(int itemId, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        var already = await _db.Likes.AnyAsync(l => l.ItemId == itemId && l.UserId == userId, ct);
        if (already) return Ok(new { liked = true });

        _db.Likes.Add(new Like { ItemId = itemId, UserId = userId });
        try
        {
            await _db.SaveChangesAsync(ct);
            return Ok(new { liked = true });
        }
        catch (DbUpdateException)
        {
            return Ok(new { liked = true });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Unlike(int itemId, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        var like = await _db.Likes.FirstOrDefaultAsync(l => l.ItemId == itemId && l.UserId == userId, ct);
        if (like is null) return NoContent();

        _db.Likes.Remove(like);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("count")]
    public async Task<IActionResult> Count(int itemId, CancellationToken ct)
    {
        var n = await _db.Likes.CountAsync(l => l.ItemId == itemId, ct);
        return Ok(new { count = n });
    }
}
