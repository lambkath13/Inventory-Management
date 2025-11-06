using InventoryApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Controllers.Api;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminUsersController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet("search")]
    public IActionResult Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(Array.Empty<object>());

        var users = _userManager.Users
            .Where(u => (u.Email ?? "").Contains(q) || (u.DisplayName ?? "").Contains(q))
            .OrderBy(u => u.Email)
            .Take(20)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.DisplayName,
                u.IsBlocked,
                u.CreatedAt
            });

        return Ok(users);
    }

   [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? q = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? sortBy = "createdAt",
        [FromQuery] string sortDir = "desc")
    {
        var users = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            users = users.Where(u =>
                (u.Email ?? "").Contains(q) ||
                (u.DisplayName ?? "").Contains(q));

        users = (sortBy ?? "createdAt").ToLower() switch
        {
            "email"       => sortDir == "asc" ? users.OrderBy(u => u.Email)       : users.OrderByDescending(u => u.Email),
            "displayname" => sortDir == "asc" ? users.OrderBy(u => u.DisplayName) : users.OrderByDescending(u => u.DisplayName),
            _             => sortDir == "asc" ? users.OrderBy(u => u.CreatedAt)   : users.OrderByDescending(u => u.CreatedAt),
        };

        var total = await users.CountAsync();
        var items = await users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.DisplayName,
                u.IsBlocked,
                u.CreatedAt
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    [HttpPost("{id}/block")]
    public async Task<IActionResult> Block(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        user.IsBlocked = true;
        var res = await _userManager.UpdateAsync(user);
        if (!res.Succeeded) return BadRequest(res.Errors);

        return Ok(new { user.Id, user.IsBlocked });
    }

    [HttpPost("{id}/unblock")]
    public async Task<IActionResult> Unblock(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        user.IsBlocked = false;
        var res = await _userManager.UpdateAsync(user);
        if (!res.Succeeded) return BadRequest(res.Errors);

        return Ok(new { user.Id, user.IsBlocked });
    }

    [HttpPost("{id}/make-admin")]
    public async Task<IActionResult> MakeAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            var createRole = await _roleManager.CreateAsync(new IdentityRole("Admin"));
            if (!createRole.Succeeded) return BadRequest(createRole.Errors);
        }

        var res = await _userManager.AddToRoleAsync(user, "Admin");
        if (!res.Succeeded) return BadRequest(res.Errors);

        return Ok(new { user.Id, Role = "Admin", Added = true });
    }

    [HttpPost("{id}/remove-admin")]
    public async Task<IActionResult> RemoveAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var res = await _userManager.RemoveFromRoleAsync(user, "Admin");
        if (!res.Succeeded) return BadRequest(res.Errors);

        return Ok(new { user.Id, Role = "Admin", Removed = true });
    }
}
