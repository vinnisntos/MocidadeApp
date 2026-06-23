using System.Text;

namespace Mocidade015.Services
{
    /// <summary>
    /// Serviço de rate limiting em memória.
    /// Implementação simples sem Redis para MVP.
    /// </summary>
    public interface IRateLimitService
    {
        /// <summary>
        /// Verifica se uma chave excedeu o limite de tentativas
        /// </summary>
        /// <param name="chave">Chave única (ex: "login:user@email.com", "ip:192.168.1.1")</param>
        /// <param name="maxTentativas">Máximo de tentativas permitidas</param>
        /// <param name="janela">Janela de tempo para contar as tentativas</param>
        /// <returns>True se PERMITIDO, false se BLOQUEADO</returns>
        Task<bool> VerificarLimiteAsync(string chave, int maxTentativas, TimeSpan janela);

        /// <summary>
        /// Limpa o histórico de tentativas de uma chave
        /// </summary>
        Task LimparAsync(string chave);
    }

    public class RateLimitService : IRateLimitService
    {
        private readonly Dictionary<string, RateLimitData> _tentativas = new();
        private readonly object _lock = new();

        private class RateLimitData
        {
            public int Contador { get; set; }
            public DateTime ExpirationTime { get; set; }
        }

        public async Task<bool> VerificarLimiteAsync(string chave, int maxTentativas, TimeSpan janela)
        {
            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    var agora = DateTime.UtcNow;

                    // Remove entradas expiradas
                    var expiradasChaves = _tentativas
                        .Where(x => x.Value.ExpirationTime < agora)
                        .Select(x => x.Key)
                        .ToList();

                    foreach (var k in expiradasChaves)
                        _tentativas.Remove(k);

                    // Verifica se a chave existe e não expirou
                    if (_tentativas.TryGetValue(chave, out var data))
                    {
                        // Se expirou, cria novo
                        if (data.ExpirationTime < agora)
                        {
                            data.Contador = 1;
                            data.ExpirationTime = agora.Add(janela);
                            return true;
                        }

                        // Se atingiu limite, nega
                        if (data.Contador >= maxTentativas)
                            return false;

                        // Incrementa tentativa
                        data.Contador++;
                        return true;
                    }

                    // Primeira tentativa
                    _tentativas[chave] = new RateLimitData
                    {
                        Contador = 1,
                        ExpirationTime = agora.Add(janela)
                    };

                    return true;
                }
            });
        }

        public async Task LimparAsync(string chave)
        {
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    _tentativas.Remove(chave);
                }
            });
        }

        /// <summary>
        /// Retorna informações sobre o rate limit de uma chave
        /// </summary>
        public (int tentativas, TimeSpan tempoRestante) ObterStatus(string chave)
        {
            lock (_lock)
            {
                if (_tentativas.TryGetValue(chave, out var data))
                {
                    var tempoRestante = data.ExpirationTime - DateTime.UtcNow;
                    if (tempoRestante < TimeSpan.Zero)
                        tempoRestante = TimeSpan.Zero;

                    return (data.Contador, tempoRestante);
                }

                return (0, TimeSpan.Zero);
            }
        }

        /// <summary>
        /// Retorna quantidade de chaves ativas
        /// </summary>
        public int ObterQuantidadeChavasAtivas()
        {
            lock (_lock)
            {
                return _tentativas.Count(x => x.Value.ExpirationTime > DateTime.UtcNow);
            }
        }
    }
}
