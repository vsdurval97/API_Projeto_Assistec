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
    entity.Property(c => c.Documento).HasMaxLength(20);
    entity.Property(c => c.Email).HasMaxLength(150);
    entity.Property(c => c.InscricaoEstadual).HasMaxLength(20);

    // Convertidos para string, mesmo padrão já usado em TipoEquipamento e
    // StatusOrdemServico — grava o nome legível no banco, não o índice
    // numérico do enum. Evita que uma futura reordenação acidental dos
    // valores do enum corrompa dado silenciosamente já persistido.
    entity.Property(c => c.TipoPessoa).HasConversion<string>().HasMaxLength(20);
    entity.Property(c => c.IndicadorInscricaoEstadual).HasConversion<string>().HasMaxLength(30);

    // OwnsOne mapeia Endereco nas MESMAS colunas da tabela Clientes (não
    // cria tabela separada) — reflete que é parte do cliente, não uma
    // entidade com ciclo de vida próprio. Navigation.IsRequired(false)
    // é o que permite Cliente.Endereco ficar null.
    entity.OwnsOne(c => c.Endereco, endereco =>
    {
        endereco.Property(e => e.Cep).HasMaxLength(8).HasColumnName("EnderecoCep");
        endereco.Property(e => e.Logradouro).HasMaxLength(200).HasColumnName("EnderecoLogradouro");
        endereco.Property(e => e.Numero).HasMaxLength(20).HasColumnName("EnderecoNumero");
        endereco.Property(e => e.Complemento).HasMaxLength(100).HasColumnName("EnderecoComplemento");
        endereco.Property(e => e.Bairro).HasMaxLength(100).HasColumnName("EnderecoBairro");
        endereco.Property(e => e.Municipio).HasMaxLength(100).HasColumnName("EnderecoMunicipio");
        endereco.Property(e => e.Uf).HasMaxLength(2).HasColumnName("EnderecoUf");
        endereco.Property(e => e.CodigoMunicipioIbge).HasMaxLength(7).HasColumnName("EnderecoCodigoMunicipioIbge");
        endereco.Property(e => e.CodigoPais).HasMaxLength(4).HasColumnName("EnderecoCodigoPais");
        endereco.Property(e => e.Pais).HasMaxLength(50).HasColumnName("EnderecoPais");
    });

    entity.Navigation(c => c.Endereco).IsRequired(false);
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