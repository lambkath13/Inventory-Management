using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.Controllers;

public class LanguageController : Controller
{
    [AllowAnonymous] 
    [HttpGet("/lang/set")]
    public IActionResult Set(string c = "en", string? u = "/")
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(c)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                Path = "/",             
                IsEssential = true,     
                SameSite = SameSiteMode.Lax,
                HttpOnly = false
            }
        );
        return LocalRedirect(u ?? "/");
    }
}
