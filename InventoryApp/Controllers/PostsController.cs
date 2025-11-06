using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Data;
using InventoryApp.Hubs;
using InventoryApp.Models;

namespace InventoryApp.Controllers;

[Authorize]
[ApiController]
[Route("api/inventories/{inventoryId:int}/posts")]
public class PostsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IHubContext<DiscussionHub> _hub;

    public PostsController(ApplicationDbContext db, IHubContext<DiscussionHub> hub)
    { _db = db; _hub = hub; }

    [HttpGet]
    public async Task<IActionResult> List(
        int inventoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var q = _db.Posts.AsNoTracking()
            .Where(p => p.InventoryId == inventoryId)
            .OrderBy(p => p.CreatedAt);

        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new { p.Id, p.UserId, p.BodyMarkdown, p.CreatedAt })
            .ToListAsync(ct);

        return Ok(new { total, page, pageSize, items });
    }


    [HttpPost]
    public async Task<IActionResult> Add(int inventoryId, [FromBody] string bodyMarkdown, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(bodyMarkdown))
            return BadRequest("Post body is empty.");

        var post = new Post
        {
            InventoryId = inventoryId,
            UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value,
            BodyMarkdown = bodyMarkdown.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        _db.Posts.Add(post);
        await _db.SaveChangesAsync(ct);

        await _hub.Clients.Group($"inv-{inventoryId}")
            .SendAsync("postAdded", new { post.Id, post.UserId, post.BodyMarkdown, post.CreatedAt }, ct);

        return Ok(post);
    }
}
