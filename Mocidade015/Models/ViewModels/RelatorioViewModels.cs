namespace Mocidade015.Models.ViewModels
{
    /// <summary>
    /// Linha do relatório administrativo — passagem + passageiro.
    /// </summary>
    public class ReservaResumoViewModel
    {
        public Guid Id { get; set; }
        public int NumeroAssento { get; set; }
        public string NomePassageiro { get; set; } = string.Empty;
        public string DocumentoPassageiro { get; set; } = string.Empty;
        public string? TelefonePassageiro { get; set; }
        public string TipoPassageiro { get; set; } = "Titular";
    }

    public class OnibusRelatorioViewModel
    {
        public Guid Id { get; set; }
        public int Numero { get; set; }
        public string Terminal { get; set; } = string.Empty;
        public int LotacaoMaxima { get; set; }
        public List<ReservaResumoViewModel> Reservas { get; set; } = new();
        public int TotalOcupados => Reservas.Count;
    }

    public class ResumoEsperaViewModel
    {
        public string Terminal { get; set; } = string.Empty;
        public int Total { get; set; }
    }
}
