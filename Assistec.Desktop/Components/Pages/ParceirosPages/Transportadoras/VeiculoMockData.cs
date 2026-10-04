namespace Assistec.Desktop.Components.Pages.ParceirosPages.Transportadoras;

public enum TipoRodado { Truck, Toco, CavaloMecanico, Van, Utilitario, Outros }
public enum TipoCarroceria { NaoAplicavel, Aberta, FechadaBau, Graneleira, PortaContainer, Sider }
public enum TipoProprietarioVeiculo { TacAgregado, TacIndependente, Outros }
public enum StatusVeiculo { Ativo, Inativo }

public record VeiculoItem(
    string Codigo, string Placa, string Uf, string Marca, string Modelo, int Ano, string Cor,
    string Renavam, string Chassi, TipoRodado Rodado, TipoCarroceria Carroceria,
    double TaraKg, double CapacidadeKg, double CapacidadeM3,
    string TransportadoraCodigo, string ProprietarioNome, string ProprietarioCpfCnpj,
    TipoProprietarioVeiculo TipoProprietario, string ProprietarioUf,
    StatusVeiculo Status, DateTime? UltimoMdfe)
{
    public string RodadoCodigo => Rodado switch
    {
        TipoRodado.Truck => "01",
        TipoRodado.Toco => "02",
        TipoRodado.CavaloMecanico => "03",
        TipoRodado.Van => "04",
        TipoRodado.Utilitario => "05",
        _ => "06"
    };

    public string RodadoLabel => Rodado switch
    {
        TipoRodado.Truck => "Truck",
        TipoRodado.Toco => "Toco",
        TipoRodado.CavaloMecanico => "Cavalo Mecânico",
        TipoRodado.Van => "VAN",
        TipoRodado.Utilitario => "Utilitário",
        _ => "Outros"
    };

    public string CarroceriaCodigo => Carroceria switch
    {
        TipoCarroceria.NaoAplicavel => "00",
        TipoCarroceria.Aberta => "01",
        TipoCarroceria.FechadaBau => "02",
        TipoCarroceria.Graneleira => "03",
        TipoCarroceria.PortaContainer => "04",
        _ => "05"
    };

    public string CarroceriaLabel => Carroceria switch
    {
        TipoCarroceria.NaoAplicavel => "Não Aplicável",
        TipoCarroceria.Aberta => "Aberta",
        TipoCarroceria.FechadaBau => "Fechada / Baú",
        TipoCarroceria.Graneleira => "Graneleira",
        TipoCarroceria.PortaContainer => "Porta Container",
        _ => "Sider"
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
        new("VE0001", "BRA2E19", "SP", "Volvo", "FH 540", 2022, "Branco",
            "12345678900", "9BW958094NB123456", TipoRodado.Truck, TipoCarroceria.FechadaBau,
            7500, 25000, 90, "TR0001",
            "Rápido Paulista Transportes LTDA", "11.222.333/0001-44", TipoProprietarioVeiculo.Outros, "SP",
            StatusVeiculo.Ativo, new DateTime(2026, 8, 28)),

        new("VE0002", "ABC1234", "SP", "Mercedes-Benz", "Actros 2546", 2021, "Cinza",
            "98765432100", "9BM384686B654321", TipoRodado.Toco, TipoCarroceria.Aberta,
            6800, 22000, 80, "TR0001",
            "José Antônio da Silva", "123.456.789-00", TipoProprietarioVeiculo.TacAgregado, "SP",
            StatusVeiculo.Ativo, new DateTime(2026, 8, 15)),

        new("VE0003", "DEF5678", "SP", "Scania", "R 450", 2023, "Vermelho",
            "11122233300", "9BSR6X4004V789012", TipoRodado.Toco, TipoCarroceria.Sider,
            4200, 14000, 55, "TR0001",
            "Marcos Vinicius Pereira", "987.654.321-00", TipoProprietarioVeiculo.TacIndependente, "SP",
            StatusVeiculo.Inativo, new DateTime(2026, 7, 1)),

        new("VE0004", "RJG4H22", "RJ", "Volvo", "FH 420", 2020, "Branco",
            "55544433200", "9BW958094KB246810", TipoRodado.CavaloMecanico, TipoCarroceria.NaoAplicavel,
            8200, 28000, 100, "TR0002",
            "Logística Nacional Express S/A", "55.666.777/0001-88", TipoProprietarioVeiculo.Outros, "RJ",
            StatusVeiculo.Ativo, new DateTime(2026, 8, 25)),

        new("VE0005", "RJX1A33", "RJ", "Fiat", "Ducato Cargo", 2024, "Prata",
            "66677788800", "93YZZZFA0PJ135791", TipoRodado.Van, TipoCarroceria.FechadaBau,
            3500, 10000, 38, "TR0002",
            "Logística Nacional Express S/A", "55.666.777/0001-88", TipoProprietarioVeiculo.Outros, "RJ",
            StatusVeiculo.Ativo, new DateTime(2026, 8, 30)),

        new("VE0006", "MGK7J55", "MG", "DAF", "XF 480", 2022, "Azul",
            "33322211100", "XLRTEH4A0HB369258", TipoRodado.Truck, TipoCarroceria.Graneleira,
            5900, 18000, 70, "TR0004",
            "TransMinas Cargas ME", "22.333.444/0001-55", TipoProprietarioVeiculo.Outros, "MG",
            StatusVeiculo.Ativo, new DateTime(2026, 8, 15)),
    };
}