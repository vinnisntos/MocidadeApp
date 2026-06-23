namespace Mocidade015.Models.Options
{
    /// <summary>
    /// Configuração centralizada da viagem. Permite trocar a data, lotação e horários
    /// sem mexer em código (e sem perder o histórico de migrations).
    /// </summary>
    public class ViagemOptions
    {
        public const string Section = "Viagem";

        /// <summary>Data da viagem (yyyy-MM-dd).</summary>
        public string Data { get; set; } = "2026-07-26";

        /// <summary>Lotação máxima de cada ônibus.</summary>
        public int LotacaoPadrao { get; set; } = 64;

        /// <summary>Mapa de horário de saída por terminal (HH:mm).</summary>
        public Dictionary<string, string> HorariosPorTerminal { get; set; } = new();

        public DateTime ObterDataViagem()
        {
            if (DateTime.TryParse(Data, out var data))
                return DateTime.SpecifyKind(data.Date, DateTimeKind.Utc);
            return new DateTime(2026, 7, 26, 0, 0, 0, DateTimeKind.Utc);
        }

        public TimeSpan ObterHorarioSaida(string terminal)
        {
            if (HorariosPorTerminal.TryGetValue(terminal, out var horario) &&
                TimeSpan.TryParse(horario, out var ts))
            {
                return ts;
            }
            return new TimeSpan(12, 0, 0);
        }
    }

    public class ContatoOptions
    {
        public const string Section = "Contato";

        public string WhatsappPagamento { get; set; } = "5515981623434";
        public string NomeAdmin { get; set; } = "Administrador";
    }
}
