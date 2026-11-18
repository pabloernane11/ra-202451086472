using OficinaApi.Models;
using OficinaApi.Services;
using OficinaApi.Tests.Fakes;
using Xunit;

namespace OficinaApi.Tests;

public class VeiculoServiceTests
{
    [Fact(DisplayName = "Sucesso: criar veículo normaliza placa para maiúsculas")]
    public async Task CriarAsync_PlacaMinuscula_NormalizaPlacaESalva()
    {
        // Arrange
        var veiculoRepo = new FakeVeiculoRepository();
        var osRepo = new FakeOrdemServicoRepository();
        var service = new VeiculoService(veiculoRepo, osRepo);

        var veiculo = new Veiculo
        {
            Placa = " abc1d23 ",
            Marca = "Fiat",
            Modelo = "Uno",
            Ano = 2020,
            NomeProprietario = "Carlos Silva",
            TelefoneProprietario = "31999990000"
        };

        // Act
        var criado = await service.CriarAsync(veiculo);

        // Assert
        Assert.NotNull(criado.Id);
        Assert.Equal("ABC1D23", criado.Placa);
        Assert.Single(veiculoRepo.Itens);
    }

    [Fact(DisplayName = "Erro: criar veículo com placa duplicada lança InvalidOperationException")]
    public async Task CriarAsync_PlacaDuplicada_LancaInvalidOperationException()
    {
        // Arrange
        var veiculoRepo = new FakeVeiculoRepository();
        await veiculoRepo.CriarAsync(new Veiculo { Placa = "ABC1D23", Marca = "Fiat", Modelo = "Uno", Ano = 2020 });
        var osRepo = new FakeOrdemServicoRepository();
        var service = new VeiculoService(veiculoRepo, osRepo);

        var duplicado = new Veiculo
        {
            Placa = "abc1d23",
            Marca = "VW",
            Modelo = "Gol",
            Ano = 2021,
            NomeProprietario = "Maria",
            TelefoneProprietario = "31988887777"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CriarAsync(duplicado));
    }

    [Fact(DisplayName = "Erro: remover veículo com OS vinculada lança InvalidOperationException")]
    public async Task RemoverAsync_VeiculoComOsVinculada_LancaInvalidOperationException()
    {
        // Arrange
        var veiculoRepo = new FakeVeiculoRepository();
        var veiculo = await veiculoRepo.CriarAsync(new Veiculo { Placa = "XYZ9Z99", Marca = "GM", Modelo = "Onix", Ano = 2022 });

        var osRepo = new FakeOrdemServicoRepository();
        await osRepo.CriarAsync(new OrdemServico { VeiculoId = veiculo.Id!, DescricaoProblema = "Revisão" });

        var service = new VeiculoService(veiculoRepo, osRepo);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RemoverAsync(veiculo.Id!));
        Assert.Single(veiculoRepo.Itens); // garante que não apagou
    }
}