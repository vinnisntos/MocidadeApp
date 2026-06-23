using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Mocidade015.Data;
using Mocidade015.Models.Options;
using Mocidade015.Services;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// =============================================================
// 1) CONFIGURAÇÃO — Fontes e validação de variáveis sensíveis
// =============================================================
// A connection string NUNCA fica no appsettings.json. Ela vem exclusivamente
// de env var (no Render) ou de User Secrets (em Development).
var connectionString = builder.Configuration["ConnectionStrings:SupabaseConnection"]
                       ?? Environment.GetEnvironmentVariable("ConnectionStrings__SupabaseConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string do Supabase não informada. Defina ConnectionStrings__SupabaseConnection como env var.");
}

// =============================================================
// 2) OPTIONS — Vincula seções do appsettings.json a classes tipadas
// =============================================================
builder.Services.Configure<ViagemOptions>(builder.Configuration.GetSection(ViagemOptions.Section));
builder.Services.Configure<ContatoOptions>(builder.Configuration.GetSection(ContatoOptions.Section));

// =============================================================
// 3) SERVIÇOS DE DOMÍNIO
// =============================================================
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});

builder.Services.AddScoped<IReservaService, ReservaService>();

// =============================================================
// 3.1) SERVIÇOS DE SEGURANÇA
// =============================================================
builder.Services.AddSingleton<IRateLimitService, RateLimitService>();

// =============================================================
// 4) AUTENTICAÇÃO E AUTORIZAÇÃO
// =============================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AcessoNegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(5);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.Name = "Mocidade.Auth";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
});

// =============================================================
// 5) MVC / RAZOR PAGES + Antiforgery
// =============================================================
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/App");
    options.Conventions.AuthorizeFolder("/Admin", "RequireAdminRole");
})
.AddViewOptions(options =>
{
    options.HtmlHelperOptions.ClientValidationEnabled = true;
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "Mocidade.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// =============================================================
// 6) PROXY REVERSO (Render)
// =============================================================
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Pipeline ------------------------------------------------------------
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
