using AssistenciaTecnica.Api.Dtos;

namespace AssistenciaTecnica.Api.Services;

// Interface existe para permitir substituir a geração real por um mock
// (NSubstitute) nos testes de controller — sem ela, testar a decisão de
// status HTTP (200/404/400) exigiria rodar o QuestPDF de verdade em todo
// teste unitário do controller, acoplando duas responsabilidades que
// devem falhar por motivos diferentes e ser testadas separadamente.
public interface IOrdemServicoPdfGenerator
{
    byte[] Gerar(OrdemServicoPdfDto dados);
}