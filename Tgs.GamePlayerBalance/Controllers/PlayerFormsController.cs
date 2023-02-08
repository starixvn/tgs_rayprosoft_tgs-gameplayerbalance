using Microsoft.AspNetCore.Mvc;

namespace Tgs.GamePlayerBalance.Controllers;

public class PlayerFormsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
