using System.Text.RegularExpressions;

namespace Mocidade015.Services
{
    /// <summary>
    /// Validador de CPF com algoritmo de dígitos verificadores.
    /// Rejeita CPF inválido, sequências e formato incorreto.
    /// </summary>
    public static class ValidadorCPF
    {
        /// <summary>
        /// Valida CPF conforme lei brasileira de dígitos verificadores.
        /// </summary>
        /// <param name="cpf">CPF com ou sem máscara (ex: "123.456.789-09" ou "12345678909")</param>
        /// <returns>True se válido, false caso contrário</returns>
        public static bool ValidarCPF(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            // Remove máscara (tudo que não é número)
            cpf = Regex.Replace(cpf, @"\D", "");

            // Deve ter exatamente 11 dígitos
            if (cpf.Length != 11)
                return false;

            // Rejeita sequências com todos os dígitos iguais
            // (000.000.000-00, 111.111.111-11, etc.)
            if (cpf == new string(cpf[0], 11))
                return false;

            // Valida primeiro dígito verificador
            int primeiroDigito = CalcularDigitoVerificador(cpf, 10);
            if ((cpf[9] - '0') != primeiroDigito)
                return false;

            // Valida segundo dígito verificador
            int segundoDigito = CalcularDigitoVerificador(cpf, 11);
            if ((cpf[10] - '0') != segundoDigito)
                return false;

            return true;
        }

        /// <summary>
        /// Calcula dígito verificador de CPF
        /// </summary>
        private static int CalcularDigitoVerificador(string cpf, int multiplicador)
        {
            int soma = 0;
            for (int i = 0; i < multiplicador - 1; i++)
            {
                soma += (cpf[i] - '0') * (multiplicador - i);
            }

            int resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }

        /// <summary>
        /// Formata CPF para o padrão 000.000.000-00
        /// </summary>
        public static string FormatarCPF(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return "";

            cpf = Regex.Replace(cpf, @"\D", "");
            if (cpf.Length != 11)
                return cpf;

            return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";
        }

        /// <summary>
        /// Remove máscara do CPF (retorna apenas dígitos)
        /// </summary>
        public static string RemoverMascara(string? cpf)
        {
            return Regex.Replace(cpf ?? "", @"\D", "");
        }

        /// <summary>
        /// Valida se uma lista de CPFs contém algum duplicado
        /// </summary>
        public static bool ContemDuplicado(IEnumerable<string> cpfs)
        {
            var cpfsDeLimpeza = cpfs.Select(RemoverMascara).ToList();
            return cpfsDeLimpeza.Count != cpfsDeLimpeza.Distinct().Count();
        }
    }
}
