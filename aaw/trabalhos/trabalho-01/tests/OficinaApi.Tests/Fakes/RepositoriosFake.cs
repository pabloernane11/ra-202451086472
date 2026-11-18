using OficinaApi.Data;
using OficinaApi.Models;

namespace OficinaApi.Tests.Fakes;

/// <summary>
/// Repositórios em memória usados APENAS nos testes unitários
/// para testar os Services sem depender de rede nem alterar o MongoDB Atlas.
/// </summary>
public class FakeVeiculoRepository : IVeiculoRepository
{
    public List<Veiculo> Itens { get; } = [];
    private int _sequencia = 1;

    public Task<List<Veiculo>> ListarTodosAsync() =>
        Task.FromResult(Itens.ToList());

    public Task<Veiculo?> ObterPorIdAsync(string id) =>
        Task.FromResult(Itens.FirstOrDefault(v => v.Id == id));

    public Task<Veiculo?> ObterPorPlacaAsync(string placa) =>
        Task.FromResult(Itens.FirstOrDefault(v => v.Placa == placa));

    public Task<Veiculo> CriarAsync(Veiculo veiculo)
    {
        veiculo.Id ??= $"veic-{_sequencia++}";
        Itens.Add(veiculo);
        return Task.FromResult(veiculo);
    }

    public Task<bool> AtualizarAsync(string id, Veiculo veiculo)
    {
        var indice = Itens.FindIndex(v => v.Id == id);
        if (indice < 0) return Task.FromResult(false);

        veiculo.Id = id;
        Itens[indice] = veiculo;
        return Task.FromResult(true);
    }

    public Task<bool> RemoverAsync(string id)
    {
        var removido = Itens.RemoveAll(v => v.Id == id) > 0;
        return Task.FromResult(removido);
    }
}

public class FakeOrdemServicoRepository : IOrdemServicoRepository
{
    public List<OrdemServico> Itens { get; } = [];
    private int _sequencia = 1;

    public Task<List<OrdemServico>> ListarTodasAsync(string? veiculoId = null, string? status = null)
    {
        var consulta = Itens.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(veiculoId))
            consulta = consulta.Where(o => o.VeiculoId == veiculoId);

        if (!string.IsNullOrWhiteSpace(status))
            consulta = consulta.Where(o => o.Status == status);

        return Task.FromResult(consulta.ToList());
    }

    public Task<OrdemServico?> ObterPorIdAsync(string id) =>
        Task.FromResult(Itens.FirstOrDefault(o => o.Id == id));

    public Task<bool> ExisteParaVeiculoAsync(string veiculoId) =>
        Task.FromResult(Itens.Any(o => o.VeiculoId == veiculoId));

    public Task<OrdemServico> CriarAsync(OrdemServico ordemServico)
    {
        ordemServico.Id ??= $"os-{_sequencia++}";
        Itens.Add(ordemServico);
        return Task.FromResult(ordemServico);
    }

    public Task<bool> AtualizarAsync(string id, OrdemServico ordemServico)
    {
        var indice = Itens.FindIndex(o => o.Id == id);
        if (indice < 0) return Task.FromResult(false);

        ordemServico.Id = id;
        Itens[indice] = ordemServico;
        return Task.FromResult(true);
    }

    public Task<bool> RemoverAsync(string id)
    {
        var removido = Itens.RemoveAll(o => o.Id == id) > 0;
        return Task.FromResult(removido);
    }
}