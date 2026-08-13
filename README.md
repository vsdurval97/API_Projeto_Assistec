# AssisTec API

<p align="left">
  <img src="https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet" alt=".NET 10" />
  <img src="https://img.shields.io/badge/C%23-14-239120?style=flat-square&logo=csharp" alt="C# 14" />
  <img src="https://img.shields.io/badge/EF%20Core-10-blueviolet?style=flat-square" alt="EF Core" />
  <img src="https://img.shields.io/badge/SQLite-3-003B57?style=flat-square&logo=sqlite" alt="SQLite" />
  <img src="https://img.shields.io/badge/Tests-33%2F33%20passing-success?style=flat-square" alt="Tests" />
</p>

REST API em .NET 10 para gestão de Ordens de Serviço de assistência técnica de PCs e impressoras.

Sobre o projeto

Técnicos autônomos de manutenção de PCs e impressoras costumam controlar suas Ordens de Serviço em planilhas, caderno ou aplicativos de anotação genéricos. Esse tipo de controle não registra de forma confiável quando um equipamento entrou para conserto, quando ficou pronto e quando foi entregue — o que gera dois problemas práticos: dificuldade de cobrar prazo com precisão e ausência de histórico em caso de discussão com o cliente sobre quando algo foi combinado.

Este projeto é uma API REST local, rodando sobre SQLite, para resolver esse problema:

Cadastro de clientes (com dados fiscais e endereço opcionais, ver seção própria abaixo) e vínculo com cada Ordem de Serviço aberta;
Rastreio do ciclo de vida do conserto (recebido → em análise → pronto → entregue), com data registrada automaticamente em cada mudança de status;
Cálculo do valor total do serviço (mão de obra + peças);
Abertura de OS por nome do cliente, sem precisar saber o ID de antemão — inclusive tratando o caso de dois clientes com o mesmo nome;
Geração da Ordem de Serviço em PDF, pronta para impressão no momento do atendimento.
Arquitetura e decisões técnicas
REST API com .NET 10

Nullable Context ativo em todo o projeto — valores possivelmente nulos precisam ser tratados explicitamente já em tempo de compilação, o que evita boa parte das NullReferenceException que só apareceriam em runtime.

SQLite

Escolha deliberada para o contexto de uso: um técnico rodando a aplicação no próprio computador de trabalho, sem servidor de banco de dados dedicado.

O banco inteiro é um arquivo .db único — copiar, mover ou fazer backup é copiar um arquivo.
Não exige processo de banco rodando em segundo plano nem consumo de memória adicional para manter um servidor de banco ativo.
O volume de escrita de uma oficina de manutenção não chega perto do que justificaria um SGBD cliente-servidor. Mesmo assim, os riscos reais de concorrência do SQLite (lock de escrita, ausência nativa de token de concorrência) são tratados explicitamente no código — não ignorados por conveniência.
Camada de DTOs

Nenhuma entidade do EF Core é exposta diretamente pela API. Toda entrada e saída passa por um DTO próprio (CriarOrdemServicoDto, OrdemServicoResponseDto, CriarClienteDto, etc.):

A entidade pode mudar de estrutura internamente sem quebrar o contrato público da API.
Cliente e OrdemServico têm referência bidirecional (Cliente.OrdensServico e OrdemServico.Cliente), o que geraria um ciclo de serialização JSON se as entidades fossem retornadas diretamente. O DTO de resposta expõe só o necessário (ClienteNome, por exemplo, sem carregar de volta a lista de OS daquele cliente).

O mapeamento entre entidade e DTO é manual, sem biblioteca de mapeamento automático — decisão para manter o fluxo de dados explícito e fácil de acompanhar.

Regra de negócio vive na entidade, não no controller

OrdemServico concentra tanto o comportamento (AtualizarStatus) quanto a regra de quais transições de status são válidas (TryObterTransicoesPermitidas). As duas coisas viviam originalmente separadas — a regra dentro do controller, o comportamento na entidade — o que criava risco de dessincronização: alguém editar uma sem lembrar da outra. Hoje o controller só orquestra (decide o status HTTP para cada resultado); a entidade é a única fonte de verdade sobre o próprio ciclo de vida.

Na mesma linha, a normalização de texto usada para buscar cliente por nome (NormalizadorTexto.RemoverAcentosEMinusculas) foi extraída para Helpers/, por ser uma função pura sem dependência de HTTP — reutilizável e testável isoladamente, sem precisar instanciar um controller inteiro para testar uma comparação de string.

Geração de PDF (QuestPDF)

A Ordem de Serviço pode ser exportada em PDF (GET /api/OrdemServico/{id}/pdf), pronta para impressão no balcão. A camada é dividida em três responsabilidades que falham por motivos diferentes, e por isso são testadas separadamente:

OrdemServicoPdfDto — view model dedicado, só com strings já formatadas (moeda, data, documento, telefone). O motor de PDF não conhece regra de formatação nem CultureInfo, só desenha o que recebe.
OrdemServicoPdfGenerator — o layout em si, estruturado em métodos privados por seção (cabeçalho, cliente, equipamento, defeito, datas, valores), usando containers que crescem com o conteúdo em vez de altura fixa — evita LayoutException quando o defeito relatado é um texto longo.
IOrdemServicoPdfGenerator como interface — permite substituir a geração real por um mock nos testes do controller, para que a decisão de status HTTP (200/404/400) seja testada sem depender do QuestPDF renderizar de verdade.
Dados fiscais e endereço do Cliente

Cliente tem campos opcionais alinhados ao leiaute de destinatário exigido pela SEFAZ para NF-e/NFC-e (TipoPessoa, IndicadorInscricaoEstadual, InscricaoEstadual, Documento, Email, Endereco). Nenhum é obrigatório — a decisão foi deixar a estrutura pronta para uma eventual camada de emissão fiscal, sem forçar o cadastro rápido de balcão a virar um formulário longo hoje.

O endereço é preenchido automaticamente a partir do CEP (via ViaCEP), campo a campo, não tudo-ou-nada:

Municipio, Uf e CodigoMunicipioIbge vêm sempre da consulta, quando ela funciona — são garantidos pela faixa do CEP.
Logradouro e Bairro só são sobrescritos se a API de fato devolver algo — cidades com CEP único para todo o município (caso real, não hipotético: Estância/SE) retornam esses campos vazios, e o que o atendente já tiver digitado manualmente é preservado.
Numero e Complemento nunca vêm de nenhuma API de CEP, em nenhuma cidade — são sempre digitação manual.
Falha na consulta (CEP inexistente, API fora do ar, timeout) nunca bloqueia o cadastro — o cliente é salvo com os dados que o atendente informou, sem o complemento automático.

CodigoMunicipioIbge não é aceito como entrada do cliente da API — é sempre resolvido pelo servidor, nunca informado diretamente, para evitar divergência entre o código e o endereço real.

Regras de negócio
Resolução de cliente por nome

A criação de uma OS aceita ClienteId ou ClienteNome. Na prática, o técnico geralmente sabe o nome do cliente, não o ID interno do banco.

Situação	Resposta
Nem ClienteId nem ClienteNome informados	400 Bad Request
ClienteId informado	usado diretamente (tem prioridade sobre ClienteNome)
ClienteNome informado, um único cliente encontrado	OS criada, ID resolvido automaticamente
ClienteNome informado, nenhum cliente encontrado	404 Not Found
ClienteNome informado, mais de um cliente com o mesmo nome	400 Bad Request com a lista dos candidatos (Id, Nome, Telefone), para o cliente da API decidir e refazer a chamada com o ClienteId correto

A busca por nome ignora diferença de maiúsculas/minúsculas e acentuação ("jose da costa" encontra "José da Costa"). Essa comparação é feita em memória, porque o LOWER() nativo do SQLite não normaliza acentos.

Ciclo de status da Ordem de Serviço
Recebido → EmAnalise → Pronto → Entregue
              ↑            ↓
              └────────────┘
     (Entregue é estado final, sem regressão possível)

A cada transição, a entidade OrdemServico carimba a data correspondente (método AtualizarStatus):

Pronto preenche DataConclusao;
Entregue preenche DataEntrega (e DataConclusao, caso ainda estivesse vazia — salvaguarda para quando o método é chamado diretamente, pulando o fluxo normal);
DataAbertura é definida uma única vez, na criação, e não muda depois — mesmo que o status regrida de EmAnalise para Recebido.

Um status fora do mapa de transições (dado corrompido, ou um valor de enum novo que não foi cadastrado) não derruba a API com uma exceção não tratada — TryObterTransicoesPermitidas retorna false de forma explícita nesse caso, e o controller responde com 500 tratado em vez de vazar um KeyNotFoundException.

Pontos de atenção resolvidos durante o desenvolvimento

Alguns problemas de concorrência e consistência de dados só aparecem sob uso real, não em teste manual sequencial. Vale registrar os principais, porque moldaram boa parte do desenho atual:

Fuso horário: as datas da entidade usam DateTime.UtcNow, não DateTime.Now. SQLite armazena DateTime como texto puro, sem informação de fuso — gravar hora local gera inconsistência entre registros feitos em horários de verão ou fusos diferentes. Como o SQLite também não preserva o DateTimeKind na leitura, foi adicionado um ValueConverter no EF Core que reafirma Kind = Utc ao carregar o dado do banco, para o JSON de resposta sair corretamente com o indicador de UTC.
Concorrência otimista: duas requisições alterando a mesma OS ao mesmo tempo não geravam nenhum erro — a última a salvar simplesmente sobrescrevia a outra. Foi adicionado um token de concorrência (UltimaModificacaoUtc) na entidade, para que o EF Core detecte esse conflito e retorne 409 Conflict em vez de perder a alteração silenciosamente. Validado com um teste de integração que força a corrida com dois DbContext reais disputando a mesma linha.
busy_timeout do SQLite: precisa estar na connection string, não em uma PRAGMA executada avulsa — SQLite permite um único escritor por vez, e sem esse timeout configurado por conexão, requisições concorrentes podem falhar direto com "database is locked".
Enum ausente no payload: [Required] do DataAnnotations não cobre o caso de uma propriedade simplesmente não vir no JSON — nesse caso, o desserializador atribui o valor 0 do enum por padrão, o que podia disparar uma transição de status não pedida. Corrigido com [JsonRequired], que valida a ausência do campo já na desserialização.
Consultas de leitura (ListarTodas, BuscarPorId, ListarTodos) usam .AsNoTracking() — não há necessidade de o EF Core rastrear mudanças em dados que só serão lidos e devolvidos na resposta.
Escopo de segurança

Duas decisões deliberadas, coerentes com o uso pretendido (um técnico rodando a API no próprio computador, sem exposição a rede externa):

Sem autenticação/autorização: nenhum endpoint exige token ou credencial. Faz sentido para uso estritamente local; se a API algum dia precisar ser exposta além de localhost (rede doméstica, deploy em servidor), autenticação passa a ser obrigatória antes disso acontecer.
Sem redirecionamento HTTPS (UseHttpsRedirection desativado no Program.cs): como cliente e servidor rodam no mesmo processo/máquina, não há rede entre os dois para um HTTPS local proteger de fato. Manter desativado evita a fricção de gerenciar certificado de desenvolvimento (dotnet dev-certs) sem ganho de segurança real neste cenário.

Nenhuma das duas decisões deve ser copiada para um contexto onde a API seja acessível por outras máquinas.

Testes

121 testes automatizados, organizados em duas camadas com propósitos diferentes:

Testes de unidade (xUnit + NSubstitute + Microsoft.EntityFrameworkCore.InMemory) — chamam o controller diretamente, isolando cada teste em um banco em memória próprio. Cobrem lógica de negócio de forma rápida, mas pulam o pipeline HTTP real (não passam pelo ModelState do [ApiController] nem pela serialização JSON de verdade).

Área	O que é coberto
Criação de OS	201 Created, cálculo correto de ValorTotal, rejeição de ClienteId inexistente
Validação de borda	valores negativos em mão de obra/peças, campos obrigatórios vazios ou nulos
Busca por ID	404 Not Found para IDs inexistentes, em Cliente e em OS
Máquina de estados	bloqueio de transição a partir de Entregue, bloqueio de regressão indevida, status desconhecido tratado sem exceção
Datas do ciclo de vida	DataConclusao e DataEntrega carimbadas corretamente, dentro de uma janela de tolerância
Resolução de cliente por nome	nome único, nome ambíguo, nome inexistente, prioridade quando ClienteId e ClienteNome vêm juntos
CRUD de Cliente	criação, busca e atualização, com persistência confirmada por consulta independente
NormalizadorTexto (isolado)	acentuação, caixa alta/baixa, strings vazias, nomes diferentes não colidindo
FormatadorDados (isolado)	máscara de CPF/CNPJ, telefone fixo/celular, moeda e data em pt-BR, sempre sem lançar exceção em dado ausente
OrdemServicoPdfDto	cálculo do total, propagação dos dados do cliente, cliente nulo e valores negativos bloqueando a geração
OrdemServicoPdfGenerator (QuestPDF real)	PDF não vazio com assinatura binária correta, texto de defeito longo sem LayoutException
Geração de PDF no controller	400/404/200 decididos sem depender do QuestPDF renderizar de verdade (gerador mockado)
CepLocalizadorService (isolado)	CEP completo, CEP genérico (Estância/SE), CEP inexistente, formato inválido, erro de rede — nunca lança exceção
Resolução de endereço por CEP	merge campo a campo: dado da API tem prioridade quando existe, dado digitado manualmente é preservado quando a API retorna vazio

Testes de integração (WebApplicationFactory + SQLite real, não InMemory) — sobem a API inteira via TestServer e testam via HTTP de fato, contra o mesmo provider de banco que roda em produção. Existem especificamente para fechar o que os testes de unidade não alcançam:

Área	O que é coberto
ModelState real do [ApiController]	validação de [Required], [Range], [JsonRequired] acontecendo de fato na desserialização, não simulada
Serialização de datas	JSON de resposta sai com sufixo Z (UTC) após round-trip real pelo SQLite
Concorrência otimista real	dois DbContext disputando a mesma linha geram DbUpdateConcurrencyException de verdade, contra o provider SQLite
CRUD de Cliente via HTTP	os quatro endpoints testados de ponta a ponta, incluindo os casos de validação
Geração de PDF via HTTP	200 com Content-Type: application/pdf e corpo não vazio, 404 para OS inexistente, 400 para id inválido
Resolução de CEP via HTTP	endereço completo, CEP genérico preservando dado manual, cliente criado normalmente sem endereço informado

A resolução de CEP nos testes de integração usa um FakeCepLocalizadorService registrado na CustomWebApplicationFactory, no lugar do ICepLocalizadorService real — a suíte nunca depende do ViaCEP estar no ar, e o resultado não muda se o dado de um CEP real for atualizado no futuro.

Cada teste segue Arrange/Act/Assert e loga o valor esperado e o valor obtido em cada asserção (via ITestOutputHelper), para facilitar diagnóstico quando algo falha.

bash
dotnet test
Resumo do teste: total: 121; falhou: 0; bem-sucedido: 121; ignorado: 0
Como executar

Pré-requisito: .NET 10 SDK.

bash
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

O passo 4 cria o arquivo assistencia.db dentro da pasta Assistec/, já com o schema completo.

Com a API rodando em ambiente de desenvolvimento, a documentação interativa fica disponível em:

http://localhost:<porta-exibida-no-terminal>/swagger

Roadmap
Frontend próprio para consumir a API — o Swagger cobre o desenvolvimento e testes manuais, mas não é uma interface para uso no dia a dia da oficina
Emissão de recibo em PDF para o cliente ao concluir a Ordem de Serviço (a OS em si já é gerada em PDF)
Validação de dígito verificador de CPF/CNPJ — hoje FormatadorDados só aplica máscara, não confirma se o documento é matematicamente válido
Validação de que o CodigoMunicipioIbge retornado pelo CEP é coerente com a Uf informada manualmente pelo usuário, para o caso de o CEP não ser encontrado
Token de concorrência otimista também em Cliente (hoje só existe em OrdemServico)
Endpoint de filtro de Ordens de Serviço por status e por cliente
Paginação nas listagens

Os campos fiscais e o preenchimento automático de endereço (seção "Dados fiscais e endereço do Cliente", acima) já existem na estrutura do projeto, mas nenhuma feature de emissão fiscal (XML, comunicação com SEFAZ, DANFE) está planejada neste roadmap — a infraestrutura foi deixada pronta para não exigir retrabalho, caso essa direção seja decidida no futuro.

Licença

Projeto pessoal, disponível para estudo e referência técnica.