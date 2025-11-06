using System.Security.Claims;
using InventoryApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Models;

namespace InventoryApp.Authorization;

public sealed class InventoryWriteHandler
    : AuthorizationHandler<InventoryWriteRequirement, int>
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<AppUser> _um;

    public InventoryWriteHandler(ApplicationDbContext db, UserManager<AppUser> um)
    {
        _db = db;
        _um = um;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        InventoryWriteRequirement requirement,
        int inventoryId)
    {
        if (!(context.User?.Identity?.IsAuthenticated ?? false))
            return;

        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        var principal = (ClaimsPrincipal)context.User;
        var userId = _um.GetUserId(principal);
        if (string.IsNullOrEmpty(userId))
            return;

        var inv = await _db.Inventories
            .AsNoTracking()
            .Where(i => i.Id == inventoryId)
            .Select(i => new { i.OwnerUserId })
            .FirstOrDefaultAsync();

        if (inv is null)
            return; 

        if (inv.OwnerUserId == userId)
        {
            context.Succeed(requirement);
            return;
        }

        var hasWriteAccess = await _db.InventoryAccesses
            .AsNoTracking()
            .AnyAsync(a => a.InventoryId == inventoryId
                        && a.UserId == userId
                        && a.CanWrite);

        if (hasWriteAccess)
        {
            context.Succeed(requirement);
        }
    }
}
