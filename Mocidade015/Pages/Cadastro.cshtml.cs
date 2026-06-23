using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mocidade015.Data;
using Mocidade015.Models;
using Mocidade015.Models.ViewModels;

namespace Mocidade015.Pages
{
    public class CadastroModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CadastroModel> _logger;

        public CadastroModel(AppDbContext context, ILogger<CadastroModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public CadastroInput Input { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var emailNorm = Input.Email.Trim().ToLowerInvariant();
            var rgNorm = Input.Rg.Trim();

            if (await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == emailNorm))
            {
                ModelState.AddModelError(nameof(Input.Email), "Este e-mail já está cadastrado.");
                return Page();
            }

            if (await _context.Usuarios.AnyAsync(u => u.Rg != null && u.Rg == rgNorm))
            {
                ModelState.AddModelError(nameof(Input.Rg), "Este RG/CPF já está cadastrado.");
                return Page();
            }

            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = Input.Nome.Trim(),
                Email = emailNorm,
                Rg = rgNorm,
                Telefone = string.IsNullOrWhiteSpace(Input.Telefone) ? null : Input.Telefone.Trim(),
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(Input.Senha),
                Role = "Cliente",
                DataCriacao = DateTime.UtcNow
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Novo usuário cadastrado: {Email}", emailNorm);

            TempData["Toast"] = "success|Conta criada com sucesso! Faça login para continuar.";
            return RedirectToPage("/Login");
        }
    }
}
