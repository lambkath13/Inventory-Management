using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    [AllowAnonymous]
    [HttpGet("/")]
    public IActionResult Index() => View();
}
