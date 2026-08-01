using System;
using System.Threading.Tasks;

namespace Mocidade015.Services
{
    public interface IReservaService
    {
        /// <summary>Tenta reservar um assento. Retorna true em caso de sucesso.</summary>
        Task<bool> ReservarAssentoAsync(Guid usuarioId, Guid assentoId, Guid? acompanhanteId);

        /// <summary>Cancela uma reserva e libera o assento para nova marcação.</summary>
        Task<bool> CancelarReservaAsync(Guid reservaId);

        /// <summary>Gera um novo ônibus para o terminal, sob decisão manual do admin.</summary>
        Task GerarNovoOnibusAsync(string terminal);

        /// <summary>Adiciona um usuário na lista de espera de um terminal.</summary>
        Task<bool> AdicionarNaListaDeEsperaAsync(Guid usuarioId, string terminalDesejado);

        /// <summary>Verifica se o usuário já está na lista de espera do terminal.</summary>
        Task<bool> JaEstaNaListaDeEsperaAsync(Guid usuarioId, string terminalDesejado);

        /// <summary>Conta quantos assentos livres ainda existem no ônibus.</summary>
        Task<int> GetAssentosDisponiveisCountAsync(Guid onibusId);

        /// <summary>
        /// Atribui, pelo admin, um assento vago a alguém da lista de espera: cria a reserva,
        /// ocupa o assento e remove a pessoa da fila. Retorna false se o assento não estiver mais
        /// disponível ou se o registro de espera não existir mais.
        /// </summary>
        Task<bool> AtribuirVagaDaEsperaAsync(Guid esperaId, Guid assentoId);
    }
}
