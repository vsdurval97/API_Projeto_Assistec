# AssisTec API

<p align="left">
  <img src="https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet" alt=".NET 10" />
  <img src="https://img.shields.io/badge/C%23-14-239120?style=flat-square&logo=csharp" alt="C# 14" />
  <img src="https://img.shields.io/badge/EF%20Core-10-blueviolet?style=flat-square" alt="EF Core" />
  <img src="https://img.shields.io/badge/SQLite-3-003B57?style=flat-square&logo=sqlite" alt="SQLite" />
  <img src="https://img.shields.io/badge/Tests-33%2F33%20passing-success?style=flat-square" alt="Tests" />
</p>

REST API em .NET 10 para gestão de Ordens de Serviço de assistência técnica de PCs e impressoras.

## Sobre o projeto

Técnicos autônomos de manutenção de PCs e impressoras costumam controlar suas Ordens de Serviço em planilhas, caderno ou aplicativos de anotação genéricos. Esse tipo de controle não registra de forma confiável quando um equipamento entrou para conserto, quando ficou pronto e quando foi entregue — o que gera dois problemas práticos: dificuldade de cobrar prazo com precisão e ausência de histórico em caso de discussão com o cliente sobre quando algo foi combinado.

Este projeto é uma API REST local, rodando sobre SQLite, para resolver esse problema:

- Cadastro de clientes e vínculo com cada Ordem de Serviço aberta;
- Rastreio do ciclo de vida do conserto (recebido → em análise → pronto → entregue), com data registrada automaticamente em cada mudança de status;
- Cálculo do valor total do serviço (mão de obra + peças);
- Abertura de OS por nome do cliente, sem precisar saber o ID de antemão — inclusive tratando o caso de dois clientes com o mesmo nome.

## Arquitetura e decisões técnicas

### REST API com .NET 10

`Nullable Context` ativo em todo o projeto — valores possivelmente nulos precisam ser tratados explicitamente já em tempo de compilação, o que evita boa parte das `NullReferenceException` que só apareceriam em runtime.

### SQLite

Escolha deliberada para o contexto de uso: um técnico rodando a aplicação no próprio computador de trabalho, sem servidor de banco de dados dedicado.

- O banco inteiro é um arquivo `.db` único — copiar, mover ou fazer backup é copiar um arquivo.
- Não exige processo de banco rodando em segundo plano nem consumo de memória adicional para manter um servidor de banco ativo.
- O volume de escrita de uma oficina de manutenção não chega perto do que justificaria um SGBD cliente-servidor. Mesmo assim, os riscos reais de concorrência do SQLite (lock de escrita, ausência nativa de token de concorrência) são tratados explicitamente no código — não ignorados por conveniência.

### Camada de DTOs

Nenhuma entidade do EF Core é exposta diretamente pela API. Toda entrada e saída passa por um DTO próprio (`CriarOrdemServicoDto`, `OrdemServicoResponseDto`, `CriarClienteDto`, etc.):

- A entidade pode mudar de estrutura internamente sem quebrar o contrato público da API.
- `Cliente` e `OrdemServico` têm referência bidirecional (`Cliente.OrdensServico` e `OrdemServico.Cliente`), o que geraria um ciclo de serialização JSON se as entidades fossem retornadas diretamente. O DTO de resposta expõe só o necessário (`ClienteNome`, por exemplo, sem carregar de volta a lista de OS daquele cliente).

O mapeamento entre entidade e DTO é manual, sem biblioteca de mapeamento automático — decisão para manter o fluxo de dados explícito e fácil de acompanhar.

## Regras de negócio

### Resolução de cliente por nome

A criação de uma OS aceita `ClienteId` ou `ClienteNome`. Na prática, o técnico geralmente sabe o nome do cliente, não o ID interno do banco.

| Situação | Resposta |
|---|---|
| Nem `ClienteId` nem `ClienteNome` informados | `400 Bad Request` |
| `ClienteId` informado | usado diretamente (tem prioridade sobre `ClienteNome`) |
| `ClienteNome` informado, um único cliente encontrado | OS criada, ID resolvido automaticamente |
| `ClienteNome` informado, nenhum cliente encontrado | `404 Not Found` |
| `ClienteNome` informado, mais de um cliente com o mesmo nome | `400 Bad Request` com a lista dos candidatos (`Id`, `Nome`, `Telefone`), para o cliente da API decidir e refazer a chamada com o `ClienteId` correto |

A busca por nome ignora diferença de maiúsculas/minúsculas e acentuação (`"jose da costa"` encontra `"José da Costa"`). Essa comparação é feita em memória, porque o `LOWER()` nativo do SQLite não normaliza acentos.

### Ciclo de status da Ordem de Serviço

```
Recebido → EmAnalise → Pronto → Entregue
              ↑            ↓
              └────────────┘
     (Entregue é estado final, sem regressão possível)
```

A cada transição, a entidade `OrdemServico` carimba a data correspondente (método `AtualizarStatus`):

- `Pronto` preenche `DataConclusao`;
- `Entregue` preenche `DataEntrega` (e `DataConclusao`, caso ainda estivesse vazia);
- `DataAbertura` é definida uma única vez, na criação, e não muda depois — mesmo que o status regrida de `EmAnalise` para `Recebido`.

O controller decide se a transição de status é permitida; a entidade decide o que acontece quando ela ocorre.

## Pontos de atenção resolvidos durante o desenvolvimento

Alguns problemas de concorrência e consistência de dados só aparecem sob uso real, não em teste manual sequencial. Vale registrar os principais, porque moldaram boa parte do desenho atual:

- **Fuso horário**: as datas da entidade usam `DateTime.UtcNow`, não `DateTime.Now`. SQLite armazena `DateTime` como texto puro, sem informação de fuso — gravar hora local gera inconsistência entre registros feitos em horários de verão ou fusos diferentes. Como o SQLite também não preserva o `DateTimeKind` na leitura, foi adicionado um `ValueConverter` no EF Core que reafirma `Kind = Utc` ao carregar o dado do banco, para o JSON de resposta sair corretamente com o indicador de UTC.
- **Concorrência otimista**: duas requisições alterando a mesma OS ao mesmo tempo não geravam nenhum erro — a última a salvar simplesmente sobrescrevia a outra. Foi adicionado um token de concorrência (`UltimaModificacaoUtc`) na entidade, para que o EF Core detecte esse conflito e retorne `409 Conflict` em vez de perder a alteração silenciosamente.
- **`busy_timeout` do SQLite**: precisa estar na connection string, não em uma `PRAGMA` executada avulsa — SQLite permite um único escritor por vez, e sem esse timeout configurado por conexão, requisições concorrentes podem falhar direto com "database is locked".
- **Enum ausente no payload**: `[Required]` do `DataAnnotations` não cobre o caso de uma propriedade simplesmente não vir no JSON — nesse caso, o desserializador atribui o valor `0` do enum por padrão, o que podia disparar uma transição de status não pedida. Corrigido com `[JsonRequired]`, que valida a ausência do campo já na desserialização.
- **Consultas de leitura** (`ListarTodas`, `BuscarPorId`, `ListarTodos`) usam `.AsNoTracking()` — não há necessidade de o EF Core rastrear mudanças em dados que só serão lidos e devolvidos na resposta.

## Testes

33 testes automatizados com xUnit e NSubstitute, usando `Microsoft.EntityFrameworkCore.InMemory` para isolar cada teste em um banco próprio.

| Área | O que é coberto |
|---|---|
| Criação de OS | `201 Created`, cálculo correto de `ValorTotal`, rejeição de `ClienteId` inexistente |
| Validação de borda | valores negativos em mão de obra/peças, campos obrigatórios vazios ou nulos |
| Busca por ID | `404 Not Found` para IDs inexistentes, em Cliente e em OS |
| Máquina de estados | bloqueio de transição a partir de `Entregue`, bloqueio de regressão indevida |
| Datas do ciclo de vida | `DataConclusao` e `DataEntrega` carimbadas corretamente, dentro de uma janela de tolerância |
| Resolução de cliente por nome | nome único, nome ambíguo, nome inexistente, prioridade quando `ClienteId` e `ClienteNome` vêm juntos |
| CRUD de Cliente | criação, busca e atualização, com persistência confirmada por consulta independente |

Cada teste segue Arrange/Act/Assert e loga o valor esperado e o valor obtido em cada asserção (via `ITestOutputHelper`), para facilitar diagnóstico quando algo falha.

```bash
dotnet test
```
```
Resumo do teste: total: 33; falhou: 0; bem-sucedido: 33; ignorado: 0
```

## Como executar

Pré-requisito: [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
# 1. Clonar o repositório
git clone https://github.com/vsdurval97/API_Projeto_Assistec.git
cd API_Projeto_Assistec

# 2. Instalar a ferramenta de migrations do EF Core, se ainda não tiver
dotnet tool install --global dotnet-ef

# 3. Restaurar dependências (usa a solução Assistec.slnx, cobrindo API e testes)
dotnet restore

# 4. Aplicar as migrations e criar o banco SQLite local
cd Assistec
dotnet ef database update
cd ..

# 5. Rodar a suíte de testes
dotnet test

# 6. Executar a API
cd Assistec
dotnet run
```

O passo 4 cria o arquivo `assistencia.db` dentro da pasta `Assistec/`, já com o schema completo.

Com a API rodando em ambiente de desenvolvimento, a documentação interativa fica disponível em:

```
http://localhost:<porta-exibida-no-terminal>/swagger
```

## Roadmap

- Frontend próprio para consumir a API — o Swagger cobre o desenvolvimento e testes manuais, mas não é uma interface para uso no dia a dia da oficina
- Emissão de recibo em PDF para o cliente ao concluir a Ordem de Serviço
- Emissão da Ordem de Serviço em PDF, para impressão no momento do recebimento do equipamento
- Testes de integração com `WebApplicationFactory`, validando o pipeline HTTP real de ponta a ponta
- Endpoint de filtro de Ordens de Serviço por status e por cliente
- Paginação nas listagens

## Licença

Projeto pessoal, disponível para estudo e referência técnica.
