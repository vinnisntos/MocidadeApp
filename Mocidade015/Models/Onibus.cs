using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mocidade015.Models
{
    public class Onibus
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public int Numero { get; set; }

        [Required]
        [StringLength(80)]
        public string TerminalSaida { get; set; } = string.Empty;

        public TimeSpan HorarioSaida { get; set; }

        [Range(1, 200)]
        public int LotacaoMaxima { get; set; } = 64;

        public DateTime DataViagem { get; set; }

        public bool Ativo { get; set; } = true;

        public ICollection<Assento> Assentos { get; set; } = new List<Assento>();
    }
}
