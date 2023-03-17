using Microsoft.AspNetCore.Mvc;

namespace Tgs.GamePlayerBalance.Controllers;

public class PlayerFormsByExcelController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
