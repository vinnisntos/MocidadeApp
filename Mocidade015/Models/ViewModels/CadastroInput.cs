using System.ComponentModel.DataAnnotations;
using Mocidade015.Services;

namespace Mocidade015.Models.ViewModels
{
    public class CadastroInput
    {
        [Required(ErrorMessage = "Informe seu nome completo.")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 120 caracteres.")]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ\s]+$", ErrorMessage = "Nome deve conter apenas letras.")]
        [Display(Name = "Nome Completo")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe seu e-mail.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [StringLength(160)]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe seu RG.")]
        [StringLength(20, ErrorMessage = "O RG não pode exceder 20 caracteres.")]
        [Display(Name = "RG")]
        public string Rg { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Telefone inválido.")]
        [StringLength(15, MinimumLength = 10, ErrorMessage = "Telefone deve ser válido.")]
        [CustomValidation(typeof(CadastroInputValidator), nameof(CadastroInputValidator.ValidarTelefone))]
        [Display(Name = "Telefone")]
        public string? Telefone { get; set; }

        [Required(ErrorMessage = "Crie uma senha.")]
        [StringLength(100, MinimumLength = 12, ErrorMessage = "A senha deve ter pelo menos 12 caracteres.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{12,}$",
            ErrorMessage = "Senha deve ter: maiúscula, minúscula, número e símbolo (@$!%*?&).")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme sua senha.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Senha), ErrorMessage = "As senhas não conferem.")]
        [Display(Name = "Confirmar senha")]
        public string ConfirmarSenha { get; set; } = string.Empty;
    }

    /// <summary>
    /// Validators customizados para CadastroInput
    /// </summary>
    public static class CadastroInputValidator
    {
        public static ValidationResult? ValidarTelefone(string? telefone, ValidationContext context)
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return ValidationResult.Success; // Opcional

            if (!ValidadorTelefone.ValidarTelefone(telefone))
                return new ValidationResult("Telefone inválido. Use o formato (XX) 9XXXX-XXXX");

            return ValidationResult.Success;
        }
    }
}