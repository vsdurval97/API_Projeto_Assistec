// AssisTec.Tests/OrdemServicoControllerTests.cs
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AssistenciaTecnica.Api.Controllers;
using AssistenciaTecnica.Api.Data;
using AssistenciaTecnica.Api.Dtos;
using AssistenciaTecnica.Api.Models;
using AssistenciaTecnica.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace AssisTec.Tests;

public class OrdemServicoControllerTests : TesteBase
{
    public OrdemServicoControllerTests(ITestOutputHelper output) : base(output)
    {
    }

    // -----------------------------------------------------------------------
    // Helpers específicos deste arquivo (CriarContextoEmMemoria e Log agora
    // vêm de TesteBase — ver TesteBase.cs)
    // -----------------------------------------------------------------------

    private static OrdemServicoController CriarController(AppDbContext context)
    {
        var loggerFalso = Substitute.For<ILogger<OrdemServicoController>>();
        var pdfGeneratorFalso = Substitute.For<IOrdemServicoPdfGenerator>();
        return new OrdemServicoController(context, loggerFalso, pdfGeneratorFalso);
    }

    // Simula a validação automática de ModelState que o [ApiController] faz
    // no pipeline real. Em records posicionais (C# 10+), os atributos de
    // validação ficam anexados ao PARÂMETRO do construtor primário — por
    // isso lemos via reflection nos parâmetros do construtor, e não via
    // Validator.TryValidateObject (que só enxerga atributos em propriedades).
    private static bool ValidarModelo<T>(T modelo, ControllerBase controller) where T : notnull
    {
        var tipo = typeof(T);
        var construtor = tipo.GetConstructors().First();
        var parametros = construtor.GetParameters();

        bool valido = true;

        foreach (var parametro in parametros)
        {
            var propriedade = tipo.GetProperty(parametro.Name!, BindingFlags.Public | BindingFlags.Instance);
            var valor = propriedade?.GetValue(modelo);

            var atributosValidacao = parametro
                .GetCustomAttributes(typeof(ValidationAttribute), inherit: true)
                .Cast<ValidationAttribute>();

            foreach (var atributo in atributosValidacao)
            {
                if (!atributo.IsValid(valor))
                {
                    valido = false;
                    controller.ModelState.AddModelError(
                        parametro.Name ?? string.Empty,
                        atributo.ErrorMessage ?? "Valor inválido.");
                }
            }
        }

        return valido;
    }

    private static async Task<Cliente> CriarClienteAsync(AppDbContext context, string nome, string telefone = "79900000000")
    {
        var cliente = new Cliente { Nome = nome, Telefone = telefone };
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();
        return cliente;
    }

    // A partir daqui, mantenha exatamente todos os métodos [Fact]/[Theory]
    // que já existiam no seu arquivo original — nenhum deles precisa mudar.
}