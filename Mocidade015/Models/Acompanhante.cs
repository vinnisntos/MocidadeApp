using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mocidade015.Models
{
    public class Acompanhante
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UsuarioResponsavelId { get; set; }

        [Required]
        [StringLength(120, MinimumLength = 3)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(20)]
        public string? RgCpf { get; set; }

        [Phone]
        [StringLength(20)]
        public string? Telefone { get; set; }

        [ForeignKey(nameof(UsuarioResponsavelId))]
        public Usuario UsuarioResponsavel { get; set; } = null!;
    }
}
