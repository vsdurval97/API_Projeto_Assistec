using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using AssistenciaTecnica.Api.Models;

namespace AssistenciaTecnica.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<OrdemServico> OrdensServico => Set<OrdemServico>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Nome).IsRequired().HasMaxLength(150);
            entity.Property(c => c.Telefone).IsRequired().HasMaxLength(20);
        });

        // Reafirma DateTimeKind.Utc ao LER do banco. O SQLite armazena TEXT
        // sem informação de fuso — sem isso, o Kind volta como "Unspecified"
        // e o System.Text.Json omite o sufixo "Z" na resposta JSON, fazendo
        // clientes (frontend/Swagger) interpretarem UTC como hora local.
        var conversorUtc = new ValueConverter<DateTime, DateTime>(
            paraGravar => paraGravar,
            lidoDoBanco => DateTime.SpecifyKind(lidoDoBanco, DateTimeKind.Utc));

        var conversorUtcNullable = new ValueConverter<DateTime?, DateTime?>(
            paraGravar => paraGravar,
            lidoDoBanco => lidoDoBanco.HasValue
                ? DateTime.SpecifyKind(lidoDoBanco.Value, DateTimeKind.Utc)
                : lidoDoBanco);

        modelBuilder.Entity<OrdemServico>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Marca).IsRequired().HasMaxLength(100);
            entity.Property(o => o.Modelo).IsRequired().HasMaxLength(100);
            entity.Property(o => o.DefeitoRelatado).IsRequired().HasMaxLength(500);
            entity.Property(o => o.ValorMaoDeObra).HasColumnType("decimal(10,2)");
            entity.Property(o => o.ValorPecas).HasColumnType("decimal(10,2)");

            entity.Property(o => o.TipoEquipamento).HasConversion<string>();
            entity.Property(o => o.Status).HasConversion<string>();

            entity.Property(o => o.DataAbertura).HasConversion(conversorUtc);
            entity.Property(o => o.DataConclusao).HasConversion(conversorUtcNullable);
            entity.Property(o => o.DataEntrega).HasConversion(conversorUtcNullable);

            entity.Property(o => o.UltimaModificacaoUtc)
                  .HasConversion(conversorUtc)
                  .IsConcurrencyToken();

            entity.HasOne(o => o.Cliente)
                  .WithMany(c => c.OrdensServico)
                  .HasForeignKey(o => o.ClienteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }

    // Cobre tanto o caminho síncrono quanto o assíncrono — evita que o
    // token de concorrência fique desatualizado caso algum código futuro
    // chame SaveChanges() em vez de SaveChangesAsync().
    public override int SaveChanges()
    {
        CarimbarUltimaModificacao();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        CarimbarUltimaModificacao();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void CarimbarUltimaModificacao()
    {
        var agoraUtc = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<OrdemServico>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UltimaModificacaoUtc = agoraUtc;
            }
        }
    }
}