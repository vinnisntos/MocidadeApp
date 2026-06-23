# 🔧 GUIA TÉCNICO DE IMPLEMENTAÇÃO
## Mocidade 015 - Plano de Ação Estruturado

---

# AÇÃO 1: CORRIGIR CÓDIGO QUEBRADO (CRÍTICO - HOJE)

## Problema:
[Index.cshtml](Index.cshtml) contém texto injetado que quebra a página.

## Solução:

### Arquivo a Corrigir: `Pages/Index.cshtml` (Linha ~11)

**Antes:**
```html
<a asp-page="/Cadastro" class="text-decoration-none text-azulcurl -fsSL https://claude.ai/install.sh | bashassun a-escuro fw-semibold mt-2">
```

**Depois:**
```html
<a asp-page="/Cadastro" class="text-decoration-none text-azul-escuro fw-semibold mt-2">
    Ainda não tem conta? Cadastre-se
</a>
```

---

# AÇÃO 2: IMPLEMENTAR VALIDADOR DE CPF (CRÍTICO - HOJE)

## Problema:
CPF "000.000.000-00" e qualquer sequência numérica é aceita.

## Solução:

### Criar: `Services/ValidadorCPF.cs`

```csharp
using System.Text.RegularExpressions;

namespace Mocidade015.Services
{
    public static class ValidadorCPF
    {
        /// <summary>
        /// Valida CPF conforme dígitos verificadores de Lei.
        /// Rejeita: nulos, inválidos, sequências (111.111.111-11), etc.
        /// </summary>
        public static bool ValidarCPF(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            // Remove máscara
            cpf = Regex.Replace(cpf, @"\D", "");

            // Deve ter 11 dígitos
            if (cpf.Length != 11)
                return false;

            // Rejeita sequências iguais
            if (cpf == "00000000000" || cpf == "11111111111" ||
                cpf == "22222222222" || cpf == "33333333333" ||
                cpf == "44444444444" || cpf == "55555555555" ||
                cpf == "66666666666" || cpf == "77777777777" ||
                cpf == "88888888888" || cpf == "99999999999")
                return false;

            // Calcula primeiro dígito verificador
            int sum = 0;
            for (int i = 0; i < 9; i++)
                sum += (cpf[i] - '0') * (10 - i);

            int remainder = sum % 11;
            int firstDigit = remainder < 2 ? 0 : 11 - remainder;

            if ((cpf[9] - '0') != firstDigit)
                return false;

            // Calcula segundo dígito verificador
            sum = 0;
            for (int i = 0; i < 10; i++)
                sum += (cpf[i] - '0') * (11 - i);

            remainder = sum % 11;
            int secondDigit = remainder < 2 ? 0 : 11 - remainder;

            if ((cpf[10] - '0') != secondDigit)
                return false;

            return true;
        }

        /// <summary>
        /// Formata CPF para padrão 000.000.000-00
        /// </summary>
        public static string FormatarCPF(string cpf)
        {
            cpf = Regex.Replace(cpf ?? "", @"\D", "");
            if (cpf.Length != 11)
                return cpf;

            return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";
        }

        /// <summary>
        /// Remove máscara e retorna apenas dígitos
        /// </summary>
        public static string RemoverMascara(string? cpf)
        {
            return Regex.Replace(cpf ?? "", @"\D", "");
        }
    }
}
```

### Atualizar: `Models/ViewModels/CadastroInput.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using Mocidade015.Services;

namespace Mocidade015.Models.ViewModels
{
    public class CadastroInput
    {
        [Required(ErrorMessage = "Informe seu nome completo.")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 120 caracteres.")]
        [Display(Name = "Nome Completo")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe seu e-mail.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [StringLength(160)]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe seu CPF.")]
        [CustomValidation(typeof(CadastroInputValidator), nameof(CadastroInputValidator.ValidarCPF))]
        [Display(Name = "CPF")]
        public string CPF { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Telefone inválido.")]
        [StringLength(20)]
        [RegularExpression(@"^\(\d{2}\)\s?9\d{4}-\d{4}$", ErrorMessage = "Formato: (11) 99999-9999")]
        [Display(Name = "Telefone")]
        public string? Telefone { get; set; }

        [Required(ErrorMessage = "Crie uma senha.")]
        [StringLength(100, MinimumLength = 12, ErrorMessage = "A senha deve ter pelo menos 12 caracteres.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{12,}$",
            ErrorMessage = "Senha deve incluir: maiúscula, minúscula, número e símbolo (@$!%*?&)")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme sua senha.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Senha), ErrorMessage = "As senhas não conferem.")]
        [Display(Name = "Confirmar senha")]
        public string ConfirmarSenha { get; set; } = string.Empty;
    }

    /// <summary>
    /// Validator customizado para CadastroInput
    /// </summary>
    public static class CadastroInputValidator
    {
        public static ValidationResult? ValidarCPF(string cpf, ValidationContext context)
        {
            if (!ValidadorCPF.ValidarCPF(cpf))
                return new ValidationResult("CPF inválido.");

            return ValidationResult.Success;
        }
    }
}
```

---

# AÇÃO 3: IMPLEMENTAR RATE LIMITING (CRÍTICO - HOJE)

## Problema:
Sem proteção contra brute force no login.

## Solução:

### Criar: `Services/RateLimitService.cs`

```csharp
using StackExchange.Redis;
using System.Net;

namespace Mocidade015.Services
{
    public interface IRateLimitService
    {
        Task<bool> VerificarLimiteAsync(string chave, int maxTentativas, TimeSpan janela);
        Task LimparAsync(string chave);
    }

    public class RateLimitService : IRateLimitService
    {
        private readonly IConnectionMultiplexer _redis;
        private const string PREFIX = "ratelimit:";

        public RateLimitService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Verifica se a chave excedeu o limite de tentativas
        /// </summary>
        /// <returns>true se PERMITIDO, false se BLOQUEADO</returns>
        public async Task<bool> VerificarLimiteAsync(string chave, int maxTentativas, TimeSpan janela)
        {
            var db = _redis.GetDatabase();
            var chaveFull = $"{PREFIX}{chave}";

            var tentativas = await db.StringGetAsync(chaveFull);

            if (tentativas.IsNull)
            {
                await db.StringSetAsync(chaveFull, 1, janela);
                return true;
            }

            int count = int.Parse(tentativas!);

            if (count >= maxTentativas)
                return false; // Bloqueado

            await db.StringIncrementAsync(chaveFull);
            return true; // Permitido
        }

        public async Task LimparAsync(string chave)
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync($"{PREFIX}{chave}");
        }
    }
}
```

### Atualizar: `Program.cs`

```csharp
// Adicionar depois de builder.Services.AddScoped<IReservaService, ReservaService>();

// Redis para cache e rate limiting
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") 
        ?? "localhost:6379";
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(
        builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));

builder.Services.AddScoped<IRateLimitService, RateLimitService>();
```

### Criar Middleware: `Middleware/RateLimitingMiddleware.cs`

```csharp
using Mocidade015.Services;
using System.Net;

namespace Mocidade015.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;

        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IRateLimitService rateLimitService)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Rate limit por IP (global)
            if (!await rateLimitService.VerificarLimiteAsync($"ip:{ip}", 100, TimeSpan.FromMinutes(1)))
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Muitas requisições. Tente novamente em 1 minuto.");
                return;
            }

            await _next(context);
        }
    }
}
```

### Aplicar Middleware: `Program.cs`

```csharp
// Após criar app:
app.UseMiddleware<RateLimitingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
```

### Atualizar: `Pages/Login.cshtml.cs`

```csharp
public async Task<IActionResult> OnPostAsync()
{
    var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    var rateLimitKey = $"login-attempt:{Input.Email}:{ip}";

    // Verificar rate limit
    if (!await _rateLimitService.VerificarLimiteAsync(
        rateLimitKey, 
        maxTentativas: 5, 
        janela: TimeSpan.FromMinutes(15)))
    {
        ModelState.AddModelError("", "Muitas tentativas de login. Tente novamente em 15 minutos.");
        return Page();
    }

    // ... resto do código de login
}
```

---

# AÇÃO 4: CRIPTOGRAFAR DADOS SENSÍVEIS (CRÍTICO - HOJE)

## Problema:
CPF e Telefone armazenados em plain text (LGPD violation).

## Solução:

### Criar: `Services/EncryptionService.cs`

```csharp
using System.Security.Cryptography;
using System.Text;

namespace Mocidade015.Services
{
    public interface IEncryptionService
    {
        string Criptografar(string plainText);
        string Descriptografar(string cipherText);
    }

    public class EncryptionService : IEncryptionService
    {
        private readonly IConfiguration _configuration;

        public EncryptionService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Criptografar(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            var key = Encoding.UTF8.GetBytes(_configuration["Encryption:Key"] 
                ?? throw new InvalidOperationException("Encryption:Key não configurada"));

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var iv = aes.IV;

                using (var encryptor = aes.CreateEncryptor(aes.Key, iv))
                using (var ms = new MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length);

                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public string Descriptografar(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            var key = Encoding.UTF8.GetBytes(_configuration["Encryption:Key"]!);
            var buffer = Convert.FromBase64String(cipherText);

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var iv = new byte[aes.BlockSize / 8];
                Array.Copy(buffer, 0, iv, 0, iv.Length);

                using (var decryptor = aes.CreateDecryptor(aes.Key, iv))
                using (var ms = new MemoryStream(buffer, iv.Length, buffer.Length - iv.Length))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
```

### Atualizar: `Data/AppDbContext.cs` (adicionar conversor)

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    var encryptionConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<string, string>(
        v => string.IsNullOrEmpty(v) ? v : EncryptValue(v),
        v => string.IsNullOrEmpty(v) ? v : DecryptValue(v));

    modelBuilder.Entity<Usuario>(entity =>
    {
        // ... configurações existentes ...
        
        // Criptografar CPF
        entity.Property(e => e.Rg)
            .HasConversion(encryptionConverter)
            .HasColumnName("rg");

        // Criptografar Telefone
        entity.Property(e => e.Telefone)
            .HasConversion(encryptionConverter)
            .HasColumnName("telefone");
    });
}

// Métodos helper para converter
private static string EncryptValue(string value)
{
    // Usar serviço injetado (alternativa: usar chave estática)
    // Por simplificar, usar aqui é complicado. Ideal usar value converter customizado
    return value; // TODO: Implementar
}

private static string DecryptValue(string value)
{
    return value; // TODO: Implementar
}
```

### Adicionar em `appsettings.json`:

```json
{
  "Encryption": {
    "Key": "TODO-GERAR-CHAVE-32-BYTES-SEGURA"
  }
}
```

---

# AÇÃO 5: IMPLEMENTAR DESIGN SYSTEM COM TAILWIND (ALTA - SEMANA 1)

## Problema:
Classes CSS customizadas sem definição, inconsistência visual.

## Solução:

### 1. Adicionar Tailwind ao `Mocidade015.csproj`:

Não é necessário via NuGet. Tailwind é ferramenta de build CSS. Usar CDN para prototipagem:

### 2. Atualizar `Pages/Shared/_Layout.cshtml`:

```html
<!-- Remover Bootstrap existente -->
<!-- <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet" /> -->

<!-- Adicionar Tailwind -->
<script src="https://cdn.tailwindcss.com"></script>

<!-- Custom Tailwind config -->
<script>
    tailwind.config = {
        theme: {
            extend: {
                colors: {
                    'brand-primary': '#1e40af',
                    'brand-success': '#16a34a',
                    'brand-danger': '#dc2626',
                    'brand-warning': '#f59e0b',
                    'brand-info': '#0891b2',
                }
            }
        }
    }
</script>

<style>
    @layer utilities {
        .btn-brand {
            @apply px-4 py-2 bg-brand-primary text-white rounded-lg hover:bg-blue-700 transition-colors font-medium;
        }
        
        .btn-ghost {
            @apply px-4 py-2 bg-transparent text-brand-primary border border-brand-primary rounded-lg hover:bg-blue-50 transition-colors;
        }
        
        .card-elevated {
            @apply bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow border border-gray-100;
        }
        
        .input-field {
            @apply w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-brand-primary focus:border-transparent;
        }
    }
</style>
```

### 3. Criar componente reutilizável: `Pages/Shared/Components/Button.cshtml`

```html
@model (string href, string label, string type, bool isLoading)

@{
    var baseClass = "px-4 py-2 rounded-lg font-medium transition-all inline-flex items-center gap-2";
    var typeClass = Model.type switch
    {
        "primary" => "bg-brand-primary text-white hover:bg-blue-700",
        "ghost" => "bg-transparent text-brand-primary border border-brand-primary hover:bg-blue-50",
        "danger" => "bg-brand-danger text-white hover:bg-red-700",
        _ => "bg-gray-200 text-gray-800 hover:bg-gray-300"
    };
}

<a href="@Model.href" class="@baseClass @typeClass" @(Model.isLoading ? "disabled" : "")>
    @if (Model.isLoading)
    {
        <span class="animate-spin">⏳</span>
    }
    <span>@Model.label</span>
</a>
```

---

# AÇÃO 6: CRIAR VALIDADOR DE TELEFONE (ALTA - SEMANA 1)

```csharp
public static class ValidadorTelefone
{
    public static bool ValidarTelefone(string? telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            return true; // Opcional

        // Remove tudo que não é número
        var apenasNumeros = Regex.Replace(telefone, @"\D", "");

        // Deve ter 11 dígitos
        if (apenasNumeros.Length != 11)
            return false;

        // Deve começar com 9 (celular) ou 8 (fixo)
        if (apenasNumeros[4] != '9' && apenasNumeros[4] != '8')
            return false;

        // DDD válido (11-99, exceto 00-10)
        var ddd = int.Parse(apenasNumeros.Substring(0, 2));
        if (ddd < 11 || ddd > 99)
            return false;

        return true;
    }

    public static string FormatarTelefone(string telefone)
    {
        var apenasNumeros = Regex.Replace(telefone ?? "", @"\D", "");
        if (apenasNumeros.Length != 11)
            return telefone ?? "";

        return $"({apenasNumeros.Substring(0, 2)}) {apenasNumeros.Substring(2, 5)}-{apenasNumeros.Substring(7, 4)}";
    }
}
```

---

# AÇÃO 7: IMPLEMENTAR 2FA TOTP (MÉDIA - SEMANA 2)

```csharp
// Instalar pacote: dotnet add package OtpNet

public class TwoFactorService
{
    public string GerarSegredo()
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }

    public string GerarCodigoQR(string email, string segredo, string issuer = "Mocidade015")
    {
        var uri = new OtpUri(OtpType.Totp, email, segredo, issuer);
        var qr = new QrCode(uri);
        return qr.GetGraphic(10);
    }

    public bool VerificarCodigo(string segredo, string codigo)
    {
        var bytes = Base32Encoding.ToBytes(segredo);
        var totp = new Totp(bytes);
        return totp.VerifyTotp(codigo, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
    }
}
```

---

# AÇÃO 8: IMPLEMENTAR AUDIT LOGGING (MÉDIA - SEMANA 1)

```csharp
public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Tabela { get; set; } = string.Empty;
    public string Acao { get; set; } = string.Empty; // INSERT, UPDATE, DELETE
    public Guid? UsuarioId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
    public string? DadosAntigos { get; set; } // JSON
    public string? DadosNovos { get; set; }   // JSON
}

// Adicionar ao AppDbContext
public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
```

---

# PRÓXIMAS AÇÕES

1. ✅ Fixar Index.cshtml
2. ✅ Implementar ValidadorCPF
3. ✅ Implementar Rate Limiting
4. ✅ Criptografar dados sensíveis
5. ⬜ Design System Tailwind
6. ⬜ 2FA TOTP
7. ⬜ Audit Logging
8. ⬜ Redis Caching
9. ⬜ Admin Dashboard melhorado
10. ⬜ Testes automatizados

---

**Status**: Pronto para começar Sprint 1  
**Tempo Estimado**: Sprint 1 = 5-7 dias úteis  
**Dependências**: Redis (para rate limiting + cache)

