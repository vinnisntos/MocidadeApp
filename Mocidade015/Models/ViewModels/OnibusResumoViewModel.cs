namespace Mocidade015.Models.ViewModels
{
    /// <summary>
    /// Resumo de um ônibus para o dashboard do passageiro.
    /// </summary>
    public class OnibusResumoViewModel
    {
        public Guid Id { get; set; }
        public int Numero { get; set; }
        public string Terminal { get; set; } = string.Empty;
        public TimeSpan HorarioSaida { get; set; }
        public int LotacaoMaxima { get; set; }
        public int Ocupados { get; set; }
        public int Disponiveis => Math.Max(0, LotacaoMaxima - Ocupados);
        public bool EstaLotado => Ocupados >= LotacaoMaxima;
        public DateTime DataViagem { get; set; }
    }
}
