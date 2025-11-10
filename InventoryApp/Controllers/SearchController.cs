using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.Controllers;

[AllowAnonymous]
public class SearchController : Controller
{
    [HttpGet("/search")]
    public IActionResult Index() => View();
}
