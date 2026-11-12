using OficinaApi.Data;
using OficinaApi.Models;

namespace OficinaApi.Services;

public class OrdemServicoService
{
    private static readonly string[] StatusValidos = ["Aberta", "EmAndamento", "Concluida", "Cancelada"];

    private readonly IOrdemServicoRepository _osRepo;
    private readonly IVeiculoRepository _veiculoRepo;

    public OrdemServicoService(IOrdemServicoRepository osRepo, IVeiculoRepository veiculoRepo)
    {
        _osRepo = osRepo;
        _veiculoRepo = veiculoRepo;
    }

    public Task<List<OrdemServico>> ListarTodasAsync(string? veiculoId = null, string? status = null) =>
        _osRepo.ListarTodasAsync(veiculoId, status);

    public Task<OrdemServico?> ObterPorIdAsync(string id) =>
        _osRepo.ObterPorIdAsync(id);

    public async Task<OrdemServico> CriarAsync(OrdemServico ordemServico)
    {
        if (ordemServico.ValorPecas < 0 || ordemServico.ValorMaoDeObra < 0)
            throw new ArgumentException("Os valores de peças e mão de obra não podem ser negativos.");

        var veiculo = await _veiculoRepo.ObterPorIdAsync(ordemServico.VeiculoId);
        if (veiculo is null)
            throw new KeyNotFoundException($"Veículo com ID '{ordemServico.VeiculoId}' não encontrado.");

        ordemServico.Status = "Aberta";
        ordemServico.DataAbertura = DateTime.UtcNow;
        ordemServico.ValorTotal = ordemServico.ValorPecas + ordemServico.ValorMaoDeObra;

        return await _osRepo.CriarAsync(ordemServico);
    }

    public async Task<bool> AtualizarAsync(string id, OrdemServico ordemServico)
    {
        var atual = await _osRepo.ObterPorIdAsync(id);
        if (atual is null)
            return false;

        if (atual.Status is "Concluida" or "Cancelada")
            throw new InvalidOperationException($"A ordem de serviço já está '{atual.Status}' e não pode mais ser alterada.");

        if (ordemServico.ValorPecas < 0 || ordemServico.ValorMaoDeObra < 0)
            throw new ArgumentException("Os valores de peças e mão de obra não podem ser negativos.");

        if (!StatusValidos.Contains(ordemServico.Status))
            throw new ArgumentException($"Status '{ordemServico.Status}' inválido. Use: Aberta, EmAndamento, Concluida ou Cancelada.");

        var veiculo = await _veiculoRepo.ObterPorIdAsync(ordemServico.VeiculoId);
        if (veiculo is null)
            throw new KeyNotFoundException($"Veículo com ID '{ordemServico.VeiculoId}' não encontrado.");

        ordemServico.DataAbertura = atual.DataAbertura;
        ordemServico.ValorTotal = ordemServico.ValorPecas + ordemServico.ValorMaoDeObra;

        return await _osRepo.AtualizarAsync(id, ordemServico);
    }

    public Task<bool> RemoverAsync(string id) =>
        _osRepo.RemoverAsync(id);
}