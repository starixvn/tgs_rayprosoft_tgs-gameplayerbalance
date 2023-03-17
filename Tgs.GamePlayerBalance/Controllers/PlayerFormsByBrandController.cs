using Microsoft.AspNetCore.Mvc;

namespace Tgs.GamePlayerBalance.Controllers;

public class PlayerFormsByBrandController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
