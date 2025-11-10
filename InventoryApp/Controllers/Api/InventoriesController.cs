using InventoryApp.Services;
using InventoryApp.Models;
using InventoryApp.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace InventoryApp.Controllers;

[ApiController]
[Route("api/inventories")]
public class InventoriesApiController : ControllerBase
{
    private readonly IInventoryService _service;
    private readonly IAuthorizationService _auth;
    private readonly UserManager<AppUser> _um;
    private readonly ITagService _tags;
    

    public InventoriesApiController(IInventoryService service, IAuthorizationService auth, UserManager<AppUser> um, ITagService tags)
    {
        _service = service;
        _auth = auth;
        _um = um;
        _tags = tags;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "createdAt",
        [FromQuery] string sortDir = "desc",
        CancellationToken ct = default)
    {
        var list = await _service.GetAllAsync(ct);
        var q = list.AsQueryable();

        q = (sortBy ?? "createdAt").ToLower() switch
        {
            "title" => sortDir == "asc" ? q.OrderBy(i => i.Title) : q.OrderByDescending(i => i.Title),
            _ => sortDir == "asc" ? q.OrderBy(i => i.CreatedAt) : q.OrderByDescending(i => i.CreatedAt),
        };

        var total = q.Count();
        var items = q.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var inv = await _service.GetByIdAsync(id, ct);
        return inv == null ? NotFound() : Ok(inv);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] InventoryCreateDto dto, CancellationToken ct)
    {
        var userId = _um.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var entity = new InventoryEntity
        {
            Title = dto.Title,
            DescriptionMarkdown = dto.DescriptionMarkdown,
            Category = dto.Category,
            IsPublic = dto.IsPublic,
            OwnerUserId = userId,

            CustomString1Name = dto.CustomString1Name,
            CustomString2Name = dto.CustomString2Name,
            CustomString3Name = dto.CustomString3Name,
            CustomString1State = dto.CustomString1State,
            CustomString2State = dto.CustomString2State,
            CustomString3State = dto.CustomString3State,

            CustomInt1Name = dto.CustomInt1Name,
            CustomInt2Name = dto.CustomInt2Name,
            CustomInt3Name = dto.CustomInt3Name,
            CustomInt1State = dto.CustomInt1State,
            CustomInt2State = dto.CustomInt2State,
            CustomInt3State = dto.CustomInt3State,

            CustomBool1Name = dto.CustomBool1Name,
            CustomBool2Name = dto.CustomBool2Name,
            CustomBool3Name = dto.CustomBool3Name,
            CustomBool1State = dto.CustomBool1State,
            CustomBool2State = dto.CustomBool2State,
            CustomBool3State = dto.CustomBool3State,

            CustomText1Name = dto.CustomText1Name,
            CustomText2Name = dto.CustomText2Name,
            CustomText3Name = dto.CustomText3Name,
            CustomText1State = dto.CustomText1State,
            CustomText2State = dto.CustomText2State,
            CustomText3State = dto.CustomText3State,

            CustomLink1Name = dto.CustomLink1Name,
            CustomLink2Name = dto.CustomLink2Name,
            CustomLink3Name = dto.CustomLink3Name,
            CustomLink1State = dto.CustomLink1State,
            CustomLink2State = dto.CustomLink2State,
            CustomLink3State = dto.CustomLink3State
        };

        var result = await _service.CreateAsync(entity, ct);
        
        if (dto.Tags is { Length: > 0 })
        {
            foreach (var raw in dto.Tags
                     .Select(t => t?.Trim())
                     .Where(t => !string.IsNullOrWhiteSpace(t))
                     .Select(t => t!)            
                     .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                await _tags.AddToInventoryAsync(result.Id, raw, ct);
            }
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] InventoryCreateDto dto, CancellationToken ct)
    {
        var can = await _auth.AuthorizeAsync(User, id, "CanWriteInventory");
        if (!can.Succeeded) return Forbid();

        var current = await _service.GetByIdAsync(id, ct);
        if (current is null) return NotFound();

        var ifMatch = Request.Headers["If-Match"].FirstOrDefault();
        if (!string.IsNullOrEmpty(ifMatch) && ifMatch != $"W/\"{current.xmin}\"")
            return StatusCode(StatusCodes.Status412PreconditionFailed, "Version mismatch");

        current.Title = dto.Title;
        current.DescriptionMarkdown = dto.DescriptionMarkdown;
        current.Category = dto.Category;
        current.IsPublic = dto.IsPublic;

        current.CustomString1Name = dto.CustomString1Name;
        current.CustomString2Name = dto.CustomString2Name;
        current.CustomString3Name = dto.CustomString3Name;
        current.CustomString1State = dto.CustomString1State;
        current.CustomString2State = dto.CustomString2State;
        current.CustomString3State = dto.CustomString3State;

        current.CustomInt1Name = dto.CustomInt1Name;
        current.CustomInt2Name = dto.CustomInt2Name;
        current.CustomInt3Name = dto.CustomInt3Name;
        current.CustomInt1State = dto.CustomInt1State;
        current.CustomInt2State = dto.CustomInt2State;
        current.CustomInt3State = dto.CustomInt3State;

        current.CustomBool1Name = dto.CustomBool1Name;
        current.CustomBool2Name = dto.CustomBool2Name;
        current.CustomBool3Name = dto.CustomBool3Name;
        current.CustomBool1State = dto.CustomBool1State;
        current.CustomBool2State = dto.CustomBool2State;
        current.CustomBool3State = dto.CustomBool3State;

        current.CustomText1Name = dto.CustomText1Name;
        current.CustomText2Name = dto.CustomText2Name;
        current.CustomText3Name = dto.CustomText3Name;
        current.CustomText1State = dto.CustomText1State;
        current.CustomText2State = dto.CustomText2State;
        current.CustomText3State = dto.CustomText3State;

        current.CustomLink1Name = dto.CustomLink1Name;
        current.CustomLink2Name = dto.CustomLink2Name;
        current.CustomLink3Name = dto.CustomLink3Name;
        current.CustomLink1State = dto.CustomLink1State;
        current.CustomLink2State = dto.CustomLink2State;
        current.CustomLink3State = dto.CustomLink3State;

        var result = await _service.UpdateAsync(id, current, ct);
        if (result is null) return Conflict("Inventory modified or not found");

        Response.Headers.ETag = $"W/\"{result.xmin}\"";
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var can = await _auth.AuthorizeAsync(User, id, "CanWriteInventory");
        if (!can.Succeeded) return Forbid();

        var ok = await _service.DeleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}
