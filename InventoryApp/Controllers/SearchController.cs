using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.Controllers;

[Authorize]
public class SearchController : Controller
{
    [HttpGet("/search")]
    public IActionResult Index() => View();
}
