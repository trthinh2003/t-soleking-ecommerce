using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using SoleKingECommerce.Dtos.Auth;
using SoleKingECommerce.Models;
using SoleKingECommerce.Services.Interfaces;

namespace SoleKingECommerce.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
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

        [HttpGet("login")]
        public IActionResult Login()
        {
            // Return a view for login
            return Ok(new { message = "Login view" });
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            // Return a view for registration
            return Ok(new { message = "Registration view" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var user = new User
            {
                Address = registerDto.Address,
                PhoneNumber = registerDto.PhoneNumber,
                UserName = registerDto.Email,
                Email = registerDto.Email,
                UserType = registerDto.UserType
            };
            var result = await _userManager.CreateAsync(user, registerDto.Password ?? string.Empty);

            if (result.Succeeded)
            {
                // Tạo token xác thực
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account",
                    new { userId = user.Id, token }, Request.Scheme);

                await _emailSender.SendEmailAsync(user.Email!, "Xác nhận email",
                    $"Vui lòng xác nhận tài khoản bằng cách <a href='{confirmationLink}'>bấm vào đây</a>.");

                return Ok(new { message = "Registration successful. Please check your email to confirm your account." });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return BadRequest(new { message = "Registration failed" });
        }
    }
}
