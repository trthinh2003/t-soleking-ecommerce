using Microsoft.AspNetCore.Mvc;

namespace SoleKingECommerce.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Customer/Product/Index.cshtml");
        }

        public IActionResult Detail(int id)
        {
            ViewBag.ProductId = id;
            return View("~/Views/Customer/Product/Detail.cshtml");
        }
    }
}
