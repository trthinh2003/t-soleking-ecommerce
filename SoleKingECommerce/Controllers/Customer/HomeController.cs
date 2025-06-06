using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SoleKingECommerce.Models;

namespace SoleKingECommerce.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View("~/Views/Customer/Home/Index.cshtml");
    }
    public IActionResult About()
    {
        return View("~/Views/Customer/Home/About.cshtml");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
