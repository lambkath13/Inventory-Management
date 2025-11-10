using InventoryApp.Services;
using InventoryApp.Models;
using InventoryApp.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Controllers.Api;
    
[ApiController]
[Route("api/items")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _service;
    private readonly IAuthorizationService _auth;
    private readonly UserManager<AppUser> _um;

    public ItemsController(IItemService service, IAuthorizationService auth, UserManager<AppUser> um)
    {
        _service = service;
        _auth = auth;
        _um = um;
    }

    [AllowAnonymous]
    [HttpGet("{inventoryId:int}")]
    public async Task<IActionResult> GetAll(
        int inventoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? sortBy = "createdAt",
        [FromQuery] string sortDir = "desc",
        CancellationToken ct = default)
    {
        var q = (await _service.GetAllAsync(inventoryId, ct)).AsQueryable();
        q = (sortBy ?? "createdAt").ToLower() switch
        {
            "customid" => sortDir == "asc" ? q.OrderBy(i => i.CustomId) : q.OrderByDescending(i => i.CustomId),
            _ => sortDir == "asc" ? q.OrderBy(i => i.CreatedAt) : q.OrderByDescending(i => i.CreatedAt),
        };
        var total = q.Count();
        var data = q.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Ok(new { total, page, pageSize, items = data });
    }

    [AllowAnonymous]
    [HttpGet("single/{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await _service.GetByIdAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ItemCreateDto dto, CancellationToken ct)
    {
        var authz = await _auth.AuthorizeAsync(User, dto.InventoryId, "CanWriteInventory");
        if (!authz.Succeeded) return Forbid();

        var userId = _um.GetUserId(User)!;

        var item = new Item
        {
            InventoryId = dto.InventoryId,
            CreatedByUserId = userId,
            String1 = dto.String1,
            String2 = dto.String2,
            String3 = dto.String3,
            Int1 = dto.Int1,
            Int2 = dto.Int2,
            Int3 = dto.Int3,
            Bool1 = dto.Bool1,
            Bool2 = dto.Bool2,
            Bool3 = dto.Bool3,
            Text1 = dto.Text1,
            Text2 = dto.Text2,
            Text3 = dto.Text3,
            Link1 = dto.Link1,
            Link2 = dto.Link2,
            Link3 = dto.Link3
        };

        try
        {
            var created = await _service.CreateAsync(item, ct);
            return Ok(created);
        }
        catch (DbUpdateException)
        {
            return Conflict("Duplicate CustomId in this inventory.");
        }
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromQuery] int inventoryId,
        [FromBody] ItemUpdateDto dto,
        CancellationToken ct)
    {
        var authz = await _auth.AuthorizeAsync(User, inventoryId, "CanWriteInventory");
        if (!authz.Succeeded) return Forbid();

        var updated = new Item
        {
            CustomId = dto.CustomId ?? "",
            String1 = dto.String1,
            String2 = dto.String2,
            String3 = dto.String3,
            Int1 = dto.Int1,
            Int2 = dto.Int2,
            Int3 = dto.Int3,
            Bool1 = dto.Bool1,
            Bool2 = dto.Bool2,
            Bool3 = dto.Bool3,
            Text1 = dto.Text1,
            Text2 = dto.Text2,
            Text3 = dto.Text3,
            Link1 = dto.Link1,
            Link2 = dto.Link2,
            Link3 = dto.Link3
        };

        try
        {
            var result = await _service.UpdateAsync(id, updated, ct);
            return result is null ? Conflict("Item modified or not found") : Ok(result);
        }
        catch (ValidationException ve)
        {
            return BadRequest(new { error = ve.Message });
        }
        catch (InvalidOperationException ioe) when (ioe.Message.StartsWith("Duplicate CustomId"))
        {
            return Conflict(new { error = ioe.Message });
        }
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, [FromQuery] int inventoryId, CancellationToken ct)
    {
        var authz = await _auth.AuthorizeAsync(User, inventoryId, "CanWriteInventory");
        if (!authz.Succeeded) return Forbid();

        var ok = await _service.DeleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}
