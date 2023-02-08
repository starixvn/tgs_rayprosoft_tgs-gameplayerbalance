using Microsoft.AspNetCore.Mvc;

namespace Tgs.GamePlayerBalance.Controllers;

public class ExcelFormsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
