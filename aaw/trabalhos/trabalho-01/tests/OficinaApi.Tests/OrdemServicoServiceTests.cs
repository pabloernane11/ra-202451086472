using OficinaApi.Models;
using OficinaApi.Services;
using OficinaApi.Tests.Fakes;
using Xunit;

namespace OficinaApi.Tests;

public class OrdemServicoServiceTests
{
    [Fact(DisplayName = "Sucesso: abrir OS calcula ValorTotal e força status Aberta")]
    public async Task CriarAsync_DadosValidos_CalculaValorTotalEForcaStatusAberta()
    {
        // Arrange
        var veiculoRepo = new FakeVeiculoRepository();
        var veiculo = await veiculoRepo.CriarAsync(new Veiculo { Placa = "ABC1D23", Marca = "Fiat", Modelo = "Uno", Ano = 2020 });
        var osRepo = new FakeOrdemServicoRepository();
        var service = new OrdemServicoService(osRepo, veiculoRepo);

        var novaOs = new OrdemServico
        {
            VeiculoId = veiculo.Id!,
            DescricaoProblema = "Troca de pastilha de freio",
            Status = "Concluida", // cliente tentando burlar o status inicial
            ValorPecas = 180.50m,
            ValorMaoDeObra = 120.00m,
            ValorTotal = 1m // cliente tentando burlar o total
        };

        // Act
        var criada = await service.CriarAsync(novaOs);

        // Assert
        Assert.Equal("Aberta", criada.Status);
        Assert.Equal(300.50m, criada.ValorTotal);
    }

    [Fact(DisplayName = "Sucesso: atualizar OS aberta recalcula ValorTotal e muda status")]
    public async Task AtualizarAsync_OsAberta_RecalculaTotalEAtualizaStatus()
    {
        // Arrange
        var veiculoRepo = new FakeVeiculoRepository();
        var veiculo = await veiculoRepo.CriarAsync(new Veiculo { Placa = "ABC1D23", Marca = "Fiat", Modelo = "Uno", Ano = 2020 });
        var osRepo = new FakeOrdemServicoRepository();
        var osExistente = await osRepo.CriarAsync(new OrdemServico
        {
            VeiculoId = veiculo.Id!,
            DescricaoProblema = "Alinhamento",
            Status = "Aberta",
            ValorPecas = 50m,
            ValorMaoDeObra = 100m,
            ValorTotal = 150m
        });

        var service = new OrdemServicoService(osRepo, veiculoRepo);
        var atualizacao = new OrdemServico
        {
            VeiculoId = veiculo.Id!,
            DescricaoProblema = "Alinhamento e balanceamento",
            Status = "EmAndamento",
            ValorPecas = 80m,
            ValorMaoDeObra = 150m
        };

        // Act
        var resultado = await service.AtualizarAsync(osExistente.Id!, atualizacao);

        // Assert
        Assert.True(resultado);
        var osSalva = await osRepo.ObterPorIdAsync(osExistente.Id!);
        Assert.Equal("EmAndamento", osSalva!.Status);
        Assert.Equal(230m, osSalva.ValorTotal);
    }

    [Fact(DisplayName = "Erro: abrir OS com valor negativo lança ArgumentException")]
    public async Task CriarAsync_ValorNegativo_LancaArgumentException()
    {
        // Arrange
        var veiculoRepo = new FakeVeiculoRepository();
        var veiculo = await veiculoRepo.CriarAsync(new Veiculo { Placa = "ABC1D23", Marca = "Fiat", Modelo = "Uno", Ano = 2020 });
        var osRepo = new FakeOrdemServicoRepository();
        var service = new OrdemServicoService(osRepo, veiculoRepo);

        var osInvalida = new OrdemServico
        {
            VeiculoId = veiculo.Id!,
            DescricaoProblema = "Teste",
            ValorPecas = -10m,
            ValorMaoDeObra = 100m
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CriarAsync(osInvalida));
    }

    [Fact(DisplayName = "Erro: atualizar OS já concluída lança InvalidOperationException")]
    public async Task AtualizarAsync_OsConcluida_LancaInvalidOperationException()
    {
        // Arrange
        var veiculoRepo = new FakeVeiculoRepository();
        var veiculo = await veiculoRepo.CriarAsync(new Veiculo { Placa = "ABC1D23", Marca = "Fiat", Modelo = "Uno", Ano = 2020 });
        var osRepo = new FakeOrdemServicoRepository();
        var osConcluida = await osRepo.CriarAsync(new OrdemServico
        {
            VeiculoId = veiculo.Id!,
            DescricaoProblema = "Troca de óleo",
            Status = "Concluida",
            ValorPecas = 100m,
            ValorMaoDeObra = 80m,
            ValorTotal = 180m
        });

        var service = new OrdemServicoService(osRepo, veiculoRepo);
        var tentativa = new OrdemServico
        {
            VeiculoId = veiculo.Id!,
            DescricaoProblema = "Tentativa de alteração",
            Status = "Aberta",
            ValorPecas = 0m,
            ValorMaoDeObra = 0m
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AtualizarAsync(osConcluida.Id!, tentativa));
    }
}