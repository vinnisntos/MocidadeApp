using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mocidade015.Data;
using Mocidade015.Models;
using Mocidade015.Models.ViewModels;
using System.Security.Claims;

namespace Mocidade015.Pages
{
    public class LoginModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(AppDbContext context, ILogger<LoginModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public LoginInput Input { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return Page();

            var emailNormalizado = Input.Email.Trim().ToLowerInvariant();

            var user = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == emailNormalizado);

            if (user == null || !BCrypt.Net.BCrypt.Verify(Input.Senha, user.SenhaHash))
            {
                _logger.LogWarning("Tentativa de login falhou para {Email}", emailNormalizado);
                ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
                return Page();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Nome),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(5)
                });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return user.Role == "Admin"
                ? RedirectToPage("/Admin/Index")
                : RedirectToPage("/App/Dashboard");
        }
    }
}
