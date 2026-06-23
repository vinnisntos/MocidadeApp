using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mocidade015.Models
{
    public class Reserva
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UsuarioId { get; set; }

        [Required]
        public Guid AssentoId { get; set; }

        public Guid? AcompanhanteId { get; set; }

        [Range(0, 10000)]
        public decimal Valor { get; set; } = 80.00m;

        public DateTime DataReserva { get; set; } = DateTime.UtcNow;

        public Usuario Usuario { get; set; } = null!;
        public Assento Assento { get; set; } = null!;
        public Acompanhante? Acompanhante { get; set; }
    }
}
