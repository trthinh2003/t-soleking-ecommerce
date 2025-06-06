using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoleKingECommerce.Models;
using SoleKingECommerce.Services.Interfaces;
using SoleKingECommerce.ViewModels.Auth;

namespace SoleKingECommerce.Controllers.Admin
{
    [Route("admin/[controller]/[action]")]
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSenderService _emailSender;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, IEmailSenderService emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        public IActionResult Register()
        {
            return View("~/Views/Admin/Auth/Register.cshtml");
        }

        public IActionResult Login()
        {
            return View("~/Views/Admin/Auth/Login.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                ModelState.AddModelError("", "Invalid login attempt");
                return View("~/Views/Admin/Auth/Login.cshtml", model);
            }
            return View("~/Views/Admin/Auth/Login.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Auth");
        }
    }
}
