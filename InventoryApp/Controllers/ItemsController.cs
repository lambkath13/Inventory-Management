using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.Controllers;

[Authorize]
public class ItemsController : Controller
{
    [HttpGet("/Items")]
    public IActionResult Index(int inventoryId) => View(model: inventoryId);

    [HttpGet("/Items/Create")]
    public IActionResult Create(int inventoryId) => View(model: inventoryId);

    [HttpGet("/Items/Edit/{id:int}")]
    public IActionResult Edit(int id) => View(model: id);

    [HttpGet("/Items/Details/{id:int}")]
    public IActionResult Details(int id) => View(id);

}
