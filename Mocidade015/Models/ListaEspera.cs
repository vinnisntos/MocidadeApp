using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mocidade015.Models
{
    public class ListaEspera
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UsuarioId { get; set; }

        [Required]
        [StringLength(50)]
        public string TerminalDesejado { get; set; } = string.Empty;

        public DateTime DataSolicitacao { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UsuarioId))]
        public virtual Usuario? Usuario { get; set; }
    }
}
