using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    [HttpGet("/admin/users")]
    public IActionResult Users() => View();
}
