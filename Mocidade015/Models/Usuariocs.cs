using System.ComponentModel.DataAnnotations;

namespace Mocidade015.Models
{
    public class Usuario
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(120, MinimumLength = 3)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(160)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Rg { get; set; }

        [Phone]
        [StringLength(20)]
        public string? Telefone { get; set; }

        [Required]
        public string SenhaHash { get; set; } = string.Empty;

        /// <summary>"Cliente" | "Admin". Promovido manualmente no banco.</summary>
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "Cliente";

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public ICollection<Acompanhante> Acompanhantes { get; set; } = new List<Acompanhante>();
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}
