using Microsoft.EntityFrameworkCore;
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

            entity.HasOne(o => o.Cliente)
                  .WithMany(c => c.OrdensServico)
                  .HasForeignKey(o => o.ClienteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}