using System.ComponentModel.DataAnnotations;

namespace Mocidade015.Models.ViewModels
{
    /// <summary>
    /// Entrada do formulário de login. Encapsulamos em uma classe para
    /// permitir evolução do contrato (ex.: "Lembrar de mim") sem reescrever a página.
    /// </summary>
    public class LoginInput
    {
        [Required(ErrorMessage = "Informe seu e-mail.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe sua senha.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Senha { get; set; } = string.Empty;
    }
}
