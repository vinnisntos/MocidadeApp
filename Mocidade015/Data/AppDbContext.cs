using Microsoft.EntityFrameworkCore;
using Mocidade015.Models;

namespace Mocidade015.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Onibus> Onibus => Set<Onibus>();
        public DbSet<Assento> Assentos => Set<Assento>();
        public DbSet<Acompanhante> Acompanhantes => Set<Acompanhante>();
        public DbSet<Reserva> Reservas => Set<Reserva>();
        public DbSet<ListaEspera> ListaEspera => Set<ListaEspera>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Conversor de DateTime para UTC (Postgres + Entity).
            // O Npgsql assume UTC por padrão; só normalizamos para evitar confusão de Kind.
            var utcConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v.ToUniversalTime(), DateTimeKind.Utc),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(utcConverter);
                    }
                }
            }

            // 1. USUÁRIOS
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Nome).HasColumnName("nome");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.Rg).HasColumnName("rg");
                entity.Property(e => e.Telefone).HasColumnName("telefone");
                entity.Property(e => e.SenhaHash).HasColumnName("senhahash");
                entity.Property(e => e.Role).HasColumnName("role");
                entity.Property(e => e.DataCriacao).HasColumnName("datacriacao");

                entity.HasIndex(e => e.Email).IsUnique();
            });

            // 2. ÔNIBUS
            modelBuilder.Entity<Onibus>(entity =>
            {
                entity.ToTable("Onibus");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Numero).HasColumnName("numero");
                entity.Property(e => e.TerminalSaida).HasColumnName("terminalsaida");
                entity.Property(e => e.HorarioSaida).HasColumnName("horariosaida");
                entity.Property(e => e.LotacaoMaxima).HasColumnName("lotacaomaxima");
                entity.Property(e => e.DataViagem).HasColumnName("dataviagem");
                entity.Property(e => e.Ativo).HasColumnName("ativo");

                entity.HasIndex(e => new { e.DataViagem, e.TerminalSaida });
            });

            // 3. ASSENTOS
            modelBuilder.Entity<Assento>(entity =>
            {
                entity.ToTable("Assentos");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.OnibusId).HasColumnName("onibusid");
                entity.Property(e => e.Numero).HasColumnName("numero");
                entity.Property(e => e.Ocupado).HasColumnName("ocupado");

                entity.HasIndex(e => new { e.OnibusId, e.Numero }).IsUnique();
            });

            // 4. ACOMPANHANTES
            modelBuilder.Entity<Acompanhante>(entity =>
            {
                entity.ToTable("Acompanhantes");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UsuarioResponsavelId).HasColumnName("usuarioresponsavelid");
                entity.Property(e => e.Nome).HasColumnName("nome");
                entity.Property(e => e.RgCpf).HasColumnName("rgcpf");
                entity.Property(e => e.Telefone).HasColumnName("telefone");

                entity.HasOne(a => a.UsuarioResponsavel)
                      .WithMany(u => u.Acompanhantes)
                      .HasForeignKey(a => a.UsuarioResponsavelId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 5. RESERVAS
            modelBuilder.Entity<Reserva>(entity =>
            {
                entity.ToTable("Reservas");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UsuarioId).HasColumnName("usuarioid");
                entity.Property(e => e.AcompanhanteId).HasColumnName("acompanhanteid");
                entity.Property(e => e.AssentoId).HasColumnName("assentoid");
                entity.Property(e => e.Valor).HasColumnName("valor");
                entity.Property(e => e.DataReserva).HasColumnName("datareserva");

                entity.HasOne(r => r.Usuario)
                      .WithMany(u => u.Reservas)
                      .HasForeignKey(r => r.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Assento)
                      .WithMany()
                      .HasForeignKey(r => r.AssentoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Acompanhante)
                      .WithMany()
                      .HasForeignKey(r => r.AcompanhanteId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(r => r.AssentoId).IsUnique();
            });

            // 6. LISTA DE ESPERA
            modelBuilder.Entity<ListaEspera>(entity =>
            {
                entity.ToTable("ListaEspera");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UsuarioId).HasColumnName("usuarioid");
                entity.Property(e => e.TerminalDesejado).HasColumnName("terminaldesejado");
                entity.Property(e => e.DataSolicitacao).HasColumnName("datasolicitacao");

                entity.HasOne(l => l.Usuario)
                      .WithMany()
                      .HasForeignKey(l => l.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(l => new { l.UsuarioId, l.TerminalDesejado }).IsUnique();
            });
        }
    }
}
