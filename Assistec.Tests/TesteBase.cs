// AssisTec.Tests/TesteBase.cs
using AssistenciaTecnica.Api.Data;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace AssisTec.Tests;

// Infraestrutura comum a toda suíte que precisa de um AppDbContext isolado
// e do padrão de log Esperado/Obtido. Antes vivia duplicada, quase
// idêntica, em cada arquivo de teste — centralizada aqui para que uma
// mudança de formato de log ou de estratégia de banco em memória seja
// feita em um lugar só.
public abstract class TesteBase
{
    protected readonly ITestOutputHelper Output;

    protected TesteBase(ITestOutputHelper output)
    {
        Output = output;
    }

    protected static AppDbContext CriarContextoEmMemoria()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    // Chame ANTES do Assert, para o log aparecer mesmo se o teste falhar.
    protected void Log(string cenario, object esperado, object? obtido)
    {
        Output.WriteLine($"CENÁRIO : {cenario}");
        Output.WriteLine($"ESPERADO: {esperado}");
        Output.WriteLine($"OBTIDO  : {obtido}");
        Output.WriteLine(new string('-', 60));
    }
}