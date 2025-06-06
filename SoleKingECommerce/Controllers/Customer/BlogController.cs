using Microsoft.AspNetCore.Mvc;

namespace SoleKingECommerce.Controllers
{
    public class BlogController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Customer/Blog/Index.cshtml");
        }

        public IActionResult Detail(int id)
        {
            ViewBag.BlogId = id;
            return View("~/Views/Customer/Blog/Detail.cshtml");
        }
    }
}
