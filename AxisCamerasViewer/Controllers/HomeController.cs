using Microsoft.AspNetCore.Mvc;

namespace AxisCamerasViewer.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}