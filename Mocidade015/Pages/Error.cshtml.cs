using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Mocidade015.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    {
        private readonly ILogger<ErrorModel> _logger;
        public ErrorModel(ILogger<ErrorModel> logger) => _logger = logger;

        public void OnGet()
        {
            // Erro genérico. Em produção evitamos exibir detalhes do ambiente.
            _logger.LogError("Página de erro acessada pelo usuário {User}", User.Identity?.Name ?? "anônimo");
        }
    }
}
