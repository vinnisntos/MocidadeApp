using System.ComponentModel.DataAnnotations;

namespace Mocidade015.Models.ViewModels
{
    public class PerfilInput
    {
        [Required(ErrorMessage = "Informe seu nome.")]
        [StringLength(120, MinimumLength = 3)]
        [Display(Name = "Nome Completo")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe seu e-mail.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [StringLength(160)]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe seu RG ou CPF.")]
        [StringLength(20)]
        [Display(Name = "RG ou CPF")]
        public string Rg { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        [Display(Name = "Telefone")]
        public string? Telefone { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
        [Display(Name = "Nova senha")]
        public string? NovaSenha { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(NovaSenha), ErrorMessage = "As senhas não conferem.")]
        [Display(Name = "Confirmar nova senha")]
        public string? ConfirmarNovaSenha { get; set; }
    }
}
