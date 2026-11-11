using OficinaApi.Data;
using OficinaApi.Models;

namespace OficinaApi.Services;

public class VeiculoService
{
    private readonly IVeiculoRepository _repo;

    public VeiculoService(IVeiculoRepository repo)
    {
        _repo = repo;
    }

    public Task<List<Veiculo>> ListarTodosAsync() =>
        _repo.ListarTodosAsync();

    public Task<Veiculo?> ObterPorIdAsync(string id) =>
        _repo.ObterPorIdAsync(id);

    public async Task<Veiculo> CriarAsync(Veiculo veiculo)
    {
        veiculo.Placa = veiculo.Placa.Trim().ToUpperInvariant();

        var existente = await _repo.ObterPorPlacaAsync(veiculo.Placa);
        if (existente is not null)
            throw new InvalidOperationException($"Já existe um veículo cadastrado com a placa '{veiculo.Placa}'.");

        return await _repo.CriarAsync(veiculo);
    }

    public async Task<bool> AtualizarAsync(string id, Veiculo veiculo)
    {
        veiculo.Placa = veiculo.Placa.Trim().ToUpperInvariant();

        var existenteComMesmaPlaca = await _repo.ObterPorPlacaAsync(veiculo.Placa);
        if (existenteComMesmaPlaca is not null && existenteComMesmaPlaca.Id != id)
            throw new InvalidOperationException($"A placa '{veiculo.Placa}' já pertence a outro veículo.");

        return await _repo.AtualizarAsync(id, veiculo);
    }

    public Task<bool> RemoverAsync(string id) =>
        _repo.RemoverAsync(id);
}