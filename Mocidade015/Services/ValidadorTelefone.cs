using System.Text.RegularExpressions;

namespace Mocidade015.Services
{
    /// <summary>
    /// Validador de telefone brasileiro com suporte a DDD.
    /// Valida formato (11) 9XXXX-XXXX e (11) XXXX-XXXX
    /// </summary>
    public static class ValidadorTelefone
    {
        /// <summary>
        /// DDDs válidos na Brasil (11-99)
        /// </summary>
        private static readonly int[] DDDsValidos = new[]
        {
            11, 12, 13, 14, 15, 16, 17, 18, 19, // São Paulo
            21, 22, 24,                           // Rio de Janeiro
            27, 28,                               // Espírito Santo
            31, 32, 33, 34, 35, 37, 38,         // Minas Gerais
            41, 42, 43, 44, 45, 46,             // Paraná e Santa Catarina
            47, 48, 49,                         // Santa Catarina
            51, 53, 54, 55,                     // Rio Grande do Sul
            61,                                  // Distrito Federal
            62, 64,                             // Goiás
            63,                                  // Tocantins
            65, 66,                             // Mato Grosso
            67,                                  // Mato Grosso do Sul
            68,                                  // Acre
            69,                                  // Rondônia
            71, 73, 74, 75, 77,                 // Bahia
            79,                                  // Sergipe
            81, 87,                             // Pernambuco
            82,                                  // Alagoas
            83,                                  // Paraíba
            84,                                  // Rio Grande do Norte
            85, 88,                             // Ceará
            86, 89,                             // Piauí
            92, 97,                             // Amazonas
            93, 94,                             // Roraima e Amapá
            95                                   // Pará
        };

        /// <summary>
        /// Valida telefone brasileiro no formato (XX) XXXXX-XXXX ou (XX) XXXX-XXXX
        /// </summary>
        /// <param name="telefone">Telefone com ou sem máscara</param>
        /// <returns>True se válido, false caso contrário</returns>
        public static bool ValidarTelefone(string? telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return false;

            // Remove tudo que não é número
            var apenasNumeros = Regex.Replace(telefone, @"\D", "");

            // Deve ter 11 dígitos (2 de DDD + 9 de número)
            if (apenasNumeros.Length != 11)
                return false;

            // Extrai DDD
            var ddd = int.Parse(apenasNumeros.Substring(0, 2));
            if (!DDDsValidos.Contains(ddd))
                return false;

            // Extrai o nono dígito (posição 4)
            var nonoDigito = apenasNumeros[4];

            // Deve ser 9 para celular ou 8 para fixo em alguns casos
            if (nonoDigito != '9' && nonoDigito != '8')
                return false;

            // Se for 8, deve ser fixo (começa com 2-5 normalmente)
            // Se for 9, deve ser celular
            if (nonoDigito == '8')
            {
                var primeiroDigitoNumero = int.Parse(apenasNumeros[2].ToString());
                // Fixos em SP começam com 2, 3, 4, 5
                if (!(primeiroDigitoNumero >= 2 && primeiroDigitoNumero <= 5))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Formata telefone para o padrão (XX) 9XXXX-XXXX ou (XX) XXXX-XXXX
        /// </summary>
        public static string FormatarTelefone(string? telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return "";

            var apenasNumeros = Regex.Replace(telefone, @"\D", "");
            if (apenasNumeros.Length != 11)
                return apenasNumeros;

            return $"({apenasNumeros.Substring(0, 2)}) {apenasNumeros.Substring(2, 5)}-{apenasNumeros.Substring(7, 4)}";
        }

        /// <summary>
        /// Remove máscara do telefone (retorna apenas dígitos)
        /// </summary>
        public static string RemoverMascara(string? telefone)
        {
            return Regex.Replace(telefone ?? "", @"\D", "");
        }

        /// <summary>
        /// Retorna o DDD de um telefone
        /// </summary>
        public static string ExtrairDDD(string? telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return "";

            var apenasNumeros = Regex.Replace(telefone, @"\D", "");
            if (apenasNumeros.Length < 2)
                return "";

            return apenasNumeros.Substring(0, 2);
        }

        /// <summary>
        /// Retorna o Estado/Região baseado no DDD
        /// </summary>
        public static string ExtrairEstado(string? ddd)
        {
            if (!int.TryParse(ddd, out var dddNum))
                return "Desconhecido";

            return dddNum switch
            {
                11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 => "SP",
                21 or 22 or 24 => "RJ",
                27 or 28 => "ES",
                31 or 32 or 33 or 34 or 35 or 37 or 38 => "MG",
                41 or 42 or 43 or 44 or 45 or 46 => "PR",
                47 or 48 or 49 => "SC",
                51 or 53 or 54 or 55 => "RS",
                61 => "DF",
                62 or 64 => "GO",
                63 => "TO",
                65 or 66 => "MT",
                67 => "MS",
                68 => "AC",
                69 => "RO",
                71 or 73 or 74 or 75 or 77 => "BA",
                79 => "SE",
                81 or 87 => "PE",
                82 => "AL",
                83 => "PB",
                84 => "RN",
                85 or 88 => "CE",
                86 or 89 => "PI",
                92 or 97 => "AM",
                93 or 94 => "RR/AP",
                95 => "PA",
                _ => "Desconhecido"
            };
        }
    }
}
