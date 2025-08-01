using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoleKingECommerce.Models;
using SoleKingECommerce.Services.Interfaces;
using SoleKingECommerce.ViewModels.Auth;

namespace SoleKingECommerce.Controllers.Customer
{
    [Route("customer/[controller]/[action]")]
    public class AuthCustomerController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSenderService _emailSender;
        private readonly ICartService _cartService;
        private readonly ILogger<AuthCustomerController> _logger;

        public AuthCustomerController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEmailSenderService emailSender,
            ICartService cartService,
            ILogger<AuthCustomerController> logger
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _cartService = cartService;
            _logger = logger;
        }

        public IActionResult Register()
        {
            return View("~/Views/Customer/Auth/Register.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Customer/Auth/Register.cshtml", model);
            }

            if (!model.AcceptTerms)
            {
                ModelState.AddModelError("AcceptTerms", "Bạn phải đồng ý với điều khoản dịch vụ");
                return View("~/Views/Customer/Auth/Register.cshtml", model);
            }

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Name = $"{model.FirstName} {model.LastName}",
                Birthday = model.BirthDate,
                UserType = UserType.Customer
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Client");
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = $"{Request.Scheme}://{Request.Host}/customer/AuthCustomer/ConfirmEmail?userId={user.Id}&token={token}";
                await _emailSender.SendEmailAsync(user.Email, "Xác nhận email",
                    $"Vui lòng xác nhận email bằng cách nhấp vào <a href='{confirmationLink}'>đây</a>.");

                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("~/Views/Customer/Auth/Register.cshtml", model);
        }

        public IActionResult Login(string returnUrl = null)
        {
            _logger.LogInformation($"Login GET - ReturnUrl: {returnUrl}");
            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Customer/Auth/Login.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return View("~/Views/Customer/Auth/ConfirmEmail.cshtml");
            }

            return View("Error");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            _logger.LogInformation($"Login POST - ReturnUrl: {returnUrl}");
            _logger.LogInformation($"Login POST - Request.Query['returnUrl']: {Request.Query["returnUrl"]}");

            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = Request.Query["returnUrl"].FirstOrDefault();
            }

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded)
                {
                    try
                    {
                        // Merge cart từ session vào database sau khi login thành công
                        var user = await _userManager.FindByEmailAsync(model.Email);
                        if (user != null)
                        {
                            await _cartService.MergeSessionCartToDatabaseAsync(user.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"Error merging cart: {ex.Message}");
                    }

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Cart");
                    }
                }
                ModelState.AddModelError("", "Invalid login attempt");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Customer/Auth/Login.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "AuthCustomer");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}
