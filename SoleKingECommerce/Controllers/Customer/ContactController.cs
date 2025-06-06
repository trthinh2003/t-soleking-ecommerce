using Microsoft.AspNetCore.Mvc;
using SoleKingECommerce.Models;

public class ContactController : Controller
{
    public IActionResult Index()
    {
        return View("~/Views/Customer/Contact/Index.cshtml");
    }

    [HttpPost]
    public IActionResult Index(ContactModel model)
    {
        if (ModelState.IsValid)
        {
            return RedirectToAction("Index", "Home");
        }

        return View("~/Views/Customer/Contact/Index.cshtml", model);
    }
}
