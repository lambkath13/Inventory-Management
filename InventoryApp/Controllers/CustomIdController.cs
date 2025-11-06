using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Data;
using InventoryApp.Models;
using InventoryApp.Services;

namespace InventoryApp.Controllers;

[Authorize]
[ApiController]
[Route("api/inventories/{inventoryId:int}/custom-id")]
public class CustomIdController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IAuthorizationService _auth;
    private readonly ICustomIdService _id;

    public CustomIdController(ApplicationDbContext db, IAuthorizationService auth, ICustomIdService id)
    { _db = db; _auth = auth; _id = id; }

    [HttpGet]
    public async Task<IActionResult> Get(int inventoryId, CancellationToken ct)
    {
        var fmt = await _db.CustomIdFormats.AsNoTracking().FirstOrDefaultAsync(f => f.InventoryId == inventoryId, ct);
        return Ok(fmt ?? new CustomIdFormat { InventoryId = inventoryId, JsonDefinition = "[]", CompiledRegex = null });
    }

    [HttpPost("preview")]
    public async Task<IActionResult> Preview(int inventoryId, [FromBody] CustomIdFormat body, CancellationToken ct)
    {
        try { JsonDocument.Parse(body.JsonDefinition); }
        catch { return BadRequest("Invalid JsonDefinition."); }

        var sample = await _id.GenerateAsync(inventoryId, ct); 
        return Ok(new { sample });
    }

    [HttpPut]
    public async Task<IActionResult> Save(int inventoryId, [FromBody] CustomIdFormat body, CancellationToken ct)
    {
        var authz = await _auth.AuthorizeAsync(User, inventoryId, "CanWriteInventory");
        if (!authz.Succeeded) return Forbid();

        try { JsonDocument.Parse(body.JsonDefinition); }
        catch { return BadRequest("Invalid JsonDefinition."); }

        var fmt = await _db.CustomIdFormats.FirstOrDefaultAsync(f => f.InventoryId == inventoryId, ct);
        if (fmt is null)
        {
            body.InventoryId = inventoryId;
            body.UpdatedAt = DateTime.UtcNow;
            _db.CustomIdFormats.Add(body);
        }
        else
        {
            fmt.JsonDefinition = body.JsonDefinition;
            fmt.CompiledRegex = body.CompiledRegex;
            fmt.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateId(int inventoryId, [FromBody] string value, CancellationToken ct)
    {
        var fmt = await _db.CustomIdFormats.AsNoTracking().FirstOrDefaultAsync(f => f.InventoryId == inventoryId, ct);
        if (fmt is null) return Ok(new { valid = true }); 
        
        var valid = _id.Validate(fmt, value ?? "");
        return Ok(new { valid });
    }
}
