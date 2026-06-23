using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mocidade015.Models
{
    public class Assento
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OnibusId { get; set; }

        [Range(1, 200)]
        public int Numero { get; set; }

        public bool Ocupado { get; set; }

        [ForeignKey(nameof(OnibusId))]
        public Onibus Onibus { get; set; } = null!;
    }
}
