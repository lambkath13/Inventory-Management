using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryApp.Services;

namespace InventoryApp.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tags;
    private readonly IAuthorizationService _auth;
    public TagsController(ITagService tags, IAuthorizationService auth) { _tags = tags; _auth = auth; }

    [HttpGet("tags/autocomplete")]
    public async Task<IActionResult> Autocomplete([FromQuery] string prefix, CancellationToken ct)
        => Ok(await _tags.AutocompleteAsync(prefix ?? string.Empty, 20, ct));


    [HttpGet("tags/cloud")]
    public async Task<IActionResult> Cloud(CancellationToken ct, [FromQuery] int take = 30)
    {
        var data = await _tags.CloudAsync(take, ct); 
        var dto = data.Select(x => new { tag = x.Tag, count = x.Count }).ToList();
        return Ok(dto);
    }


    [HttpGet("inventories/{inventoryId:int}/tags")]
    public async Task<IActionResult> GetForInventory(int inventoryId, CancellationToken ct)
        => Ok(await _tags.GetForInventoryAsync(inventoryId, ct));

    [HttpPost("inventories/{inventoryId:int}/tags")]
    public async Task<IActionResult> AddTag(int inventoryId, [FromQuery] string tag, CancellationToken ct)
    {
        var res = await _auth.AuthorizeAsync(User, inventoryId, "CanWriteInventory");
        if (!res.Succeeded) return Forbid();
        await _tags.AddToInventoryAsync(inventoryId, tag.Trim(), ct);
        return Ok();
    }

    [HttpDelete("inventories/{inventoryId:int}/tags")]
    public async Task<IActionResult> RemoveTag(int inventoryId, [FromQuery] string tag, CancellationToken ct)
    {
        var res = await _auth.AuthorizeAsync(User, inventoryId, "CanWriteInventory");
        if (!res.Succeeded) return Forbid();
        await _tags.RemoveFromInventoryAsync(inventoryId, tag.Trim(), ct);
        return NoContent();
    }
}
