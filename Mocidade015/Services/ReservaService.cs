using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mocidade015.Data;
using Mocidade015.Models;
using Mocidade015.Models.Options;

namespace Mocidade015.Services
{
    /// <summary>
    /// Implementação do serviço de reservas com:
    /// • Lock transacional (Serializable) para evitar corrida em assentos concorridos.
    /// • Horários e data vindos de <see cref="ViagemOptions"/> — sem hardcode.
    /// • UTC padronizado em todo o ciclo de vida.
    /// </summary>
    public class ReservaService : IReservaService
    {
        private readonly AppDbContext _context;
        private readonly ViagemOptions _viagem;
        private readonly ILogger<ReservaService> _logger;

        public ReservaService(
            AppDbContext context,
            IOptions<ViagemOptions> viagemOptions,
            ILogger<ReservaService> logger)
        {
            _context = context;
            _viagem = viagemOptions.Value;
            _logger = logger;
        }

        public async Task<bool> ReservarAssentoAsync(Guid usuarioId, Guid assentoId, Guid? acompanhanteId)
        {
            // 1. Verificações fora de transação (apenas leitura de validação).
            var assento = await _context.Assentos
                .Include(a => a.Onibus)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == assentoId);

            if (assento == null || assento.Ocupado)
                return false;

            // 2. Bloqueio: usamos Serializable + retry leve para concorrência de dois clientes
            //    clicando no mesmo assento ao mesmo tempo. Funciona com Postgres + EF Core.
            using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
                {
                    // Re-busca com tracking dentro da transação
                    var assentoLock = await _context.Assentos
                        .Include(a => a.Onibus)
                        .FirstOrDefaultAsync(a => a.Id == assentoId);

                    if (assentoLock == null || assentoLock.Ocupado)
                    {
                        await tx.RollbackAsync();
                        return false;
                    }

                    // Já existe reserva para esse assento? Checagem dupla (índice único também protege).
                    bool assentoJaReservado = await _context.Reservas
                        .AnyAsync(r => r.AssentoId == assentoId);

                    if (assentoJaReservado)
                    {
                        await tx.RollbackAsync();
                        return false;
                    }

                    // Não permite reserva duplicada do mesmo passageiro (titular ou acompanhante) no mesmo ônibus.
                    bool jaEstaNoOnibus = await _context.Reservas
                        .AnyAsync(r => r.Assento.OnibusId == assentoLock.OnibusId &&
                                       ((acompanhanteId != null && r.AcompanhanteId == acompanhanteId) ||
                                        (acompanhanteId == null && r.UsuarioId == usuarioId && r.AcompanhanteId == null)));

                    if (jaEstaNoOnibus)
                    {
                        await tx.RollbackAsync();
                        return false;
                    }

                    assentoLock.Ocupado = true;

                    var reserva = new Reserva
                    {
                        Id = Guid.NewGuid(),
                        UsuarioId = usuarioId,
                        AcompanhanteId = acompanhanteId,
                        AssentoId = assentoId,
                        Valor = 80.00m,
                        DataReserva = DateTime.UtcNow
                    };

                    _context.Reservas.Add(reserva);
                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();
                    return true;
                }
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(ex, "Conflito ao reservar assento {AssentoId}", assentoId);
                await tx.RollbackAsync();
                return false;
            }
            catch
            {
                await tx.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> CancelarReservaAsync(Guid reservaId)
        {
            using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var reserva = await _context.Reservas
                    .Include(r => r.Assento)
                    .FirstOrDefaultAsync(r => r.Id == reservaId);

                if (reserva == null) return false;

                if (reserva.Assento != null)
                    reserva.Assento.Ocupado = false;

                _context.Reservas.Remove(reserva);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                return false;
            }
        }

        public async Task VerificarEGerarNovoOnibusAsync(string terminal)
        {
            terminal = (terminal ?? string.Empty).Trim();

            var existeOnibusComAssentosLivres = await _context.Onibus
                .Where(o => o.TerminalSaida == terminal && o.DataViagem == _viagem.ObterDataViagem())
                .AnyAsync(o => o.Assentos.Any(a => !a.Ocupado));

            if (existeOnibusComAssentosLivres) return;

            var totalOnibusNoTerminal = await _context.Onibus
                .Where(o => o.TerminalSaida == terminal)
                .CountAsync();

            var novoOnibus = new Onibus
            {
                Id = Guid.NewGuid(),
                Numero = totalOnibusNoTerminal + 1,
                TerminalSaida = terminal,
                HorarioSaida = _viagem.ObterHorarioSaida(terminal),
                DataViagem = _viagem.ObterDataViagem(),
                LotacaoMaxima = _viagem.LotacaoPadrao,
                Ativo = true
            };

            _context.Onibus.Add(novoOnibus);
            await _context.SaveChangesAsync();

            for (int i = 1; i <= _viagem.LotacaoPadrao; i++)
            {
                _context.Assentos.Add(new Assento
                {
                    Id = Guid.NewGuid(),
                    OnibusId = novoOnibus.Id,
                    Numero = i,
                    Ocupado = false
                });
            }
            await _context.SaveChangesAsync();

            _logger.LogInformation("Novo ônibus gerado para o terminal {Terminal}: #{Numero}", terminal, novoOnibus.Numero);
        }

        public async Task<bool> AdicionarNaListaDeEsperaAsync(Guid usuarioId, string terminalDesejado)
        {
            terminalDesejado = (terminalDesejado ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(terminalDesejado)) return false;

            var jaEstaNaLista = await _context.ListaEspera
                .AnyAsync(l => l.UsuarioId == usuarioId && l.TerminalDesejado == terminalDesejado);

            if (jaEstaNaLista) return false;

            // Se já tem reserva no terminal, não precisa entrar na lista.
            var jaTemReserva = await _context.Reservas
                .Include(r => r.Assento)
                .ThenInclude(a => a.Onibus)
                .AnyAsync(r => r.UsuarioId == usuarioId &&
                               r.Assento != null &&
                               r.Assento.Onibus != null &&
                               r.Assento.Onibus.TerminalSaida == terminalDesejado);

            if (jaTemReserva) return false;

            _context.ListaEspera.Add(new ListaEspera
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                TerminalDesejado = terminalDesejado,
                DataSolicitacao = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> JaEstaNaListaDeEsperaAsync(Guid usuarioId, string terminalDesejado)
        {
            terminalDesejado = (terminalDesejado ?? string.Empty).Trim();
            return await _context.ListaEspera
                .AnyAsync(l => l.UsuarioId == usuarioId && l.TerminalDesejado == terminalDesejado);
        }

        public async Task<int> GetAssentosDisponiveisCountAsync(Guid onibusId)
        {
            return await _context.Assentos
                .Where(a => a.OnibusId == onibusId && !a.Ocupado)
                .CountAsync();
        }
    }
}
