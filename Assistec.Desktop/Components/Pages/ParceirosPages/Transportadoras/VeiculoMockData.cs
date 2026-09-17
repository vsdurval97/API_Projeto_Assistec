namespace Assistec.Desktop.Components.Pages.ParceirosPages.Transportadoras;

public enum TipoRodado { Truck, Toco, CavaloMecanico, Van, Outros }
public enum TipoProprietarioVeiculo { Outros, TacAgregado, TacIndependente }
public enum StatusVeiculo { Ativo, Inativo }

public record VeiculoItem(
    string Codigo, string Placa, string Uf, string Modelo, int Ano, string Cor,
    string Renavam, string Chassi, TipoRodado Rodado, string Carroceria,
    double TaraKg, double CapacidadeKg, double CapacidadeM3,
    string TransportadoraCodigo, TipoProprietarioVeiculo TipoProprietario,
    StatusVeiculo Status, DateTime? UltimoMdfe)
{
    public string RodadoLabel => Rodado switch
    {
        TipoRodado.Truck => "Truck",
        TipoRodado.Toco => "Toco",
        TipoRodado.CavaloMecanico => "Cavalo Mecânico",
        TipoRodado.Van => "VAN",
        _ => "Outros"
    };

    public string TipoProprietarioLabel => TipoProprietario switch
    {
        TipoProprietarioVeiculo.TacAgregado => "TAC-Agregado",
        TipoProprietarioVeiculo.TacIndependente => "TAC-Independente",
        _ => "Outros"
    };

    public string StatusLabel => Status == StatusVeiculo.Ativo ? "Ativo" : "Inativo";
}

public static class VeiculoMockData
{
    public static readonly List<VeiculoItem> Veiculos = new()
    {
        new("VE0001", "BRA2E19", "SP", "Volvo FH 540", 2022, "Branco",
            "12345678900", "9BW958094NB123456", TipoRodado.Truck, "Fechada / Baú",
            7500, 25000, 90, "TR0001", TipoProprietarioVeiculo.Outros,
            StatusVeiculo.Ativo, new DateTime(2026, 8, 28)),

        new("VE0002", "ABC1234", "SP", "Mercedes-Benz Actros 2546", 2021, "Cinza",
            "98765432100", "9BM384686B654321", TipoRodado.Toco, "Aberta",
            6800, 22000, 80, "TR0001", TipoProprietarioVeiculo.TacAgregado,
            StatusVeiculo.Ativo, new DateTime(2026, 8, 15)),

        new("VE0003", "DEF5678", "SP", "Scania R 450", 2023, "Vermelho",
            "11122233300", "9BSR6X4004V789012", TipoRodado.Toco, "Sider",
            4200, 14000, 55, "TR0001", TipoProprietarioVeiculo.TacIndependente,
            StatusVeiculo.Inativo, new DateTime(2026, 7, 1)),

        new("VE0004", "RJG4H22", "RJ", "Volvo FH 420", 2020, "Branco",
            "55544433200", "9BW958094KB246810", TipoRodado.CavaloMecanico, "Não Aplicável",
            8200, 28000, 100, "TR0002", TipoProprietarioVeiculo.Outros,
            StatusVeiculo.Ativo, new DateTime(2026, 8, 25)),

        new("VE0005", "RJX1A33", "RJ", "Fiat Ducato Cargo", 2024, "Prata",
            "66677788800", "93YZZZFA0PJ135791", TipoRodado.Van, "Fechada / Baú",
            3500, 10000, 38, "TR0002", TipoProprietarioVeiculo.Outros,
            StatusVeiculo.Ativo, new DateTime(2026, 8, 30)),

        new("VE0006", "MGK7J55", "MG", "DAF XF 480", 2022, "Azul",
            "33322211100", "XLRTEH4A0HB369258", TipoRodado.Truck, "Graneleira",
            5900, 18000, 70, "TR0004", TipoProprietarioVeiculo.Outros,
            StatusVeiculo.Ativo, new DateTime(2026, 8, 15)),
    };
}