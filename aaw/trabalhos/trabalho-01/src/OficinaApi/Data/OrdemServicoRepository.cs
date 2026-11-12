using MongoDB.Bson;
using MongoDB.Driver;
using OficinaApi.Models;

namespace OficinaApi.Data;

public interface IOrdemServicoRepository
{
    Task<List<OrdemServico>> ListarTodasAsync(string? veiculoId = null, string? status = null);
    Task<OrdemServico?> ObterPorIdAsync(string id);
    Task<bool> ExisteParaVeiculoAsync(string veiculoId);
    Task<OrdemServico> CriarAsync(OrdemServico ordemServico);
    Task<bool> AtualizarAsync(string id, OrdemServico ordemServico);
    Task<bool> RemoverAsync(string id);
}

public class OrdemServicoRepository : IOrdemServicoRepository
{
    private readonly IMongoCollection<OrdemServico> _colecao;

    public OrdemServicoRepository(IMongoDatabase database)
    {
        _colecao = database.GetCollection<OrdemServico>("ordens_servico");
    }

    public async Task<List<OrdemServico>> ListarTodasAsync(string? veiculoId = null, string? status = null)
    {
        var builder = Builders<OrdemServico>.Filter;
        var filtro = builder.Empty;

        if (!string.IsNullOrWhiteSpace(veiculoId))
        {
            if (!ObjectId.TryParse(veiculoId, out _))
                return new List<OrdemServico>();

            filtro &= builder.Eq(o => o.VeiculoId, veiculoId);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filtro &= builder.Eq(o => o.Status, status);
        }

        return await _colecao.Find(filtro).ToListAsync();
    }

    public async Task<OrdemServico?> ObterPorIdAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _colecao.Find(o => o.Id == id).FirstOrDefaultAsync();
    }

    public async Task<bool> ExisteParaVeiculoAsync(string veiculoId)
    {
        if (!ObjectId.TryParse(veiculoId, out _))
            return false;

        return await _colecao.Find(o => o.VeiculoId == veiculoId).AnyAsync();
    }

    public async Task<OrdemServico> CriarAsync(OrdemServico ordemServico)
    {
        ordemServico.Id = null;
        await _colecao.InsertOneAsync(ordemServico);
        return ordemServico;
    }

    public async Task<bool> AtualizarAsync(string id, OrdemServico ordemServico)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        ordemServico.Id = id;
        var resultado = await _colecao.ReplaceOneAsync(o => o.Id == id, ordemServico);
        return resultado.MatchedCount > 0;
    }

    public async Task<bool> RemoverAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        var resultado = await _colecao.DeleteOneAsync(o => o.Id == id);
        return resultado.DeletedCount > 0;
    }
}