using InventoryApp.Models;
using InventoryApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.Controllers;

[AllowAnonymous]
public class InventoriesController : Controller
{
    private readonly IInventoryService _service;
    private readonly IAuthorizationService _auth;
    private readonly UserManager<AppUser> _um;

    public InventoriesController(IInventoryService service, IAuthorizationService auth, UserManager<AppUser> um)
    {
        _service = service;
        _auth = auth;
        _um = um;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var items = await _service.GetAllAsync(ct);
        return View(items.OrderByDescending(x => x.CreatedAt).ToList());
    }

    public IActionResult Details(int id) => View(model: id);

    public IActionResult Create() => View();

    public IActionResult Edit(int id) => View(model: id);

}
