using Microsoft.AspNetCore.Mvc;
using InventoryApp.Services;
using Microsoft.AspNetCore.Authorization;

namespace InventoryApp.Controllers.Api
{
    [AllowAnonymous]
    [Route("api/home")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IInventoryService _inventories;

        public HomeController(IInventoryService inventories)
        {
            _inventories = inventories;
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest(int take = 4, CancellationToken ct = default)
        {
            var latest = await _inventories.GetLatestAsync(take, ct);
            return Ok(latest);
        }

        [HttpGet("top")]
        public async Task<IActionResult> GetTop(int take = 5, CancellationToken ct = default)
        {
            var top = await _inventories.GetTopAsync(take, ct);
            return Ok(top);
        }
    }
}
